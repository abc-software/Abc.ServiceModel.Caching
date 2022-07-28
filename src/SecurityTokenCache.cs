// ----------------------------------------------------------------------------
// <copyright file="SecurityTokenCache.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

#if !WIF35

namespace Abc.ServiceModel.Caching
{
    using System.Collections.Generic;
    using System.IdentityModel.Tokens;

    /// <summary>
    /// Security Token Cache.
    /// </summary>
    public abstract class SecurityTokenCache
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityTokenCache"/> class.
        /// </summary>
        protected SecurityTokenCache()
        {
        }

        /// <summary>
        /// Clears the entries in the cache.
        /// </summary>
        public abstract void ClearEntries();

        /// <summary>
        /// Attempts to add an entry to the cache. 
        /// </summary>
        /// <param name="key">The key of the entry to be added.</param>
        /// <param name="value">The associated SecurityToken to be added.</param>
        /// <returns><c>true</c> on success. <c>false</c> on failure</returns>
        public abstract bool TryAddEntry(object key, SecurityToken value);

        /// <summary>
        /// Attempts to retrieve all entries from the cache. 
        /// </summary>
        /// <param name="key">The key of the entry to be retrieved.</param>
        /// <param name="tokens">The <see cref="SecurityToken"/> list associated with the input key.</param>
        /// <returns><c>true</c> on success. <c>false</c> on failure</returns>
        public abstract bool TryGetAllEntries(object key, out IList<SecurityToken> tokens);

        /// <summary>
        /// Attempts to retrieve an entry from the cache.
        /// </summary>
        /// <param name="key">The key of the entry to be retrieved.</param>
        /// <param name="value">The <see cref="SecurityToken"/> associated with the input key.</param>
        /// <returns><c>true</c> on success. <c>false</c> on failure</returns>
        public abstract bool TryGetEntry(object key, out SecurityToken value);

        /// <summary>
        /// Attempts to remove all matching entries from cache. 
        /// </summary>
        /// <param name="key">The key of the entry to be removed.</param>
        /// <returns><c>true</c> on success. <c>false</c> on failure</returns>
        public abstract bool TryRemoveAllEntries(object key);

        /// <summary>
        /// Attempts to remove an entry from the cache. 
        /// </summary>
        /// <param name="key">The key of the entry to be removed.</param>
        /// <returns><c>true</c> on success. <c>false</c> on failure</returns>
        public abstract bool TryRemoveEntry(object key);

        /// <summary>
        /// Attempts to replace an existing entry in the cache with a new one. 
        /// </summary>
        /// <param name="key">The key of the entry to be retrieved.</param>
        /// <param name="newValue">The <see cref="SecurityToken"/> which will replace the entry associated with the key.</param>
        /// <returns><c>true</c> on success. <c>false</c> on failure</returns>
        public abstract bool TryReplaceEntry(object key, SecurityToken newValue);
    }
}

#endif