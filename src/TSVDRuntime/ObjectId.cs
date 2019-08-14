// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.CompilerServices;

namespace TSVDRuntime
{
    /// <summary>
    /// Class ObjectId.
    /// </summary>
    public static class ObjectId
    {
        /// <summary>
        /// The ids.
        /// </summary>
        private static readonly ConditionalWeakTable<object, RefId> ObjIds = new ConditionalWeakTable<object, RefId>();

        /// <summary>
        /// Gets the reference identifier.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Guid.</returns>
        public static Guid GetRefId(object obj)
        {
            if (obj == null)
            {
                return default(Guid);
            }

            return ObjIds.GetOrCreateValue(obj).Id;
        }

        /// <summary>
        /// Class RefId.
        /// </summary>
        private class RefId
        {
            /// <summary>
            /// Gets the identifier.
            /// </summary>
            /// <value>The identifier.</value>
            public Guid Id { get; } = Guid.NewGuid();
        }
    }
}