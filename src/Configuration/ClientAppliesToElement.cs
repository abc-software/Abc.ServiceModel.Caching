// ----------------------------------------------------------------------------
// <copyright file="ClientAppliesToElement.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

namespace Abc.ServiceModel.Caching.Configuration
{
    using Abc.ServiceModel.Caching;
    using System;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Configuration;

    /// <summary>
    /// Business logic router endpoint behavior element.
    /// </summary>
    public class ClientAppliesToElement : BehaviorExtensionElement
    {
        private ConfigurationPropertyCollection properties;

        /// <summary>
        /// Gets or sets the URI for the appliesTo.
        /// </summary>
        [ConfigurationProperty("address", DefaultValue = null, Options = ConfigurationPropertyOptions.IsRequired)]
        public Uri Address
        {
            get
            {
                return (Uri)base["address"];
            }

            set
            {
                base["address"] = value;
            }
        }

        /// <summary>
        /// Gets the collection of address headers for the appliesTo that the builder can create.
        /// </summary>
        [ConfigurationProperty("headers")]
        public AddressHeaderCollectionElement Headers
        {
            get
            {
                return (AddressHeaderCollectionElement)base["headers"];
            }
        }

        /// <summary>
        /// Gets the identity for the appliesTo.
        /// </summary>
        [ConfigurationProperty("identity")]
        public IdentityElement Identity
        {
            get
            {
                return (IdentityElement)base["identity"];
            }
        }

        /// <inheritdoc/>
        public override Type BehaviorType
        {
            get { return typeof(ClientAppliesToBehavior); }
        }

        /// <inheritdoc/>
        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (properties == null)
                {
                    var propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("address", typeof(Uri), null, null, null, ConfigurationPropertyOptions.IsRequired));
                    propertys.Add(new ConfigurationProperty("headers", typeof(AddressHeaderCollectionElement), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("identity", typeof(IdentityElement), null, null, null, ConfigurationPropertyOptions.None));
                    properties = propertys;
                }

                return properties;
            }
        }

        /// <inheritdoc/>
        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            var element = (ClientAppliesToElement)from;
            Address = element.Address;
            Headers.Headers = element.Headers.Headers;

            if (element.ElementInformation.Properties["identity"].ValueOrigin != PropertyValueOrigin.Default)
            {
                CopyIdentity(element.Identity);
            }
        }

        /// <inheritdoc/>
        protected override object CreateBehavior()
        {
            return new ClientAppliesToBehavior(new EndpointAddress(Address, WcfConfigLoader.LoadIdentity(Identity), Headers.Headers));
        }

        private void CopyIdentity(IdentityElement source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var properties = source.ElementInformation.Properties;
            if (properties["userPrincipalName"].ValueOrigin != PropertyValueOrigin.Default)
            {
                Identity.UserPrincipalName.Value = source.UserPrincipalName.Value;
            }

            if (properties["servicePrincipalName"].ValueOrigin != PropertyValueOrigin.Default)
            {
                Identity.ServicePrincipalName.Value = source.ServicePrincipalName.Value;
            }

            if (properties["certificate"].ValueOrigin != PropertyValueOrigin.Default)
            {
                Identity.Certificate.EncodedValue = source.Certificate.EncodedValue;
            }

            if (properties["certificateReference"].ValueOrigin != PropertyValueOrigin.Default)
            {
                Identity.CertificateReference.StoreName = source.CertificateReference.StoreName;
                Identity.CertificateReference.StoreLocation = source.CertificateReference.StoreLocation;
                Identity.CertificateReference.X509FindType = source.CertificateReference.X509FindType;
                Identity.CertificateReference.FindValue = source.CertificateReference.FindValue;
            }
        }
    }
}