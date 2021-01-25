namespace ResilientCommand
{
    public sealed class CircuitBreakerSettings
    {
        public CircuitBreakerSettings(
            bool isEnabled = Default.IsEnabled,
            double failureThreshhold = Default.FailureThreshhold,
            int samplingDurationMiliseconds = Default.SamplingDurationMiliseconds,
            int minimumThroughput = Default.MinimumThroughput,
            int durationMiliseconds = Default.DurationMiliseconds)
        {
            IsEnabled = isEnabled;
            FailureThreshhold = failureThreshhold;
            SamplingDurationMiliseconds = samplingDurationMiliseconds;
            MinimumThroughput = minimumThroughput;
            DurationMiliseconds = durationMiliseconds;
        }

        public bool IsEnabled { get; private set; }
        public double FailureThreshhold { get; private set; }
        public int SamplingDurationMiliseconds { get; private set; }
        public int MinimumThroughput { get; private set; }
        public int DurationMiliseconds { get; private set; }

        public sealed class Default
        {
            public const double FailureThreshhold = 0.5;
            public const int SamplingDurationMiliseconds = 10000;
            public const int MinimumThroughput = 20;
            public const int DurationMiliseconds = 5000;
            public const bool IsEnabled = true;
        }

        public static CircuitBreakerSettings DefaultCircuitBreakerSettings => new CircuitBreakerSettings();
    }
}
