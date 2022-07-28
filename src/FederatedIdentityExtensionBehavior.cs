// ----------------------------------------------------------------------------
// <copyright file="FederatedIdentityExtensionBehavior.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

namespace Abc.ServiceModel.Caching
{
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    /// <summary>
    /// The federated identity extensions Behavior
    /// </summary>
    internal class FederatedIdentityExtensionBehavior : IEndpointBehavior
    {
        private readonly string usage;

        /// <summary>
        /// Initializes a new instance of the <see cref="FederatedIdentityExtensionBehavior"/> class.
        /// </summary>
        /// <param name="usage">The usage in client processing.</param>
        public FederatedIdentityExtensionBehavior(string usage)
        {
            this.usage = usage;
        }

        /// <inheritdoc/>
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        /// <inheritdoc/>
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.ChannelInitializers.Add(new CachedChannelInitializer(this.usage));
        }

        /// <inheritdoc/>
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        /// <inheritdoc/>
        public void Validate(ServiceEndpoint endpoint)
        {
            var other = endpoint.Behaviors.Find<ClientCredentials>();
            if (other != null)
            {
                endpoint.Behaviors.Remove(other.GetType());
                var item = new CachedClientCredentials(other);
                endpoint.Behaviors.Add(item);
            }
        }
    }
}