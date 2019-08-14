// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace TSVDRuntime
{
    /// <summary>
    /// Log helper.
    /// </summary>
    public static class LogHelper
    {
        /// <summary>
        /// Get file path with execution id appended to the file name.
        /// </summary>
        /// <param name="filePath">Input file path.</param>
        /// <returns>Output file path.</returns>
        public static string GetFilePathWithExecutionId(string filePath)
        {
            string directoryName = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileName(filePath);
            string newFileName = string.Format("{0}-{1}{2}", Path.GetFileNameWithoutExtension(filePath), ExecutionContext.ExecutionId, Path.GetExtension(filePath));
            string newFilePath = Path.Combine(directoryName, newFileName);

            return newFilePath;
        }

        /// <summary>
        /// Get InterceptionPoint Log lines.
        /// </summary>
        /// <param name="interceptionPoint">InterceptionPoint information.</param>
        /// <param name="instance">Instance object of the InterceptionPoint call.</param>
        /// <param name="includeStackTrace">A value indicating whether stack trace should be included as a log line.</param>
        /// <returns>Log lines for InterceptionPoint.</returns>
        public static List<string> GetInterceptionPointLogLines(InterceptionPoint interceptionPoint, object instance, bool includeStackTrace)
        {
            List<string> lines = new List<string>();
            lines.AddRange(GetInterceptionPointLogLines(interceptionPoint));
            int objId = instance != null ? instance.GetHashCode() : -1;
            lines.Add(string.Format("ObjectId: {0}", objId));
            if (includeStackTrace)
            {
                lines.Add(string.Format("StackTrace: {0}", interceptionPoint.StackTrace));
            }

            return lines;
        }

        /// <summary>
        /// Get InterceptionPoint Log lines.
        /// </summary>
        /// <param name="interceptionPoint">InterceptionPoint information.</param>
        /// <param name="objId">Id of the instance object.</param>
        /// <param name="includeStackTrace">A value indicating whether stack trace should be included as a log line.</param>
        /// <returns>Log lines for InterceptionPoint.</returns>
        public static List<string> GetInterceptionPointLogLines(InterceptionPoint interceptionPoint, int objId, bool includeStackTrace)
        {
            List<string> lines = new List<string>();
            lines.AddRange(GetInterceptionPointLogLines(interceptionPoint));
            lines.Add(string.Format("ObjectId: {0}", objId));
            if (includeStackTrace)
            {
                lines.Add(string.Format("StackTrace: {0}", interceptionPoint.StackTrace));
            }

            return lines;
        }

        /// <summary>
        /// Get InterceptionPoint Log lines.
        /// </summary>
        /// <param name="interceptionPoint">InterceptionPoint information.</param>
        /// <returns>Log lines for InterceptionPoint.</returns>
        public static List<string> GetInterceptionPointLogLines(InterceptionPoint interceptionPoint)
        {
            List<string> lines = new List<string>();
            lines.Add(string.Format("Id: {0}", interceptionPoint.GLobalHitCount));
            lines.Add(string.Format("ThreadId: {0}", interceptionPoint.ThreadId));
            lines.Add(string.Format("Timestamp: {0}", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture)));
            lines.Add(string.Format("API: {0}", interceptionPoint.API));
            lines.Add(string.Format("Method: {0}", interceptionPoint.Method));
            lines.Add(string.Format("ILOffset: {0}", interceptionPoint.ILOffset));
            return lines;
        }

        /// <summary>
        /// Get process info log lines.
        /// </summary>
        /// <returns>Process info log lines.</returns>
        public static List<string> GetProcessInfoLogLines()
        {
            List<string> lines = new List<string>();
            Process process = Process.GetCurrentProcess();
            lines.Add(string.Format("ProcessName: {0}", process.ProcessName));
            lines.Add(string.Format("ProcessId: {0}", process.Id));
            return lines;
        }
    }
}
