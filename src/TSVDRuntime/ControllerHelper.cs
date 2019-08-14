// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace TSVDRuntime
{
    /// <summary>
    /// Class ControllerHelper.
    /// </summary>
    public static class ControllerHelper
    {
        private static readonly HashSet<string> KnownBugs = new HashSet<string>();
        private static FileLogger debugLogger;
        private static FileLogger bugLogger;

        static ControllerHelper()
        {
            InitLogs();
        }

        /// <summary>
        /// Gets or sets a value indicating whether traps should be logged for debug purpose.
        /// Since currently we log synchronously, logging traps can be expensive when TSVD
        /// sets a large number of traps. So we set it to false as default.
        /// </summary>
        public static bool EnableTrapLogging { get; set; } = false;

        /// <summary>
        /// Set logging directory.
        /// </summary>
        /// <param name="directoryPath">Directory path.</param>
        public static void SetLoggingDirectory(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                InitLogs(directoryPath);
            }
        }

        /// <summary>
        /// Log a debug line.
        /// </summary>
        /// <param name="line">A text line to log.</param>
        public static void Debug(string line)
        {
            debugLogger.Log(line);
        }

        /// <summary>
        /// Log trap point.
        /// </summary>
        /// <param name="trap">Trap details.</param>
        public static void WriteTrap(Trap trap)
        {
            if (EnableTrapLogging)
            {
                List<string> lines = new List<string>();
                lines.Add("---Trap---");
                lines.AddRange(LogHelper.GetInterceptionPointLogLines(trap.InterceptionPoint, trap.ObjectId, false));
                lines.Add(string.Format("Delay: {0}", trap.Delay));
                debugLogger.Log(lines);
            }
        }

        /// <summary>
        /// Log a race condition.
        /// </summary>
        /// <param name="trap">Trap details.</param>
        /// <param name="trappedPoint">InterceptionPoint that got trapped.</param>
        /// <returns><see langword="true" /> if XXXX, <see langword="false" /> otherwise.</returns>
        public static bool WriteBug(Trap trap, InterceptionPoint trappedPoint)
        {
            if (bugLogger == null)
            {
                return false;
            }

            string bug = trap.InterceptionPoint.Method + ";" + trap.InterceptionPoint.API + ";" + trap.InterceptionPoint.ILOffset + ";" + trap.InterceptionPoint.StackTrace + "\n"
                + trappedPoint.Method + ";" + trappedPoint.API + ";" + trappedPoint.ILOffset + ";" + trappedPoint.StackTrace;
            if (!KnownBugs.Contains(bug))
            {
                KnownBugs.Add(bug);

                List<string> lines = new List<string>();
                lines.Add("---Race Condition---");
                lines.Add("---Trap---");
                lines.AddRange(LogHelper.GetInterceptionPointLogLines(trap.InterceptionPoint, trap.ObjectId, true));
                lines.Add(string.Format("Delay: {0}", trap.Delay));
                lines.Add("---Trapped---");
                lines.AddRange(LogHelper.GetInterceptionPointLogLines(trappedPoint, trap.ObjectId, true));
                bugLogger.Log(lines);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Get thread safety group details of the InterceptionPoint.
        /// </summary>
        /// <param name="threadSafetyGroups">The thread safety groups.</param>
        /// <param name="interceptionPoint">InterceptionPoint.</param>
        /// <returns>Thread safety InterceptionPoint.</returns>
        public static ThreadSafetyInterceptionPoint GetThreadSafetyInterceptionPoint(List<ThreadSafetyGroup> threadSafetyGroups, InterceptionPoint interceptionPoint)
        {
            if (threadSafetyGroups == null)
            {
                return null;
            }

            foreach (ThreadSafetyGroup threadSafetyGroup in threadSafetyGroups)
            {
                foreach (string writeAPI in threadSafetyGroup.WriteAPIs)
                {
                    if (SignatureMatchUtils.IsWildcardMatch(interceptionPoint.API, writeAPI))
                    {
                        return new ThreadSafetyInterceptionPoint()
                        {
                            ThreadSafetyGroup = threadSafetyGroup,
                            IsWriteAPI = true,
                        };
                    }
                }

                foreach (string readAPI in threadSafetyGroup.ReadAPIs)
                {
                    if (SignatureMatchUtils.IsWildcardMatch(interceptionPoint.API, readAPI))
                    {
                        return new ThreadSafetyInterceptionPoint()
                        {
                            ThreadSafetyGroup = threadSafetyGroup,
                            IsWriteAPI = false,
                        };
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Read thread safety specification files.
        /// </summary>
        /// <param name="threadSafetySpecificationFiles">Thread safety specification files.</param>
        /// <returns>Thread safety groups.</returns>
        public static List<ThreadSafetyGroup> ReadThreadSafetyGroups(List<string> threadSafetySpecificationFiles)
        {
            List<ThreadSafetyGroup> threadSafetyGroups = new List<ThreadSafetyGroup>();
            foreach (string filePath in threadSafetySpecificationFiles)
            {
                string exePath = Assembly.GetExecutingAssembly().Location;
                string fullPath = Path.Combine(Path.GetDirectoryName(exePath), filePath);
                TSVDRuntimeConfiguration threadSafetySpecification = TSVDRuntimeConfiguration.Parse(fullPath);
                threadSafetyGroups.AddRange(threadSafetySpecification.ThreadSafetyGroups);
            }

            return threadSafetyGroups;
        }

        /// <summary>
        /// Initializes static members of the <see cref="DebugLog"/> class.
        /// </summary>
        private static void InitLogs(string logDirectory = null)
        {
            string debugLogFile = string.Format("{0}-{1}{2}", Constants.DebugLogFilePrefix, ExecutionContext.ExecutionId, Constants.LogFileExtension);
            string bugLogFile = string.Format("{0}-{1}{2}", Constants.BugLogFilePrefix, ExecutionContext.ExecutionId, Constants.LogFileExtension);

            if (logDirectory != null && Directory.Exists(logDirectory))
            {
                debugLogFile = Path.Combine(logDirectory, debugLogFile);
                bugLogFile = Path.Combine(logDirectory, bugLogFile);
            }

            debugLogger = new FileLogger(debugLogFile);
            bugLogger = new FileLogger(bugLogFile);

            debugLogger.Log(LogHelper.GetProcessInfoLogLines());
        }
    }
}
