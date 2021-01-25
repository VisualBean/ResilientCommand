namespace ResilientCommand
{
    public sealed class ExecutionTimeoutSettings
    {
        public ExecutionTimeoutSettings(
            bool isEnabled = Default.IsEnabled,
            int executionTimeoutInMiliseconds = Default.ExecutionTimeoutInMiliseconds)
        {
            IsEnabled = isEnabled;
            ExecutionTimeoutInMiliseconds = executionTimeoutInMiliseconds;
        }

        public bool IsEnabled { get; private set; }
        public int ExecutionTimeoutInMiliseconds { get; private set; }

        public sealed class Default
        {
            public const bool IsEnabled = true;
            public const int ExecutionTimeoutInMiliseconds = 10000;
        }

        public static ExecutionTimeoutSettings DefaultExecutionTimeoutSettings => new ExecutionTimeoutSettings();
    }
}
