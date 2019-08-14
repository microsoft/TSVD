// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace TSVDRuntime
{
    /// <summary>
    /// Interceptor class.
    /// </summary>
    public class Interceptor
    {
        /// <summary>
        /// Runtime configuration.
        /// </summary>
        private static readonly TSVDRuntimeConfiguration Configuration;

        static Interceptor()
        {
            var workingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Directory.SetCurrentDirectory(workingDirectory);
            string configPath = Path.Combine(workingDirectory, Constants.RuntimeConfigFile);
            Configuration = TSVDRuntimeConfiguration.Parse(configPath);
        }

        /// <summary>
        /// Method called right before an intercepted api.
        /// </summary>
        /// <param name="instance">Object instance of the called API.</param>
        /// <param name="method">Name of the method calling the api.</param>
        /// <param name="api">intercepeted API name.</param>
        /// <param name="ilOffset">ILOffset of the API call.</param>
        public static void OnStart(object instance, string method, string api, int ilOffset)
        {
            InterceptionPoint interceptionPoint = new InterceptionPoint()
            {
                Method = method,
                API = MethodSignatureWithoutReturnType(api),
                ILOffset = ilOffset,
                ThreadId = Thread.CurrentThread.ManagedThreadId,
            };
            Configuration.TrapController.InterceptionPointStart(interceptionPoint, instance);
        }

        private static string MethodSignatureWithoutReturnType(string fullName)
        {
            string[] tokens = fullName.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            return tokens[tokens.Length - 1].Replace("::", ".");
        }
    }
}
