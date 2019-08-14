// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace TSVDInstrumenter
{
    /// <summary>
    /// Class containing the main entry method.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry method.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>0 if instrumentation succeeds for at least one input assembly.</returns>
        public static int Main(string[] args)
        {
            Console.WriteLine(Constants.ProgramName + " - " + typeof(Program).Assembly.GetName().Version);

            CommandLineArgs arguments = new CommandLineArgs(args);
            if (!arguments.ArgumentsValid)
            {
                arguments.Usage();
                return (int)InstrumentationResult.ERROR_ARGUMENTS;
            }

            Instrumenter instrumenter = new Instrumenter(arguments.InstrumentationConfigFile, arguments.RuntimeConfigFile);
            var result = instrumenter.Instrument(arguments.Assemblies);
            Console.WriteLine($"Instrumetation result: " + result);

            return (int)result;
        }
    }
}
