// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;

namespace TSVDInstrumenter
{
    /// <summary>
    /// TSVD instrumenter class.
    /// </summary>
    public class Instrumenter
    {
        private readonly ILRewriter ilRewriter;
        private string instrumentationConfigurationFile;
        private string runtimeConfigurationFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="Instrumenter"/> class.
        /// </summary>
        /// <param name="instrumentationConfigFile">Instrumentation configuration filepath. </param>
        /// <param name="runtimeConfigFile">Runtime configuration file path.</param>
        public Instrumenter(string instrumentationConfigFile, string runtimeConfigFile)
        {
            this.instrumentationConfigurationFile = instrumentationConfigFile;
            this.runtimeConfigurationFile = runtimeConfigFile;
            var configuration = InstrumentationConfiguration.Parse(instrumentationConfigFile);
            this.ilRewriter = new ILRewriter(configuration);

            // a dummy call to force copying TSVDRuntime.dll to the build directory
            TSVDRuntime.Dummy.Call();
        }

        /// <summary>
        /// Instruments a list of assemblies.
        /// </summary>
        /// <param name="assemblyPaths">A list of assemblies to instrument.</param>
        /// <returns>0, if at least one input assembly is instrumented correctly.</returns>
        public InstrumentationResult Instrument(List<string> assemblyPaths)
        {
            if (assemblyPaths == null)
            {
                return InstrumentationResult.ERROR_ARGUMENTS;
            }

            bool instrumented = false;
            string directoryPath = null;

            Console.WriteLine("Instrumenting assemblies.");
            foreach (string path in assemblyPaths)
            {
                Console.Write($"{Path.GetFileName(path)} ...");
                var result = this.ilRewriter.RewriteThreadSafetyAPIs(path);
                Console.WriteLine(result);
                if (result == InstrumentationResult.OK)
                {
                    directoryPath = Path.GetDirectoryName(path);
                    instrumented = true;
                }
            }

            Console.WriteLine("Instrumentation complete. Copying TSVD files.");

            if (instrumented)
            {
                Helper.CopyDependentAssemblies(directoryPath);
                Helper.CopyRuntimeConfiguration(directoryPath, this.runtimeConfigurationFile);
            }

            Console.WriteLine($"Done.");
            return instrumented ? InstrumentationResult.OK : InstrumentationResult.ERROR_INSTRUMENTATION;
        }
    }
}
