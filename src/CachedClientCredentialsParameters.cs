// ----------------------------------------------------------------------------
// <copyright file="CachedClientCredentialsParameters.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

namespace Abc.ServiceModel.Caching
{
    using System.IdentityModel.Tokens;

    /// <summary>
    /// Encapsulates properties that control the token retrieval logic of CachedIssuedSecurityTokenProvider objects. 
    /// </summary>
    internal class CachedClientCredentialsParameters
    {
        /// <summary>
        /// Gets or sets the security token sent to the security token service (STS) in the wst:ActAs element.
        /// </summary>
        public SecurityToken ActAs { get; set; }

        /// <summary>
        /// Gets or sets the security token sent to the security token service as the wst:OnBehalfOf element. 
        /// </summary>
        public SecurityToken OnBehalfOf { get; set; }
    }
}