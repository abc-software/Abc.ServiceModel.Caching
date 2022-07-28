// ----------------------------------------------------------------------------
// <copyright file="ClientAppliesToBehavior.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

namespace Abc.ServiceModel.Caching
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    /// <summary>
    /// Business logic router endpoint behavior element.
    /// </summary>
    internal class ClientAppliesToBehavior : IEndpointBehavior
    {
        private EndpointAddress appliesTo;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientAppliesToBehavior"/> class.
        /// </summary>
        /// <param name="appliesTo">The apply to.</param>
        public ClientAppliesToBehavior(EndpointAddress appliesTo)
        {
            this.appliesTo = appliesTo ?? throw new ArgumentNullException(nameof(appliesTo));
        }

        /// <summary>
        /// Gets or sets the contents of the <c>wsp:AppliesTo</c> element.
        /// </summary>
        public EndpointAddress AppliesTo
        {
            get
            {
                return this.appliesTo;
            }

            set
            {
                this.appliesTo = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <inheritdoc/>
        public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
            var item = endpoint.Behaviors.Find<CachedClientCredentials>();
            if (item == null)
            {
                var other = endpoint.Behaviors.Remove<ClientCredentials>();
                if (other == null)
                {
                    throw new InvalidOperationException("The ChannelFactory does not have a ClientCredentials object in its Endpoint.Behaviors.");
                }

                item = new CachedClientCredentials(other);
                endpoint.Behaviors.Add(item);
            }

            item.AppliesTo = this.appliesTo;
        }

        /// <inheritdoc/>
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            // do nothing
        }

        /// <inheritdoc/>
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            // EndpointBehaviorUsedOnWrongSide
            throw new InvalidOperationException(string.Format("The IEndpointBehavior '{0}' cannot be used on the server side; this behavior can only be applied to clients.", this.GetType().Name));
        }

        /// <inheritdoc/>
        public void Validate(ServiceEndpoint endpoint)
        {
            // do nothing
        }
    }
}