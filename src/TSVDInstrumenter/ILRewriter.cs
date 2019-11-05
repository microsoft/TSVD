// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace TSVDInstrumenter
{
    /// <summary>
    /// ILRewriter class.
    /// </summary>
    public class ILRewriter
    {
        private readonly MethodDefinition interceptionMethod;

        private readonly InstrumentationConfiguration configuration;
        private ModuleDefinition tsvdRuntimeModule;

        /// <summary>
        /// Initializes a new instance of the <see cref="ILRewriter"/> class.
        /// </summary>
        /// <param name="configuration">Instrumentation Configuration.</param>
        public ILRewriter(InstrumentationConfiguration configuration)
        {
            this.configuration = configuration;

            string cwd = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string runtimeAssembly = Path.Combine(cwd, Constants.TSVDRuntimeLibraryFile);
            this.tsvdRuntimeModule = ModuleDefinition.ReadModule(runtimeAssembly);
            this.interceptionMethod = this.tsvdRuntimeModule
                .Types.Single(t => t.Name == Constants.InterceptorType)
                .Methods.Single(x => x.Name == Constants.OnStartMethod).Resolve();
        }

        /// <summary>
        /// Instrument an assemblly to intercept just before the calls of thread-unsafe APIs.
        /// </summary>
        /// <param name="assemblyPath">assembly path.</param>
        /// <returns>Instrumentatin result.</returns>
        public InstrumentationResult RewriteThreadSafetyAPIs(string assemblyPath)
        {
            AssemblyDefinition assembly = null;
            InstrumentationResult preInstrumentationResult = this.CheckAndLoadAssembly(assemblyPath, out assembly);
            if (preInstrumentationResult != InstrumentationResult.OK)
            {
                return preInstrumentationResult;
            }

            bool instrumented = false;
            foreach (var module in assembly.Modules)
            {
                bool isMixedMode = (module.Attributes & ModuleAttributes.ILOnly) == 0;
                if (isMixedMode)
                {
                    return InstrumentationResult.SKIPPED_MixedModeAssembly;
                }

                var interceptionMethodRef = module.ImportReference(this.interceptionMethod);

                var objectType = module.ImportReference(typeof(object));
                var voidType = module.ImportReference(typeof(void));
                var stackType = module.ImportReference(typeof(Stack<object>));

                foreach (var type in module.GetAllTypes())
                {
                    var allMethods = new List<MethodDefinition>();
                    var staticConstructor = type.GetStaticConstructor();
                    if (staticConstructor != null && staticConstructor.HasBody)
                    {
                        allMethods.Add(staticConstructor);
                    }

                    allMethods.AddRange(type.GetConstructors().Where(x => x != null && x.HasBody));
                    allMethods.AddRange(type.Methods.Where(x => x.HasBody));

                    foreach (var method in allMethods)
                    {
                        bool instrumentedMethod = false;
                        ILProcessor ilProcessor = method.Body.GetILProcessor();
                        method.Body.SimplifyMacros();
                        foreach (var instruction in method.Body.Instructions.ToList())
                        {
                            if (instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt)
                            {
                                var methodReference = (MethodReference)instruction.Operand;
                                var methodSignature = this.MethodSignatureWithoutReturnType(methodReference.FullName);
                                if (this.configuration.IsThreadSafetyAPI(methodSignature))
                                {
                                    bool hasThis = ((MethodReference)instruction.Operand).HasThis;
                                    VariableDefinition instanceVarDef = null;
                                    VariableDefinition stackVarDefinition = null;
                                    if (hasThis)
                                    {
                                        instanceVarDef = new VariableDefinition(objectType);
                                        stackVarDefinition = new VariableDefinition(stackType);
                                        method.Body.Variables.Add(instanceVarDef);
                                        method.Body.Variables.Add(stackVarDefinition);
                                        var loadThisInstruction = this.LocateLoadThisInstruction(instruction);
                                        var storeThisInstruction = ilProcessor.Create(OpCodes.Stloc, instanceVarDef);
                                        ilProcessor.InsertAfter(loadThisInstruction, storeThisInstruction);  // store "this" to a variable
                                        ilProcessor.InsertAfter(storeThisInstruction, ilProcessor.Create(OpCodes.Ldloc, instanceVarDef));  // load "this"
                                    }

                                    MethodReference apiReference = (MethodReference)instruction.Operand;

                                    List<Instruction> patch = this.InterceptionPatch(
                                        ilProcessor,
                                        hasThis,
                                        instanceVarDef,
                                        interceptionMethodRef,
                                        this.MethodSignatureWithoutReturnType(method.FullName),
                                        this.MethodSignatureWithoutReturnType(apiReference.FullName),
                                        instruction.Offset);
                                    patch.ForEach(x => ilProcessor.InsertBefore(instruction, x));
                                    instrumented = true;
                                    instrumentedMethod = true;
                                }
                            }
                        }

                        method.Body.OptimizeMacros();
                        if (instrumentedMethod)
                        {
                            method.Body.InitLocals = true;
                        }
                    }
                }
            }

            try
            {
                assembly.Write();
            }
            catch
            {
                return InstrumentationResult.ERROR_Other;
            }

            return instrumented ? InstrumentationResult.OK : InstrumentationResult.SKIPPED_NothingToInstrument;
        }

        private InstrumentationResult CheckAndLoadAssembly(string assemblyPath, out AssemblyDefinition assembly)
        {
            assembly = null;
            string assemblyName = Path.GetFileName(assemblyPath);

            if (!File.Exists(assemblyPath))
            {
                return InstrumentationResult.ERROR_FileNotFound;
            }

            if (!this.configuration.IsAssemblyMatch(assemblyPath))
            {
                return InstrumentationResult.SKIPPED_NotATarget;
            }

            try
            {
                assembly = AssemblyDefinition.ReadAssembly(assemblyPath, new ReaderParameters { ReadWrite = true });
            }
            catch
            {
                assembly = null;
            }

            if (assembly == null)
            {
                return InstrumentationResult.ERROR_CannotLoadAssembly;
            }

            if ((assembly.MainModule.Attributes & ModuleAttributes.StrongNameSigned) != 0)
            {
                assembly.Dispose();
                return InstrumentationResult.SKIPPED_SignedAssembly;
            }

            bool alreadyInstrumented = assembly.MainModule.AssemblyReferences.Any(x => x.Name == Constants.TSVDRuntimeLibrary);
            if (alreadyInstrumented)
            {
                assembly.Dispose();
                return InstrumentationResult.SKIPPED_AlreadyInstrumented;
            }

            return InstrumentationResult.OK;
        }

        /// <summary>
        /// This method starts from an API call instruction and goes backward to locate the instruction that
        /// pushes the "this" object into stack. We insert out instrumentation patch at that point so that
        /// the interception method can access the "this" object. After the interception method ends, we
        /// push the "this" object to stack again so that the original API call can continue.
        /// </summary>
        /// <param name="callInstruction">Instruction that calls the API.</param>
        /// <returns>The instruction that pushes the "this" object into stack.</returns>
        private Instruction LocateLoadThisInstruction(Instruction callInstruction)
        {
            Tuple<int, int> poppedPushed = MSILHelper.GetStackTransition(callInstruction);
            int toPop = poppedPushed.Item1;

            Instruction current = callInstruction.Previous;
            while (toPop > 1)
            {
                poppedPushed = MSILHelper.GetStackTransition(current);
                toPop += poppedPushed.Item1;
                toPop -= poppedPushed.Item2;
                current = current.Previous;
            }

            return current;
        }

        /// <summary>
        /// Generates the patch to be inserted in order to intercept before the API call.
        /// </summary>
        /// <param name="ilProcessor">IL processor.</param>
        /// <param name="hasThis">has this object.</param>
        /// <param name="varDefinition">variable that will hold the "this" object.</param>
        /// <param name="interceptionMethodRef">reference to interception method.</param>
        /// <param name="method">caller method name.</param>
        /// <param name="api">api name.</param>
        /// <param name="ilOffset">IL Offset where the API is called.</param>
        /// <returns>A list of instructions to be inserted.</returns>
        private List<Instruction> InterceptionPatch(ILProcessor ilProcessor, bool hasThis, VariableDefinition varDefinition,
            MethodReference interceptionMethodRef, string method, string api, int ilOffset)
        {
            List<Instruction> patch = new List<Instruction>();
            if (hasThis)
            {
                patch.Add(ilProcessor.Create(OpCodes.Ldloc, varDefinition));
            }
            else
            {
                patch.Add(ilProcessor.Create(OpCodes.Ldnull));
            }

            patch.AddRange(new List<Instruction>()
            {
                ilProcessor.Create(OpCodes.Ldstr, method),
                ilProcessor.Create(OpCodes.Ldstr, api),
                ilProcessor.Create(OpCodes.Ldc_I4, ilOffset),
                ilProcessor.Create(OpCodes.Call,  interceptionMethodRef),
            });

            return patch;
        }

        private string MethodSignatureWithoutReturnType(string fullName)
        {
            string[] tokens = fullName.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            return tokens[1].Replace("::", ".").Replace("get_Item", "Item.get").Replace("set_Item", "Item.set");
        }
    }
}
