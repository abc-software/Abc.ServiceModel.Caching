// ----------------------------------------------------------------------------
// <copyright file="MemSecurityTokenCache.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

namespace Abc.ServiceModel.Caching
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens;
    using System.Runtime.Caching;
#if WIF35
    using Microsoft.IdentityModel.Tokens;
#endif

    /// <summary>
    /// In memory security token cache.
    /// </summary>
    public class MemSecurityTokenCache : SecurityTokenCache
    {
        private const string RegionName = null; // .NET4 not support parameters "MemCache";
        private readonly object syncRoot = new object();

        /// <inheritdoc/>
        public override bool TryAddEntry(object key, SecurityToken value)
        {
            if (key == null)
            {
                return false;
            }

            lock (this.syncRoot)
            {
                SecurityToken token;
                bool flag = this.TryGetEntry(key, out token);
                if (!flag)
                {
                    MemoryCache.Default.Add(this.GetCacheKey(key), value, value.ValidTo, RegionName);
                }

                return flag;
            }
        }

        /// <inheritdoc/>
        public override bool TryGetEntry(object key, out SecurityToken value)
        {
            value = null;
            if (key == null)
            {
                return false;
            }

            lock (this.syncRoot)
            {
                value = (SecurityToken)MemoryCache.Default.Get(this.GetCacheKey(key), RegionName);
                return value != null;
            }
        }

        /// <inheritdoc/>
        public override bool TryRemoveEntry(object key)
        {
            if (key == null)
            {
                return false;
            }

            lock (this.syncRoot)
            {
                return MemoryCache.Default.Remove(this.GetCacheKey(key), RegionName) != null;
            }
        }

        /// <inheritdoc/>
        public override bool TryReplaceEntry(object key, SecurityToken newValue)
        {
            return this.TryAddEntry(key, newValue);
        }

        /// <inheritdoc/>
        public override void ClearEntries()
        {
            lock (this.syncRoot)
            {
                var items = MemoryCache.Default.GetValues(RegionName);
                foreach (var item in items)
                {
                    MemoryCache.Default.Remove(item.Key, RegionName);
                }
            }
        }

        /// <inheritdoc/>
        public override bool TryGetAllEntries(object key, out IList<SecurityToken> tokens)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override bool TryRemoveAllEntries(object key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create string representation of key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The cache key.</returns>
        protected virtual string GetCacheKey(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return key.ToString();
        }
    }
}