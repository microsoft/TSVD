// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace TSVDInstrumenter
{
    /// <summary>
    /// Instrumentation result.
    /// </summary>
    public enum InstrumentationResult
    {
        /// <summary>
        /// instrumentation succesful
        /// </summary>
        OK = 0,

        /// <summary>
        /// error in input arguments
        /// </summary>
        ERROR_ARGUMENTS,

        /// <summary>
        /// generic instrumentation error
        /// </summary>
        ERROR_INSTRUMENTATION,

        /// <summary>
        /// skipped instrumentation because the assembly file is not found
        /// </summary>
        ERROR_FileNotFound,

        /// <summary>
        /// assembly cannot be loaded for instrumentation
        /// </summary>
        ERROR_CannotLoadAssembly,

        /// <summary>
        /// other errors
        /// </summary>
        ERROR_Other,

        /// <summary>
        /// skipped instrumentation because the assembly is signed
        /// </summary>
        SKIPPED_SignedAssembly,

        /// <summary>
        /// skipped instrumentation because the assembly is mixed mode
        /// </summary>
        SKIPPED_MixedModeAssembly,

        /// <summary>
        /// skipped instrumentation because the assembly is not a target
        /// </summary>
        SKIPPED_NotATarget,

        /// <summary>
        /// skipped instrumentation because there is no assembly to instrument
        /// </summary>
        SKIPPED_NothingToInstrument,

        /// <summary>
        /// skipped instrumentation because the assembly is already instrumented
        /// </summary>
        SKIPPED_AlreadyInstrumented,
    }
}
