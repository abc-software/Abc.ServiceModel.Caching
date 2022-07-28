// ----------------------------------------------------------------------------
// <copyright file="IAsyncResultExtension.cs" company="ABC software Ltd">
//    Copyright © ABC SOFTWARE. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0.
//    See LICENSE in the project root for license information.
// </copyright>
// ----------------------------------------------------------------------------

namespace Abc.ServiceModel.Threading
{
    using System;
#if WIF35
    using Microsoft.IdentityModel.Threading;
#else
    using System.IdentityModel;
#endif

    internal static class IAsyncResultExtension
    {
        public static T EndWithDispose<T, A>(this IAsyncResult result)
            where A : TypedAsyncResult<T>
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (!(result is A))
            {
                throw new ArgumentException("Invalid type", nameof(result));
            }

            using (var completedResult = (TypedAsyncResult<T>)result)
            {
                return TypedAsyncResult<T>.End(completedResult);
            }
        }
    }
}