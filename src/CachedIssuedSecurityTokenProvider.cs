// ----------------------------------------------------------------------------
// <copyright file="CachedIssuedSecurityTokenProvider.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

namespace Abc.ServiceModel.Caching
{
    using System;
    using System.Globalization;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.Linq;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;
    using Abc.ServiceModel.Threading;
#if WIF35
    using Microsoft.IdentityModel;
    using Microsoft.IdentityModel.Claims;
    using Microsoft.IdentityModel.Protocols.WSTrust;
    using Microsoft.IdentityModel.Tokens;
#else
    using System.Security.Claims;
#endif

    /// <summary>
    /// Token provider for issued security tokens.
    /// </summary>
    internal class CachedIssuedSecurityTokenProvider : IssuedSecurityTokenProvider
    {
        private readonly SecurityTokenCache tokenCache;
        private readonly CachedClientCredentialsParameters clientCredentialsParamaters;
        private readonly SecurityTokenHandlerCollectionManager securityTokenHandlerCollectionManager;
        private readonly object thisLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedIssuedSecurityTokenProvider" /> class.
        /// </summary>
        /// <param name="tokenCache">The security token cache.</param>
        /// <param name="provider">The provider.</param>
        public CachedIssuedSecurityTokenProvider(SecurityTokenCache tokenCache, IssuedSecurityTokenProvider provider)
            : this(tokenCache, provider, new CachedClientCredentialsParameters(), SecurityTokenHandlerCollectionManager.CreateDefaultSecurityTokenHandlerCollectionManager())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedIssuedSecurityTokenProvider" /> class.
        /// </summary>
        /// <param name="tokenCache">The security token cache.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="clientCredentialsParamaters">The federated client credentials parameters.</param>
        /// <param name="securityTokenHandlerCollectionManager">The security token handler collection manager.</param>
        public CachedIssuedSecurityTokenProvider(SecurityTokenCache tokenCache, IssuedSecurityTokenProvider provider, CachedClientCredentialsParameters clientCredentialsParamaters, SecurityTokenHandlerCollectionManager securityTokenHandlerCollectionManager)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            this.tokenCache = tokenCache;
            this.clientCredentialsParamaters = clientCredentialsParamaters ?? throw new ArgumentNullException(nameof(clientCredentialsParamaters));
            base.CacheIssuedTokens = false;
            this.CloneBase(provider);
            this.securityTokenHandlerCollectionManager = securityTokenHandlerCollectionManager ?? throw new ArgumentNullException(nameof(securityTokenHandlerCollectionManager));
            this.CacheIssuedTokens = true; // Cache enabled by default
        }

        /// <summary>
        /// Gets or sets a value indicating whether to cache issued tokens.
        /// </summary>
        /// <returns><c>true</c> if issued tokens are cached; otherwise, <c>false</c>.</returns>
        public new bool CacheIssuedTokens { get; set; }

        /// <summary>
        /// Clones the base.
        /// </summary>
        /// <param name="issuedSecurityTokenProvider">The issued security token provider.</param>
        internal void CloneBase(IssuedSecurityTokenProvider issuedSecurityTokenProvider)
        {
            if (issuedSecurityTokenProvider == null)
            {
                throw new ArgumentNullException(nameof(issuedSecurityTokenProvider));
            }

            this.IdentityVerifier = issuedSecurityTokenProvider.IdentityVerifier;
            this.IssuerBinding = issuedSecurityTokenProvider.IssuerBinding;
            this.IssuerAddress = issuedSecurityTokenProvider.IssuerAddress;
            this.TargetAddress = issuedSecurityTokenProvider.TargetAddress;
            this.KeyEntropyMode = issuedSecurityTokenProvider.KeyEntropyMode;
            this.IdentityVerifier = issuedSecurityTokenProvider.IdentityVerifier;
            this.CacheIssuedTokens = issuedSecurityTokenProvider.CacheIssuedTokens;
            this.MaxIssuedTokenCachingTime = issuedSecurityTokenProvider.MaxIssuedTokenCachingTime;
            this.MessageSecurityVersion = issuedSecurityTokenProvider.MessageSecurityVersion;
            this.SecurityTokenSerializer = issuedSecurityTokenProvider.SecurityTokenSerializer;
            this.SecurityAlgorithmSuite = issuedSecurityTokenProvider.SecurityAlgorithmSuite;
            this.IssuedTokenRenewalThresholdPercentage = issuedSecurityTokenProvider.IssuedTokenRenewalThresholdPercentage;
            if (issuedSecurityTokenProvider.IssuerChannelBehaviors != null && issuedSecurityTokenProvider.IssuerChannelBehaviors.Count > 0)
            {
                foreach (IEndpointBehavior behavior in issuedSecurityTokenProvider.IssuerChannelBehaviors)
                {
                    this.IssuerChannelBehaviors.Add(behavior);
                }
            }

            if (issuedSecurityTokenProvider.TokenRequestParameters != null && issuedSecurityTokenProvider.TokenRequestParameters.Count > 0)
            {
                foreach (XmlElement element in issuedSecurityTokenProvider.TokenRequestParameters)
                {
                    this.TokenRequestParameters.Add(element);
                }
            }
        }

