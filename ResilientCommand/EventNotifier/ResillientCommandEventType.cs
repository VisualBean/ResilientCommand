namespace ResilientCommand
{
    public enum ResillientCommandEventType
    {
        None = 0,
        Failure,
        Success,
        TimedOut,
        CircuitBroken,
        CircuitReset,
        FallbackSuccess,
        FallbackMissing,
        FallbackDisabled,
        ResponseFromCache,
        Collapsed,
    }
}
