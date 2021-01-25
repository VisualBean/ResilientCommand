using System;

namespace ResilientCommand
{
    [Serializable]
    public class FallbackNotImplementedException : Exception
    {
        public FallbackNotImplementedException(CommandKey commandKey, Exception innerException) : base($"Fallback not implemented for command: {commandKey}. Either implement fallback or disable it through configuration.", innerException)
        {
        }
    }
}