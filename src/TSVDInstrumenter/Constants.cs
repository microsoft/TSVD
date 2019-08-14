// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace TSVDInstrumenter
{
    /// <summary>
    /// Class containing all constant values.
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// Gets program name.
        /// </summary>
        public static string ProgramName => "TSVD";

        /// <summary>
        /// Gets runtime library name.
        /// </summary>
        public static string TSVDRuntimeLibrary => "TSVDRuntime";

        /// <summary>
        /// Gets runtime library file name.
        /// </summary>
        public static string TSVDRuntimeLibraryFile => TSVDRuntimeLibrary + ".dll";

        /// <summary>
        /// Gets instrumenter file name.
        /// </summary>
        public static string TSVDInstrumenterFile => "TSVDInstrumenter.exe";

        /// <summary>
        /// Gets interceptor type.
        /// </summary>
        public static string InterceptorType => "Interceptor";

        /// <summary>
        /// Gets onStart method name.
        /// </summary>
        public static string OnStartMethod => "OnStart";

        /// <summary>
        ///  Gets list of dependent assemblies that need to be copied to instrumented assemblies.
        /// </summary>
        public static List<string> DependentAssemblies => new List<string>() { TSVDRuntimeLibrary };

        /// <summary>
        /// Gets runtime configuration file.
        /// </summary>
        public static string RuntimeConfigFile => "TSVDRuntimeConfiguration.cfg";
    }
}
