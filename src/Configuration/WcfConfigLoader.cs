// ----------------------------------------------------------------------------
// <copyright file="WcfConfigLoader.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

namespace Abc.ServiceModel.Caching.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Reflection;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;

    /// <summary>
    /// The Reflected ConfigLoader.
    /// </summary>
    internal class WcfConfigLoader
    {
        [SecuritySafeCritical]
        internal static EndpointAddress LoadEndpointAddress(EndpointAddressElementBase element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            return new EndpointAddress(element.Address, LoadIdentity(element.Identity), element.Headers.Headers);
        }

        [SecuritySafeCritical]
        internal static EndpointIdentity LoadIdentity(IdentityElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            return (EndpointIdentity)CallStaticPrivateMethod("LoadIdentity", element);
        }

        [SecuritySafeCritical]
        internal static Binding LookupBinding(string bindingSectionName, string configurationName, ContextInformation context)
        {
            if (string.IsNullOrEmpty(bindingSectionName))
            {
                throw new ArgumentException("Must be set value.", nameof(bindingSectionName));
            }

            return (Binding)CallStaticPrivateMethod("LookupBinding", bindingSectionName, configurationName, context);
        }

        [SecuritySafeCritical]
        internal static void LoadChannelBehaviors(string behaviorName, ContextInformation context, KeyedByTypeCollection<IEndpointBehavior> channelBehaviors)
        {
            CallStaticPrivateMethod("LoadChannelBehaviors", behaviorName, context, channelBehaviors);
        }

        private static object CallStaticPrivateMethod(string methodName, params object[] parameters)
        {
            if (methodName == null)
            {
                throw new ArgumentNullException(nameof(methodName));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var type = typeof(Binding).Assembly.GetType("System.ServiceModel.Description.ConfigLoader");

            var paramTypes = Array.ConvertAll(parameters, new Converter<object, Type>(delegate (object o) { return o.GetType(); }));

            var method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static, null, paramTypes, null);
            if (method == null)
            {
                throw new ArgumentException($"Could not find a method with the name '{methodName}'", "methodName");
            }

            return method.Invoke(null, parameters);
        }
    }
}