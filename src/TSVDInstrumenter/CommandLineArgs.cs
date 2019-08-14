// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TSVDInstrumenter
{
    /// <summary>
    /// Contains command line arguments.
    /// </summary>
    public class CommandLineArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineArgs"/> class.
        /// </summary>
        /// <param name="args">Arguments.</param>
        public CommandLineArgs(string[] args)
        {
            this.ArgumentsValid = true;

            if (args.Length >= 1)
            {
                if (File.Exists(args[0]))
                {
                    if (this.InstrumentationTargetValid(args[0]))
                    {
                        this.Assemblies = new List<string>() { args[0] };
                    }
                    else
                    {
                        Console.Error.WriteLine($"ERROR: Not a valid instrumentation target: {args[0]}");
                        this.ArgumentsValid = false;
                    }
                }
                else if (Directory.Exists(args[0]))
                {
                    var validAssemblies = Directory.GetFiles(args[0]).Where(x => this.InstrumentationTargetValid(x));
                    if (validAssemblies.Count() > 0)
                    {
                        this.Assemblies = new List<string>(validAssemblies);
                    }
                    else
                    {
                        Console.Error.WriteLine($"No assemblies in the given directory {args[0]}");
                        this.ArgumentsValid = false;
                    }
                }
            }
            else
            {
                Console.Error.WriteLine($"ERROR: No assembly provided");
                this.ArgumentsValid = false;
            }

            if (args.Length >= 2)
            {
                if (File.Exists(args[1]))
                {
                    this.InstrumentationConfigFile = args[1];
                }
                else
                {
                    Console.Error.WriteLine($"ERROR: Instrumentation configuration file not found: {args[1]}");
                    this.ArgumentsValid = false;
                }
            }
            else
            {
                Console.Error.WriteLine($"ERROR: No insttrumentation configuration file provided");
                this.ArgumentsValid = false;
            }

            if (args.Length >= 3)
            {
                if (File.Exists(args[2]))
                {
                    this.RuntimeConfigFile = args[2];
                }
                else
                {
                    Console.Error.WriteLine($"ERROR: Runtime configuration file not found: {args[2]}");
                    this.ArgumentsValid = false;
                }
            }
            else
            {
                Console.Error.WriteLine($"ERROR: No runtime configuration file provided");
                this.ArgumentsValid = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a value indicating whether command line arguments are valid.
        /// </summary>
        public bool ArgumentsValid { get; set; }

        /// <summary>
        /// Gets or sets list of assemblies to instrument.
        /// </summary>
        public List<string> Assemblies { get; set; }

        /// <summary>
        /// Gets or sets instrumentation configuration file path.
        /// </summary>
        public string InstrumentationConfigFile { get; set; }

        /// <summary>
        /// Gets or sets runtime configuration file path.
        /// </summary>
        public string RuntimeConfigFile { get; set; }

        /// <summary>
        /// Prints usage.
        /// </summary>
        public void Usage()
        {
            Console.WriteLine("Usage: <assembly|directory> <instrumentation configuration file> <runtime confoguration file>");
        }

        private bool InstrumentationTargetValid(string filepath)
        {
            string fileExt = Path.GetExtension(filepath);
            if (string.Equals(".dll", fileExt, StringComparison.OrdinalIgnoreCase) || string.Equals(".exe", fileExt, StringComparison.OrdinalIgnoreCase))
            {
                string fileName = Path.GetFileName(filepath);
                if (!string.Equals(fileName, Constants.TSVDRuntimeLibraryFile, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(fileName, Constants.TSVDInstrumenterFile, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
