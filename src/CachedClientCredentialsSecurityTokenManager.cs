// ----------------------------------------------------------------------------
// <copyright file="CachedClientCredentialsSecurityTokenManager.cs" company="ABC software Ltd">
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
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
#if WIF35
    using Microsoft.IdentityModel.Protocols.WSTrust;
#endif

    /// <summary>
    /// Manages security tokens for the client.
    /// </summary>
    internal class CachedClientCredentialsSecurityTokenManager : ClientCredentialsSecurityTokenManager
    {
        private readonly CachedClientCredentials clientCredentials;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedClientCredentialsSecurityTokenManager"/> class.
        /// </summary>
        /// <param name="clientCredentials">The client credentials.</param>
        /// <remarks>
        /// Idea http://wcfguidanceforwpf.codeplex.com"
        /// </remarks>
        public CachedClientCredentialsSecurityTokenManager(CachedClientCredentials clientCredentials)
            : base(clientCredentials)
        {
            this.clientCredentials = clientCredentials;
        }

        /// <summary>
        /// Creates a security token provider.
        /// </summary>
        /// <param name="tokenRequirement">The <see cref="T:System.IdentityModel.Selectors.SecurityTokenRequirement"/>.</param>
        /// <returns>
        /// The <see cref="T:System.IdentityModel.Selectors.SecurityTokenProvider"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="tokenRequirement"/> is null.</exception>
        public override SecurityTokenProvider CreateSecurityTokenProvider(SecurityTokenRequirement tokenRequirement)
        {
            if (!this.IsIssuedSecurityTokenRequirement(tokenRequirement))
            {
                return base.CreateSecurityTokenProvider(tokenRequirement);
            }

            var provider = base.CreateSecurityTokenProvider(tokenRequirement);
            if (provider is SimpleSecurityTokenProvider)
            {
                return provider;
            }

            var issuedProvider = provider as IssuedSecurityTokenProvider;
            if (issuedProvider != null)
            {
                if (this.clientCredentials.AppliesTo != null)
                {
                    issuedProvider.TargetAddress = this.clientCredentials.AppliesTo;
                }

                return new CachedIssuedSecurityTokenProvider(this.clientCredentials.TokenCache, issuedProvider, FindIssuedTokenClientCredentialsParameters(tokenRequirement), clientCredentials.SecurityTokenHandlerCollectionManager);
            }

            return provider;
        }

        internal static CachedClientCredentialsParameters FindIssuedTokenClientCredentialsParameters(SecurityTokenRequirement tokenRequirement)
        {
            if (tokenRequirement == null)
            {
                throw new ArgumentNullException(nameof(tokenRequirement));
            }

            CachedClientCredentialsParameters parameters = null;
            ChannelParameterCollection result = null;
            if (tokenRequirement.TryGetProperty<ChannelParameterCollection>(ServiceModelSecurityTokenRequirement.ChannelParametersCollectionProperty, out result) && (result != null))
            {
                parameters = result.OfType<CachedClientCredentialsParameters>().FirstOrDefault();
            }

            if (parameters == null)
            {
                parameters = new CachedClientCredentialsParameters();
            }

            return parameters;
        }
    }
}