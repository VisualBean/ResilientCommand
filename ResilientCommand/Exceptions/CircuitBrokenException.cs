using System;
using System.Runtime.Serialization;

namespace ResilientCommand
{
    [Serializable]
    internal class CircuitBrokenException : Exception
    {
        const string exceptionMessage = "Circuit has been broken";
        public CircuitBrokenException(Exception innerException) : base(exceptionMessage, innerException)
        {
        }
    }
}