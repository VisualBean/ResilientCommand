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

        public static ExecutionTimeoutSettings DefaultExecutionTimeoutSettings => new ExecutionTimeoutSettings();

        public int ExecutionTimeoutInMiliseconds { get; private set; }

        public bool IsEnabled { get; set; }

        public sealed class Default
        {
            public const int ExecutionTimeoutInMiliseconds = 1000;
            public const bool IsEnabled = true;
        }
    }
}
