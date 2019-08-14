// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace TSVDRuntime
{
    /// <summary>
    /// TSVD Runtime Configuration.
    /// </summary>
    [XmlType(TypeName = "TSVDRuntimeConfiguration")]
    public class TSVDRuntimeConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TSVDRuntimeConfiguration" /> class.
        /// </summary>
        public TSVDRuntimeConfiguration()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets a value indicating if TSVD should throw an exception when it finds a thread-safety violation.
        /// </summary>
        public bool ThrowExceptionOnRace { get; set; } = true;

        /// <summary>
        /// Gets or sets TSVD parameters.
        /// </summary>
        public TSVDParameters TSVDParameters { get; set; } = new TSVDParameters();

        /// <summary>
        /// Gets or sets trap delay mode.
        /// </summary>
        /// <value>The trap algorithm.</value>
        public TrapAlgorithm TrapAlgorithm { get; set; }

        /// <summary>
        /// Gets or sets trap controller of type "Random".
        /// </summary>
        /// <value>The randomized trap controller.</value>
        public RandomizedTrapController RandomizedTrapController { get; set; }

        /// <summary>
        /// Gets or sets trap controller of type "HB".
        /// </summary>
        /// <value>The hb trap controller.</value>
        public HBTrapController HBTrapController { get; set; }

        /// <summary>
        /// Gets or sets the trap controller.
        /// </summary>
        /// <value>The trap controller.</value>
        [XmlIgnore]
        public ITrapController TrapController { get; set; } = null;

        /// <summary>
        /// Gets or sets bug log file path.
        /// </summary>
        /// <value>The bug log path.</value>
        public string BugLogPath { get; set; }

        /// <summary>
        /// Gets or sets the current dir.
        /// </summary>
        /// <value>The current dir.</value>
        public string CurrentDir { get; set; }

        /// <summary>
        /// Gets or sets thread safety groups.
        /// </summary>
        /// <value>The thread safety groups.</value>
        [XmlElement(ElementName = "ThreadSafetyGroup")]
        public List<ThreadSafetyGroup> ThreadSafetyGroups { get; set; } = new List<ThreadSafetyGroup>();

        /// <summary>
        /// Parse config.
        /// </summary>
        /// <param name="filePath">Config file path.</param>
        /// <returns>Parsed config object.</returns>
        public static TSVDRuntimeConfiguration Parse(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(TSVDRuntimeConfiguration));
            XmlReaderSettings settings = new XmlReaderSettings();
            using (var reader = XmlReader.Create(filePath, settings))
            {
                TSVDRuntimeConfiguration config = serializer.Deserialize(reader) as TSVDRuntimeConfiguration;
                config.Initialize();
                return config;
            }
        }

        /// <summary>
        /// Write config to file.
        /// </summary>
        /// <param name="filePath">File path.</param>
        public void Write(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(TSVDRuntimeConfiguration));
            FileStream fileStream = new FileStream(filePath, FileMode.Create);
            serializer.Serialize(fileStream, this);
            fileStream.Close();
        }

        /// <summary>
        /// Update trap delay mode.
        /// </summary>
        public void Initialize()
        {
            switch (this.TrapAlgorithm)
            {
                case TrapAlgorithm.Randomized:
                    this.TrapController = this.RandomizedTrapController;
                    break;

                case TrapAlgorithm.LearnedHB:
                    this.TrapController = this.HBTrapController;
                    break;
                default:
                    this.TrapController = null;
                    break;
            }

            if (this.TrapController == null)
            {
                ControllerHelper.Debug("Error: Trap controller is null!");
            }
            else if (!this.TrapController.ParametersValid())
            {
                ControllerHelper.Debug("Error: Trap controller parameters invalid!");
                this.TrapController = null;
            }

            this.TrapController?.Initialize(this);
        }
    }
}