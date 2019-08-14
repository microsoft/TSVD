// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Xml.Serialization;

namespace TSVDRuntime
{
    /// <summary>
    /// Thread safety group.
    /// </summary>
    public class ThreadSafetyGroup
    {
        /// <summary>
        /// Gets or sets name of the thread safety group.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the thread safety group belongs to a static class
        /// or should be considered across instances.
        /// </summary>
        public bool IsStatic { get; set; }

        /// <summary>
        /// Gets or sets write APIs.
        /// </summary>
        [XmlElement("WriteAPI")]
        public List<string> WriteAPIs { get; set; }

        /// <summary>
        /// Gets or sets read APIs.
        /// </summary>
        [XmlElement("ReadAPI")]
        public List<string> ReadAPIs { get; set; }
    }
}
