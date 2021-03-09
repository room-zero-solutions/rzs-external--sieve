using System;

namespace Sieve.Exceptions
{
    public class RzSieveException : Exception
    {
        public RzSieveException(string message) : base(message)
        {
        }

        public RzSieveException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public RzSieveException()
        {
        }

        protected RzSieveException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}
