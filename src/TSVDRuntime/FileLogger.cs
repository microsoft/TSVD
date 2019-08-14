// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.IO;

namespace TSVDRuntime
{
    /// <summary>
    /// File logger.
    /// </summary>
    public class FileLogger
    {
        /// <summary>
        /// File path for logging.
        /// </summary>
        private readonly string filePath;

        /// <summary>
        /// Log lock.
        /// </summary>
        private readonly object logLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileLogger"/> class.
        /// </summary>
        /// <param name="filePath">File path to log.</param>
        public FileLogger(string filePath, bool append = false)
        {
            this.filePath = filePath;
            this.logLock = new object();

            if (append && File.Exists(this.filePath))
            {
                File.Delete(this.filePath);
            }
        }

        /// <summary>
        /// write formatted strings.
        /// </summary>
        /// <param name="format">string format.</param>
        /// <param name="args">arguments.</param>
        public void Log(string format, params object[] args)
        {
            string line = string.Format(format, args);
            this.Log(new List<string> { line });
        }

        /// <summary>
        /// Write a list of lines.
        /// </summary>
        /// <param name="lines">list of lines to write.</param>
        public void Log(List<string> lines)
        {
            lock (this.logLock)
            {
                try
                {
                    FileStream fileStream = new FileStream(this.filePath, FileMode.Append);
                    using (StreamWriter streamWriter = new StreamWriter(fileStream))
                    {
                        foreach (string line in lines)
                        {
                            streamWriter.WriteLine(line);
                        }
                    }
                }
                catch
                {
                    // ignore
                }
            }
        }
    }
}
