// ----------------------------------------------------------------------------
// <copyright file="TypedCompletedAsyncResult.cs" company="ABC software Ltd">
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

    internal class TypedCompletedAsyncResult<T> : TypedAsyncResult<T>
    {
        public TypedCompletedAsyncResult(T result, AsyncCallback callback, object state)
            : base(callback, state)
        {
            this.Complete(result, true);
        }

        public TypedCompletedAsyncResult(T result, AsyncCallback callback, object state, Exception exception)
            : base(callback, state)
        {
            this.Complete(result, true, exception);
        }

        public static T EndWithDispose(IAsyncResult result)
        {
            return result.EndWithDispose<T, TypedCompletedAsyncResult<T>>();
        }

        public static new T End(IAsyncResult result)
        {
            if (!(result is TypedAsyncResult<T>))
            {
                throw new ArgumentException($"Invalid argument type. Expected '{nameof(TypedAsyncResult<T>)}'.", nameof(result));
            }

            return TypedAsyncResult<T>.End(result);
        }
    }
}