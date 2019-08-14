// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace TSVDRuntime
{
    /// <summary>
    /// Interface ITrapController.
    /// </summary>
    public interface ITrapController
    {
        /// <summary>
        /// Initialize the controller.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        void Initialize(TSVDRuntimeConfiguration configuration);

        /// <summary>
        /// Indicates if all required context are correctly specified.
        /// </summary>
        /// <returns>True if context are valid.</returns>
        bool ParametersValid();

        /// <summary>
        /// Converts to rchpointstart.
        /// </summary>
        /// <param name="interceptionPoint">The InterceptionPoint.</param>
        /// <param name="instance">The instance.</param>
        /// <param name="parameters">The parameters.</param>
        void InterceptionPointStart(InterceptionPoint interceptionPoint, object instance = null, object[] parameters = null);
    }
}
