// ----------------------------------------------------------------------------
// <copyright file="MemPrincipalSecurityTokenCache.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

namespace Abc.ServiceModel.Caching
{
    using System;
    using System.Web;

    /// <summary>
    /// In memory security token cache with <see cref="System.Threading.Thread.CurrentPrincipal"/> as cache key. />.
    /// </summary>
    public class MemPrincipalSecurityTokenCache : MemSecurityTokenCache
    {
        /// <inheritdoc/>
        public override bool TryRemoveAllEntries(object key)
        {
            var cacheKey = CacheKey.FromValue(this.GetCacheKey(key));
            cacheKey.CanIgnoreEndpointId = true;

            foreach (object item in HttpRuntime.Cache)
            {
                ////var cacheKey0 = CacheKey.FromValue(item.Value);   
                ////cacheKey.CanIgnoreEndpointId = true;

                ////if (cacheKey.Equals(cacheKey0)) {
                //    //this.TryRemoveEntry(cacheKey0.ToString());   
                ////} 
            }

            return true;
        }

        /// <inheritdoc/>
        protected override string GetCacheKey(object key)
        {
            var principal = System.Threading.Thread.CurrentPrincipal;

            var newKey = new CacheKey(base.GetCacheKey(key), principal.Identity.Name);
            return newKey.ToString();
        }

        //// public override bool TryGetAllEntries(object key, out IList<SecurityToken> tokens) {
        ////     tokens = new List<SecurityToken>();
        ////     //lock (this._syncRoot) {
        ////         foreach (object item in HostingEnvironment.Cache) {
        ////             if (item) {

        ////                 tokens.Add(entry.value);
        ////             }
        ////         }

        ////         return (tokens.Count > 0);
        ////     //}
        ////}

        private class CacheKey
        {
            private readonly string endpointId;
            private readonly string identityName;
            private bool canIgnoreEndpointId;

            public CacheKey(string endpointId, string identityName)
            {
                this.endpointId = endpointId;
                this.identityName = identityName;
            }

            public string EndpointId
            {
                get { return this.endpointId; }
            }

            public string IdentityName
            {
                get { return this.identityName; }
            }

            public bool CanIgnoreEndpointId
            {
                get { return this.canIgnoreEndpointId; }
                set { this.canIgnoreEndpointId = value; }
            }

            public static CacheKey FromValue(string hash)
            {
                if (hash == null)
                {
                    throw new ArgumentNullException(nameof(hash));
                }

                var values = hash.Split(new char[] { ':' }, 2);
                return new CacheKey(values[1], values[0]);
            }

            public static bool operator ==(CacheKey a, CacheKey b)
            {
                if (object.ReferenceEquals(a, null))
                {
                    return object.ReferenceEquals(b, null);
                }

                return a.Equals(b);
            }

            public static bool operator !=(CacheKey a, CacheKey b)
            {
                return !(a == b);
            }

            public override bool Equals(object obj)
            {
                if (!(obj is CacheKey))
                {
                    return false;
                }

                CacheKey key = (CacheKey)obj;
                bool flag = true;
                if (!this.canIgnoreEndpointId && !key.canIgnoreEndpointId)
                {
                    flag = StringComparer.Ordinal.Equals(key.EndpointId, this.endpointId);
                }

                return flag && key.identityName == this.identityName;
            }

            public override string ToString()
            {
                return this.identityName + ":" + this.endpointId;
            }

            public override int GetHashCode()
            {
                int hash = 0;
                if (this.identityName != null)
                {
                    hash = hash ^ this.identityName.GetHashCode();
                }

                if (this.endpointId != null && !this.canIgnoreEndpointId)
                {
                    hash = hash ^ this.endpointId.GetHashCode();
                }

                return hash;
            }
        }
    }
}