        /// <summary>
        /// Asynchronously begins getting the token core.
        /// </summary>
        /// <param name="timeout">A <see cref="T:System.TimeSpan" /> after which the call times out.</param>
        /// <param name="callback">The <see cref="T:System.AsyncCallback" />.</param>
        /// <param name="state">A <see cref="T:System.Object" /> that represents the state.</param>
        /// <returns>
        /// An <see cref="T:System.IAsyncResult" />.
        /// </returns>
        protected override IAsyncResult BeginGetTokenCore(TimeSpan timeout, AsyncCallback callback, object state)
        {
            // Get token from cache
            CacheKey key = this.GetCacheKey();
            SecurityToken securityToken = this.GetServiceToken(key);
            if (securityToken != null)
            {
                return new TypedCompletedAsyncResult<SecurityToken>(securityToken, callback, state);
            }

            this.SetupActAsOnBehalfOfParameters();

            return base.BeginGetTokenCore(timeout, callback, state);
        }

        /// <summary>
        /// The asynchronous callback for getting the token core.
        /// </summary>
        /// <param name="result">An <see cref="T:System.IAsyncResult" />.</param>
        /// <returns>
        /// A <see cref="T:System.IdentityModel.Tokens.SecurityToken" />.
        /// </returns>
        protected override SecurityToken EndGetTokenCore(IAsyncResult result)
        {
            if (result is TypedCompletedAsyncResult<SecurityToken>)
            {
                return TypedCompletedAsyncResult<SecurityToken>.End(result);
            }

            SecurityToken securityToken = base.EndGetTokenCore(result);

            // Not sure that the key will remain unchanged
            CacheKey key = this.GetCacheKey();
            this.CacheServiceToken(key, securityToken);

            return securityToken;
        }

        /// <summary>
        /// Gets the token core.
        /// </summary>
        /// <param name="timeout">A <see cref="T:System.TimeSpan"/> after which the call times out.</param>
        /// <returns>
        /// The <see cref="T:System.IdentityModel.Tokens.SecurityToken"/> that represents the token core.
        /// </returns>
        protected override SecurityToken GetTokenCore(TimeSpan timeout)
        {
            // Get token from cache
            CacheKey key = this.GetCacheKey();
            SecurityToken securityToken = this.GetServiceToken(key);
            if (securityToken != null)
            {
                return securityToken;
            }

            this.SetupActAsOnBehalfOfParameters();

            // Get Token
            securityToken = base.GetTokenCore(timeout);

            // Cache Token
            this.CacheServiceToken(key, securityToken);

            return securityToken;
        }

        protected override void CancelTokenCore(TimeSpan timeout, SecurityToken token)
        {
            CacheKey key = this.GetCacheKey();
            lock (this.thisLock)
            {
                if (this.CacheIssuedTokens && this.tokenCache != null)
                {
                    this.tokenCache.TryRemoveEntry(key);
                }
            }

            base.CancelTokenCore(timeout, token);
        }

