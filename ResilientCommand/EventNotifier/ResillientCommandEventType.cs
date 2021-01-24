namespace ResilientCommand
{
    public enum ResillientCommandEventType
    {
        None = 0,
        TimedOut,
        CircuitBroken,
        CircuitReset,
        FallbackSuccess,
        FallbackSkipped,
        ExceptionThrown,
        ResponseFromCache,
    }
}
