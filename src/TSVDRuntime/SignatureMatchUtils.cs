// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Text.RegularExpressions;

namespace TSVDRuntime
{
    /// <summary>
    /// Utility methods used for method/call signature matching.
    /// </summary>
    public class SignatureMatchUtils
    {
        /// <summary>
        /// Get wildcard regex pattern.
        /// </summary>
        /// <param name="value">String with wildcard.</param>
        /// <returns>Regex pattern that matches wild card.</returns>
        public static string GetWildcardRegexPattern(string value)
        {
            return string.Format("^{0}$", Regex.Escape(value).Replace("\\*", ".*"));
        }

        /// <summary>
        /// Check if the signature matches the wildcard signature.
        /// </summary>
        /// <param name="fullSignature">Full signature.</param>
        /// <param name="wildcardSignature">Wilcard signature.</param>
        /// <returns>True if match, false otherwise.</returns>
        public static bool IsWildcardMatch(string fullSignature, string wildcardSignature)
        {
            return Regex.IsMatch(fullSignature, GetWildcardRegexPattern(wildcardSignature));
        }
    }
}