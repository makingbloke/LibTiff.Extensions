// Copyright (c)2021 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using System;
using System.Runtime.Serialization;

namespace dotDoc.LibTiff.Extensions.Exceptions
{
    [Serializable]
    public class UtilityExtensionsException : Exception
    {
        public UtilityExtensionsException()
        {
        }

        public UtilityExtensionsException(string message)
            : base(message)
        {
        }

        public UtilityExtensionsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected UtilityExtensionsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