        private static DateTime TimeoutAdd(DateTime time, TimeSpan timeout)
        {
            if ((timeout >= TimeSpan.Zero) && ((DateTime.MaxValue - time) <= timeout))
            {
                return DateTime.MaxValue;
            }

            if ((timeout <= TimeSpan.Zero) && ((DateTime.MinValue - time) >= timeout))
            {
                return DateTime.MinValue;
            }

            return time + timeout;
        }

        private CacheKey GetCacheKey()
        {
            SecurityToken callerToken = this.clientCredentialsParamaters.OnBehalfOf;
            if (callerToken == null)
            {
                callerToken = this.clientCredentialsParamaters.ActAs;
            }

            if (callerToken != null)
            {
                return new CacheKey(this.TargetAddress.Uri, callerToken.Id);
            }

            return new CacheKey(this.TargetAddress.Uri);
        }

        private SecurityToken GetServiceToken(CacheKey key)
        {
            // Get token from cache
            lock (this.thisLock)
            {
                SecurityToken securityToken = null;
                if (this.CacheIssuedTokens
                    && this.tokenCache != null
                    && this.tokenCache.TryGetEntry(key, out securityToken)
                    && this.IsServiceTokenTimeValid(securityToken))
                {
                    return securityToken;
                }
            }

            return null;
        }

        private void CacheServiceToken(CacheKey key, SecurityToken securityToken)
        {
            lock (this.thisLock)
            {
                if (this.CacheIssuedTokens && this.tokenCache != null)
                {
                    this.tokenCache.TryAddEntry(key, securityToken);
                }
            }
        }

        private bool IsServiceTokenTimeValid(SecurityToken serviceToken)
        {
            if (serviceToken == null)
            {
                return false;
            }

            DateTime serviceTokenEffectiveExpirationTime = this.GetServiceTokenEffectiveExpirationTime(serviceToken);
            return DateTime.UtcNow <= serviceTokenEffectiveExpirationTime;
        }

        private DateTime GetServiceTokenEffectiveExpirationTime(SecurityToken serviceToken)
        {
            if (serviceToken == null)
            {
                throw new ArgumentNullException(nameof(serviceToken));
            }

            if (serviceToken.ValidTo.ToUniversalTime() >= new DateTime(DateTime.MaxValue.Ticks - 0xc92a69c000L, DateTimeKind.Utc))
            {
                return serviceToken.ValidTo;
            }

            TimeSpan span = serviceToken.ValidTo.ToUniversalTime() - serviceToken.ValidFrom.ToUniversalTime();
            long ticks = span.Ticks;

            long num2 = Convert.ToInt64((((double)this.IssuedTokenRenewalThresholdPercentage) / 100.0) * ticks, NumberFormatInfo.InvariantInfo);

            DateTime time = TimeoutAdd(serviceToken.ValidFrom.ToUniversalTime(), new TimeSpan(num2));
            DateTime time2 = TimeoutAdd(serviceToken.ValidFrom.ToUniversalTime(), this.MaxIssuedTokenCachingTime);

            if (time <= time2)
            {
                return time;
            }

            return time2;
        }

