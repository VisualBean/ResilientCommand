using System;

namespace ResilientCommand
{
    [Serializable]
    public class CircuitBrokenException : Exception
    {
        const string exceptionMessage = "Circuit has been broken";

        public CircuitBrokenException(CommandKey commandKey) : base($"{exceptionMessage} for {commandKey}.")
        {

        }
        public CircuitBrokenException(CommandKey commandKey, Exception innerException) : base($"{exceptionMessage} for {commandKey}.", innerException)
        {
        }
    }
}