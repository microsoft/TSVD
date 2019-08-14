// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace TSVDInstrumenter
{
    /// <summary>
    /// Instrumentation configuration.
    /// </summary>
    [XmlType(TypeName = "InstrumentationConfiguration")]
    public class InstrumentationConfiguration
    {
        private HashSet<string> exactMatchAssemblies;
        private List<Regex> regexAssemblies;
        private HashSet<string> exactMatchBlacklistAssemblies;
        private List<Regex> regexBlacklistAssemblies;
        private HashSet<string> exactMatchAPIs;
        private List<Regex> regexAPIs;
        private bool initialized = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstrumentationConfiguration"/> class.
        /// </summary>
        public InstrumentationConfiguration()
        {
        }

        /// <summary>
        /// Gets or sets whitelisted assemblies to instrument. Assembly names can have wildcards such as Foo*.dll.
        /// </summary>
        [XmlArray("Assemblies")]
        [XmlArrayItem(ElementName = "Assembly")]
        public List<string> Assemblies { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets blacklisted assemblies. These assemblies will be ignored during instrumentation.
        /// This list overrides the white list assemblies <see cref="Assemblies"/>.
        /// Assembly names can be wildcards (e.g., Bar*.dll).
        /// </summary>
        [XmlArray("BlacklistAssemblies")]
        [XmlArrayItem(ElementName = "Assembly")]
        public List<string> BlacklistAssemblies { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets thread unsafe APIs. API names can be wildcard, such as List.Add*.
        /// </summary>
        [XmlArray("ThreadSafetyAPIs")]
        [XmlArrayItem(ElementName = "API")]
        public List<string> ThreadSafetyAPIs { get; set; } = new List<string>();

        /// <summary>
        /// Parse config.
        /// </summary>
        /// <param name="configFilePath">Config file path.</param>
        /// <returns>Parsed config object.</returns>
        public static InstrumentationConfiguration Parse(string configFilePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(InstrumentationConfiguration));
            XmlReaderSettings settings = new XmlReaderSettings();
            using (var reader = XmlReader.Create(configFilePath, settings))
            {
                InstrumentationConfiguration config = serializer.Deserialize(reader) as InstrumentationConfiguration;
                return config;
            }
        }

        /// <summary>
        /// Write config to file.
        /// </summary>
        /// <param name="filePath">File path.</param>
        public void Write(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(InstrumentationConfiguration));
            FileStream fileStream = new FileStream(filePath, FileMode.Create);
            serializer.Serialize(fileStream, this);
            fileStream.Close();
        }

        /// <summary>
        /// Decides if a given assembly name matches the white list and not the black list.
        /// </summary>
        /// <param name="assemblyPath">Assembly path.</param>
        /// <returns>True if match; false otherwise.</returns>
        public bool IsAssemblyMatch(string assemblyPath)
        {
            if (!this.initialized)
            {
                this.InitConfiguration();
            }

            string fileName = Path.GetFileName(assemblyPath);

            // first check the blacklist
            if (this.exactMatchBlacklistAssemblies.Contains(fileName) || this.regexBlacklistAssemblies.Any(x => x.IsMatch(fileName)))
            {
                return false;
            }

            return this.Assemblies == null
                || this.Assemblies.Count == 0
                || this.exactMatchAssemblies.Contains(fileName)
                || this.regexAssemblies.Any(x => x.IsMatch(fileName));
        }

        /// <summary>
        /// Decides if an API matches the list of known thread-unsafe APIs.
        /// </summary>
        /// <param name="apiSignature">API signature.</param>
        /// <returns>True if the signature matches; false otherwise.</returns>
        public bool IsThreadSafetyAPI(string apiSignature)
        {
            if (!this.initialized)
            {
                this.InitConfiguration();
            }

            return this.exactMatchAPIs.Contains(apiSignature)
                || this.regexAPIs.Any(x => x.IsMatch(apiSignature));
        }

        /// <summary>
        /// Get wildcard regex pattern.
        /// </summary>
        /// <param name="value">String with wildcard.</param>
        /// <returns>Regex pattern that matches wild card.</returns>
        private string GetWildcardRegexPattern(string value)
        {
            return string.Format("^{0}$", Regex.Escape(value).Replace("\\*", ".*"));
        }

        /// <summary>
        /// Check if the signature matches the wildcard signature.
        /// </summary>
        /// <param name="fullSignature">Full signature.</param>
        /// <param name="wildcardSignature">Wilcard signature.</param>
        /// <returns>True if match, false otherwise.</returns>
        private bool IsWildcardMatch(string fullSignature, string wildcardSignature)
        {
            return Regex.IsMatch(fullSignature, this.GetWildcardRegexPattern(wildcardSignature));
        }

        /// <summary>
        /// For each group of patterns to match, we maitnain two lists: one with exact match patterns (without wildcards)
        /// and the other with wildcard patterns. For the latter, we precompile the regexs.
        /// </summary>
        private void InitConfiguration()
        {
            this.exactMatchAssemblies = new HashSet<string>();
            this.regexAssemblies = new List<Regex>();

            if (this.Assemblies != null)
            {
                foreach (var assembly in this.Assemblies)
                {
                    if (assembly.Contains("*"))
                    {
                        this.regexAssemblies.Add(new Regex(this.GetWildcardRegexPattern(assembly), RegexOptions.Compiled));
                    }
                    else
                    {
                        this.exactMatchAssemblies.Add(assembly);
                    }
                }
            }

            this.exactMatchBlacklistAssemblies = new HashSet<string>();
            this.regexBlacklistAssemblies = new List<Regex>();

            if (this.BlacklistAssemblies != null)
            {
                foreach (var assembly in this.BlacklistAssemblies)
                {
                    if (assembly.Contains("*"))
                    {
                        this.regexBlacklistAssemblies.Add(new Regex(this.GetWildcardRegexPattern(assembly), RegexOptions.Compiled));
                    }
                    else
                    {
                        this.exactMatchBlacklistAssemblies.Add(assembly);
                    }
                }
            }

            this.exactMatchAPIs = new HashSet<string>();
            this.regexAPIs = new List<Regex>();

            if (this.ThreadSafetyAPIs != null)
            {
                foreach (var api in this.ThreadSafetyAPIs)
                {
                    if (api.Contains("*"))
                    {
                        this.regexAPIs.Add(new Regex(this.GetWildcardRegexPattern(api), RegexOptions.Compiled));
                    }
                    else
                    {
                        this.exactMatchAPIs.Add(api);
                    }
                }
            }

            this.initialized = true;
        }
    }
}