        private void SetupActAsOnBehalfOfParameters()
        {
            if (this.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrustFeb2005)
            {
                if (this.clientCredentialsParamaters.OnBehalfOf != null)
                {
                    this.RemoveTokenRequestParameter(WsTrustConstants.ElementNames.OnBehalfOf, WsTrustConstants.Namespaces.WsTrust2005);
                    this.TokenRequestParameters.Add(this.CreateXmlTokenElement(this.clientCredentialsParamaters.OnBehalfOf, WsTrustConstants.Prefixes.WsTrust2005, WsTrustConstants.ElementNames.OnBehalfOf, WsTrustConstants.Namespaces.WsTrust2005, SecurityTokenHandlerCollectionManager.Usage.OnBehalfOf));
                }
            }
            else if (this.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrust13)
            {
                if (this.clientCredentialsParamaters.OnBehalfOf != null)
                {
                    this.RemoveTokenRequestParameter(WsTrustConstants.ElementNames.OnBehalfOf, WsTrustConstants.Namespaces.WsTrust13);
                    this.TokenRequestParameters.Add(this.CreateXmlTokenElement(this.clientCredentialsParamaters.OnBehalfOf, WsTrustConstants.Prefixes.WsTrust13, WsTrustConstants.ElementNames.OnBehalfOf, WsTrustConstants.Namespaces.WsTrust13, SecurityTokenHandlerCollectionManager.Usage.OnBehalfOf));
                }
            }

            if (this.clientCredentialsParamaters.ActAs != null)
            {
                this.RemoveTokenRequestParameter(WsTrustConstants.ElementNames.ActAs, WsTrustConstants.Namespaces.WsTrust14);
                this.TokenRequestParameters.Add(this.CreateXmlTokenElement(this.clientCredentialsParamaters.ActAs, WsTrustConstants.Prefixes.WsTrust14, WsTrustConstants.ElementNames.ActAs, WsTrustConstants.Namespaces.WsTrust14, SecurityTokenHandlerCollectionManager.Usage.ActAs));
            }
        }

        private void RemoveTokenRequestParameter(string localName, string xmlNamespace)
        {
            var element = this.TokenRequestParameters.FirstOrDefault(e => e.LocalName == localName && e.NamespaceURI == xmlNamespace);
            this.TokenRequestParameters.Remove(element);
        }

        private XmlElement CreateXmlTokenElement(SecurityToken token, string prefix, string name, string ns, string usage)
        {
            Stream stream = new MemoryStream();
            using (XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(stream, System.Text.Encoding.UTF8, false))
            {
                writer.WriteStartElement(prefix, name, ns);
                this.WriteToken(writer, token, usage);
                writer.WriteEndElement();
                writer.Flush();
            }

            stream.Seek(0L, SeekOrigin.Begin);
            XmlDocument document = new XmlDocument { XmlResolver = null, PreserveWhitespace = true };
            document.Load(XmlReader.Create(stream, new XmlReaderSettings() { XmlResolver = null }));
            stream.Close();
            return document.DocumentElement;
        }

        private void WriteToken(XmlWriter xmlWriter, SecurityToken token, string usage)
        {
            SecurityTokenHandlerCollectionManager manager = this.securityTokenHandlerCollectionManager;
            SecurityTokenHandlerCollection handlers = null;
            if (manager.ContainsKey(usage))
            {
                handlers = manager[usage];
            }
            else
            {
                handlers = manager[string.Empty];
            }

            if (handlers != null && handlers.CanWriteToken(token))
            {
                handlers.WriteToken(xmlWriter, token);
            }
            else
            {
                this.SecurityTokenSerializer.WriteToken(xmlWriter, token);
            }
        }

        private class CacheKey
        {
            public CacheKey(Uri targetAddress)
                : this(targetAddress, null)
            {
            }

            public CacheKey(Uri targetAddress, string tokenId)
            {
                if (targetAddress == null)
                {
                    throw new ArgumentNullException(nameof(targetAddress));
                }

                this.TargetAddress = targetAddress;
                this.TokenId = tokenId;
            }

            public Uri TargetAddress { get; private set; }

            public string TokenId { get; private set; }

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

            /// <inheritdoc/>
            public override bool Equals(object obj)
            {
                if (!(obj is CacheKey))
                {
                    return false;
                }

                var key = (CacheKey)obj;

                bool flag = key.TargetAddress != this.TargetAddress;
                if (flag)
                {
                    flag = StringComparer.Ordinal.Equals(key.TokenId, this.TokenId);
                }

                return flag;
            }

            /// <inheritdoc/>
            public override int GetHashCode()
            {
                if (this.TokenId == null)
                {
                    return this.TargetAddress.GetHashCode();
                }

                return this.TargetAddress.GetHashCode() ^ this.TokenId.GetHashCode();
            }

            /// <inheritdoc/>
            public override string ToString()
            {
                return this.GetHashCode().ToString();
            }
        }
    }
}