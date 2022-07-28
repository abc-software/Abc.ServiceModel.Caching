// ----------------------------------------------------------------------------
// <copyright file="CachedChannelInitializer.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

namespace Abc.ServiceModel.Caching
{
    using System;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Xml;
#if WIF35
    using Microsoft.IdentityModel.Claims;
    using Microsoft.IdentityModel.Protocols.WSTrust;
    using Microsoft.IdentityModel.Tokens.Saml2;
#else
    using System.IdentityModel.Services;
    using System.Security.Claims;
#endif

    internal class CachedChannelInitializer : IChannelInitializer
    {
        private readonly string usage;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedChannelInitializer"/> class.
        /// </summary>
        /// <param name="usage">The usage.</param>
        public CachedChannelInitializer(string usage)
        {
            this.usage = usage ?? string.Empty;
        }

        /// <inheritdoc/>
        public void Initialize(IClientChannel channel)
        {
            SecurityToken callerToken = null;

            var claimsPrincipal = System.Threading.Thread.CurrentPrincipal as ClaimsPrincipal;
            if (claimsPrincipal != null)
            {
                foreach (var claimsIdentity in claimsPrincipal.Identities)
                {
#if WIF35
                    callerToken = claimsIdentity.BootstrapToken;
#else
                    var context = claimsIdentity.BootstrapContext as BootstrapContext;
                    if (context == null)
                    {
                        continue;
                    }

                    if (context.SecurityToken != null)
                    {
                        callerToken = context.SecurityToken;
                    }
                    else if (!string.IsNullOrEmpty(context.Token))
                    {
                        // If however the websites app domain is reset
                        var handlers = FederatedAuthentication.FederationConfiguration.IdentityConfiguration.SecurityTokenHandlers;
                        using (var reader = XmlReader.Create(new StringReader(context.Token), new XmlReaderSettings() { XmlResolver = null }))
                        {
                            callerToken = handlers.ReadToken(reader);
                        }
                    }
#endif
                    if (callerToken is SamlSecurityToken || callerToken is Saml2SecurityToken)
                    {
                        break;
                    }
                }
            }

            if (null != callerToken)
            {
                var parameters = new CachedClientCredentialsParameters();

                if (string.Equals("ActAs", usage, StringComparison.OrdinalIgnoreCase))
                {
                    parameters.ActAs = callerToken;
                }
                else if (string.Equals("OnBehalfOf", usage, StringComparison.OrdinalIgnoreCase))
                {
                    parameters.OnBehalfOf = callerToken;
                }

                try
                {
                    channel.GetProperty<ChannelParameterCollection>().Add(parameters);
                }
                catch (Exception)
                {
                    // TODO: validate
                }
            }
        }
    }
}