// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace TSVDInstrumenter
{
    /// <summary>
    /// Helper functions for MSIL processing.
    /// </summary>
    public class MSILHelper
    {
        /// <summary>
        /// Returns how many items an MSIL instruction pops from and pushes into the stack.
        /// </summary>
        /// <param name="instruction">An MSIL instruction.</param>
        /// <returns>A tuple.<int, int> containing the number of items popped from and the number of items pushed into the stack</int></returns>
        public static Tuple<int, int> GetStackTransition(Instruction instruction)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Br:
                case Code.Br_S:
                case Code.Break:
                case Code.Endfinally:
                case Code.Jmp:
                case Code.Leave:
                case Code.Leave_S:
                case Code.Nop:
                case Code.Rethrow:
                case Code.Constrained:
                case Code.Tail:
                case Code.Unaligned:
                case Code.Volatile:
                case Code.Readonly:
                case Code.No:
                    return new Tuple<int, int>(0, 0);

                case Code.Arglist:
                case Code.Ldarg:
                case Code.Ldarg_0:
                case Code.Ldarg_1:
                case Code.Ldarg_2:
                case Code.Ldarg_3:
                case Code.Ldarg_S:
                case Code.Ldarga:
                case Code.Ldarga_S:
                case Code.Ldc_I4:
                case Code.Ldc_I4_0:
                case Code.Ldc_I4_1:
                case Code.Ldc_I4_2:
                case Code.Ldc_I4_3:
                case Code.Ldc_I4_4:
                case Code.Ldc_I4_5:
                case Code.Ldc_I4_6:
                case Code.Ldc_I4_7:
                case Code.Ldc_I4_8:
                case Code.Ldc_I4_M1:
                case Code.Ldc_I4_S:
                case Code.Ldc_I8:
                case Code.Ldc_R4:
                case Code.Ldc_R8:
                case Code.Ldftn:
                case Code.Ldloc:
                case Code.Ldloc_0:
                case Code.Ldloc_1:
                case Code.Ldloc_2:
                case Code.Ldloc_3:
                case Code.Ldloc_S:
                case Code.Ldloca:
                case Code.Ldloca_S:
                case Code.Ldnull:
                case Code.Ldsfld:
                case Code.Ldsflda:
                case Code.Ldstr:
                case Code.Ldtoken:
                case Code.Sizeof:
                    return new Tuple<int, int>(0, 1);

                case Code.Brfalse:
                case Code.Brfalse_S:
                case Code.Brtrue:
                case Code.Brtrue_S:
                case Code.Endfilter:
                case Code.Initobj:
                case Code.Pop:
                case Code.Starg:
                case Code.Starg_S:
                case Code.Stloc:
                case Code.Stloc_0:
                case Code.Stloc_1:
                case Code.Stloc_2:
                case Code.Stloc_3:
                case Code.Stloc_S:
                case Code.Stsfld:
                case Code.Switch:
                case Code.Throw:
                    return new Tuple<int, int>(1, 0);

                case Code.Box:
                case Code.Castclass:
                case Code.Ckfinite:
                case Code.Conv_I:
                case Code.Conv_I1:
                case Code.Conv_I2:
                case Code.Conv_I4:
                case Code.Conv_I8:
                case Code.Conv_Ovf_I:
                case Code.Conv_Ovf_I_Un:
                case Code.Conv_Ovf_I1:
                case Code.Conv_Ovf_I1_Un:
                case Code.Conv_Ovf_I2:
                case Code.Conv_Ovf_I2_Un:
                case Code.Conv_Ovf_I4:
                case Code.Conv_Ovf_I4_Un:
                case Code.Conv_Ovf_I8:
                case Code.Conv_Ovf_I8_Un:
                case Code.Conv_Ovf_U:
                case Code.Conv_Ovf_U_Un:
                case Code.Conv_Ovf_U1:
                case Code.Conv_Ovf_U1_Un:
                case Code.Conv_Ovf_U2:
                case Code.Conv_Ovf_U2_Un:
                case Code.Conv_Ovf_U4:
                case Code.Conv_Ovf_U4_Un:
                case Code.Conv_Ovf_U8:
                case Code.Conv_Ovf_U8_Un:
                case Code.Conv_R_Un:
                case Code.Conv_R4:
                case Code.Conv_R8:
                case Code.Conv_U:
                case Code.Conv_U1:
                case Code.Conv_U2:
                case Code.Conv_U4:
                case Code.Conv_U8:
                case Code.Isinst:
                case Code.Ldfld:
                case Code.Ldflda:
                case Code.Ldind_I:
                case Code.Ldind_I1:
                case Code.Ldind_I2:
                case Code.Ldind_I4:
                case Code.Ldind_I8:
                case Code.Ldind_R4:
                case Code.Ldind_R8:
                case Code.Ldind_Ref:
                case Code.Ldind_U1:
                case Code.Ldind_U2:
                case Code.Ldind_U4:
                case Code.Ldlen:
                case Code.Ldobj:
                case Code.Ldvirtftn:
                case Code.Localloc:
                case Code.Mkrefany:
                case Code.Neg:
                case Code.Newarr:
                case Code.Not:
                case Code.Refanytype:
                case Code.Refanyval:
                case Code.Unbox:
                case Code.Unbox_Any:
                    return new Tuple<int, int>(1, 1);

                case Code.Dup:
                    return new Tuple<int, int>(1, 2);

                case Code.Beq:
                case Code.Beq_S:
                case Code.Bge:
                case Code.Bge_S:
                case Code.Bge_Un:
                case Code.Bge_Un_S:
                case Code.Bgt:
                case Code.Bgt_S:
                case Code.Bgt_Un:
                case Code.Bgt_Un_S:
                case Code.Ble:
                case Code.Ble_S:
                case Code.Ble_Un:
                case Code.Ble_Un_S:
                case Code.Blt:
                case Code.Blt_S:
                case Code.Blt_Un:
                case Code.Blt_Un_S:
                case Code.Bne_Un:
                case Code.Bne_Un_S:
                case Code.Cpobj:
                case Code.Stfld:
                case Code.Stind_I:
                case Code.Stind_I1:
                case Code.Stind_I2:
                case Code.Stind_I4:
                case Code.Stind_I8:
                case Code.Stind_R4:
                case Code.Stind_R8:
                case Code.Stind_Ref:
                case Code.Stobj:
                    return new Tuple<int, int>(2, 0);

                case Code.Add:
                case Code.Add_Ovf:
                case Code.Add_Ovf_Un:
                case Code.And:
                case Code.Ceq:
                case Code.Cgt:
                case Code.Cgt_Un:
                case Code.Clt:
                case Code.Clt_Un:
                case Code.Div:
                case Code.Div_Un:
                case Code.Ldelem_Any:
                case Code.Ldelem_I:
                case Code.Ldelem_I1:
                case Code.Ldelem_I2:
                case Code.Ldelem_I4:
                case Code.Ldelem_I8:
                case Code.Ldelem_R4:
                case Code.Ldelem_R8:
                case Code.Ldelem_Ref:
                case Code.Ldelem_U1:
                case Code.Ldelem_U2:
                case Code.Ldelem_U4:
                case Code.Ldelema:
                case Code.Mul:
                case Code.Mul_Ovf:
                case Code.Mul_Ovf_Un:
                case Code.Or:
                case Code.Rem:
                case Code.Rem_Un:
                case Code.Shl:
                case Code.Shr:
                case Code.Shr_Un:
                case Code.Sub:
                case Code.Sub_Ovf:
                case Code.Sub_Ovf_Un:
                case Code.Xor:
                    return new Tuple<int, int>(2, 1);

                case Code.Cpblk:
                case Code.Initblk:
                case Code.Stelem_Any:
                case Code.Stelem_I:
                case Code.Stelem_I1:
                case Code.Stelem_I2:
                case Code.Stelem_I4:
                case Code.Stelem_I8:
                case Code.Stelem_R4:
                case Code.Stelem_R8:
                case Code.Stelem_Ref:
                    return new Tuple<int, int>(3, 0);

                case Code.Call:
                case Code.Callvirt:
                case Code.Calli:
                    var method = (MethodReference)instruction.Operand;
                    int popped = method.HasThis ? method.Parameters.Count + 1 : method.Parameters.Count;
                    if (instruction.OpCode.Code == Code.Calli)
                    {
                        popped++;
                    }

                    int pushed = method.ReturnType == null ? 0 : 1;
                    return new Tuple<int, int>(popped, pushed);
                case Code.Newobj:
                    return new Tuple<int, int>(((MethodReference)instruction.Operand).Parameters.Count, 1);
                case Code.Ret:
                    break;
                default:
                    throw new Exception("Unknown IL code type " + instruction.OpCode);
            }

            return new Tuple<int, int>(-1, -1);
        }
    }
}
