// ----------------------------------------------------------------------------
// <copyright file="CachedClientCredentials.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

namespace Abc.ServiceModel.Caching
{
    using System;
    using System.IdentityModel.Selectors;
    using System.ServiceModel;
    using System.ServiceModel.Description;
#if WIF35
    using Microsoft.IdentityModel.Protocols.WSTrust;
    using Microsoft.IdentityModel.Tokens;
#else
    using System.IdentityModel.Tokens;
#endif

    /// <summary>
    /// Enables the user to configure client credentials with issued token cache.
    /// </summary>
    /// <remarks>
    /// Idea http://wcfguidanceforwpf.codeplex.com"
    /// </remarks>
    internal class CachedClientCredentials
#if WIF35
        : FederatedClientCredentials 
#else
        : ClientCredentials
#endif
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CachedClientCredentials"/> class.
        /// </summary>
        public CachedClientCredentials()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedClientCredentials"/> class.
        /// </summary>
        /// <param name="clientCredentials">The client credentials.</param>
        public CachedClientCredentials(ClientCredentials clientCredentials)
            : this(null, clientCredentials)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedClientCredentials" /> class.
        /// </summary>
        /// <param name="tokenCache">The token cache.</param>
        /// <param name="clientCredentials">The client credentials.</param>
        public CachedClientCredentials(SecurityTokenCache tokenCache, ClientCredentials clientCredentials)
            : base(clientCredentials)
        {
            this.TokenCache = tokenCache;
        }

#if WIF35
        /// <summary>
        /// Initializes a new instance of the <see cref="CachedClientCredentials"/> class.
        /// </summary>
        /// <param name="clientCredentials">The client credentials.</param>
        /// <param name="securityTokenHandlerCollectionManager">The security token handler collection manager.</param>
        public CachedClientCredentials(ClientCredentials clientCredentials, SecurityTokenHandlerCollectionManager securityTokenHandlerCollectionManager)
            : this(null, clientCredentials, securityTokenHandlerCollectionManager) 
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedClientCredentials" /> class.
        /// </summary>
        /// <param name="tokenCache">The token cache.</param>
        /// <param name="clientCredentials">The client credentials.</param>
        /// <param name="securityTokenHandlerCollectionManager">The security token handler collection manager.</param>
        public CachedClientCredentials(SecurityTokenCache tokenCache, ClientCredentials clientCredentials, SecurityTokenHandlerCollectionManager securityTokenHandlerCollectionManager)
            : base(clientCredentials, securityTokenHandlerCollectionManager) 
        {
            this.TokenCache = tokenCache;
        }
#endif

        /// <summary>
        /// Gets the token cache.
        /// </summary>
        public SecurityTokenCache TokenCache { get; private set; }

        /// <summary>
        /// Gets or sets the applies to.
        /// </summary>
        public EndpointAddress AppliesTo { get; set; }

        /// <summary>
        /// Creates a security token manager for this instance. This method is rarely called explicitly; it is primarily used in extensibility scenarios and is called by the system itself.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.ServiceModel.ClientCredentialsSecurityTokenManager"/> for this <see cref="T:System.ServiceModel.Description.ClientCredentials"/> instance.
        /// </returns>
        public override SecurityTokenManager CreateSecurityTokenManager()
        {
            return new CachedClientCredentialsSecurityTokenManager((CachedClientCredentials)this.Clone());
        }

        /// <summary>
        /// Creates a new copy of this <see cref="T:System.ServiceModel.Description.ClientCredentials"/> instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.ServiceModel.Description.ClientCredentials"/> instance.
        /// </returns>
        protected override ClientCredentials CloneCore()
        {
#if WIF35
            return new CachedClientCredentials(this, this.SecurityTokenHandlerCollectionManager) { AppliesTo = this.AppliesTo, TokenCache = this.TokenCache };
#else
            return new CachedClientCredentials(this) { AppliesTo = this.AppliesTo, TokenCache = this.TokenCache };
#endif
        }
    }
}