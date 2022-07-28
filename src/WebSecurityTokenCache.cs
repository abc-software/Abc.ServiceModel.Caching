// ----------------------------------------------------------------------------
// <copyright file="WebSecurityTokenCache.cs" company="ABC software Ltd">
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
    using System.Web;
    using System.Web.Caching;
#if WIF35
    using Microsoft.IdentityModel.Tokens;
#endif

    /// <summary>
    /// In Web Cache security token cache.
    /// </summary>
    public class WebSecurityTokenCache : SecurityTokenCache
    {
        private readonly object syncRoot = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSecurityTokenCache"/> class.
        /// </summary>
        public WebSecurityTokenCache()
        {
        }

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
                    HttpRuntime.Cache.Insert(this.GetCacheKey(key), value, null, value.ValidTo, Cache.NoSlidingExpiration);
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
                value = (SecurityToken)HttpRuntime.Cache.Get(this.GetCacheKey(key));
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
                return HttpRuntime.Cache.Remove(this.GetCacheKey(key)) != null;
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
            throw new NotImplementedException();
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

        /// <inheritdoc/>
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