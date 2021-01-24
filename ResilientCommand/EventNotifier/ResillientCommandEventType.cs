namespace ResilientCommand
{
    public enum ResillientCommandEventType
    {
        None = 0,
        TimedOut,
        CircuitBroken,
        CircuitReset,
        FallbackUsed,
        FallbackSkipped,
        ExceptionThrown,
        CachedResponseUsed,
    }
}
