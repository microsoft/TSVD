// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.IO;

namespace TSVDInstrumenter
{
    /// <summary>
    /// Helper class.
    /// </summary>
    public class Helper
    {
        /// <summary>
        /// Copy dependent assemblies.
        /// </summary>
        /// <param name="applicationDirectory">Application directory.</param>
        public static void CopyDependentAssemblies(string applicationDirectory)
        {
            string instrumenterDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            foreach (string dependentAssemblyName in Constants.DependentAssemblies)
            {
                string dllPath = Path.Combine(instrumenterDirectory, dependentAssemblyName + ".dll");
                string pdbPath = Path.Combine(instrumenterDirectory, dependentAssemblyName + ".pdb");
                if (File.Exists(dllPath))
                {
                    File.Copy(dllPath, Path.Combine(applicationDirectory, dependentAssemblyName + ".dll"), true);
                    if (File.Exists(pdbPath))
                    {
                        File.Copy(pdbPath, Path.Combine(applicationDirectory, dependentAssemblyName + ".pdb"), true);
                    }
                }
            }
        }

        /// <summary>
        /// Copy runtime configuration.
        /// </summary>
        /// <param name="applicationDirectory">Application directory.</param>
        /// <param name="configFile">source runtime configuration file.</param>
        public static void CopyRuntimeConfiguration(string applicationDirectory, string configFile)
        {
            string runtimeConfigFile = Path.Combine(applicationDirectory, Constants.RuntimeConfigFile);
            File.Copy(configFile, runtimeConfigFile, true);
        }
    }
}
