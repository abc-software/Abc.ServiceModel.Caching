// ----------------------------------------------------------------------------
// <copyright file="ChannelFactoryOperations.cs" company="ABC software Ltd">
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
#if WIF35
    using Microsoft.IdentityModel.Tokens;
#endif

    /// <summary>
    /// A utility class that defines methods for creating various types of channels. 
    /// </summary>
    public static class ChannelFactoryOperations
    {
        /// <summary>
        /// Configures the specified ChannelFactory to use FederatedClientCredentials to provide additional features for requesting issued tokens.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channelFactory">The channel factory.</param>
        /// <param name="cache">The cache.</param>
        public static void ConfigureChannelFactory<T>(this ChannelFactory<T> channelFactory, SecurityTokenCache cache)
        {
            ConfigureChannelFactory<T>(channelFactory, cache, null);
        }

        /// <summary>
        /// Configures the channel factory.
        /// </summary>
        /// <typeparam name="T">The type of channel produced by the channel factory.</typeparam>
        /// <param name="channelFactory">The channel factory to configure.</param>
        /// <param name="cache">The security token cache.</param>
        /// <param name="appliesTo">The applies to.</param>
        public static void ConfigureChannelFactory<T>(this ChannelFactory<T> channelFactory, SecurityTokenCache cache, Uri appliesTo)
        {
            if (channelFactory == null)
            {
                throw new ArgumentNullException(nameof(channelFactory));
            }

            if (channelFactory.State != CommunicationState.Created && channelFactory.State != CommunicationState.Opening)
            {
                throw new InvalidOperationException("The ChannelFactory cannot be configured. Invoke this method prior to opening the ChannelFactory.");
            }

            var other = channelFactory.Endpoint.Behaviors.Find<ClientCredentials>();
            if (other == null)
            {
                throw new InvalidOperationException("The ChannelFactory does not have a ClientCredentials object in its Endpoint.Behaviors.");
            }

            channelFactory.Endpoint.Behaviors.Remove(other.GetType());
            var item = new CachedClientCredentials(cache, other);
            if (appliesTo != null)
            {
                item.AppliesTo = new EndpointAddress(appliesTo);
            }

            channelFactory.Endpoint.Behaviors.Add(item);
        }
    }
}