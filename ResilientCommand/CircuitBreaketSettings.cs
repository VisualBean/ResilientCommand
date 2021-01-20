using System;
using System.Collections.Generic;
using System.Text;

namespace ResilientCommand
{
    public class CircuitBreakerSettings
    {
        public CircuitBreakerSettings(
            double failureThreshhold = Default.FailureThreshhold,
            int samplingDurationMiliseconds = Default.SamplingDurationMiliseconds,
            int minimumThroughput = Default.MinimumThroughput,
            int durationMiliseconds = Default.DurationMiliseconds)
        {
            FailureThreshhold = failureThreshhold;
            SamplingDurationMiliseconds = samplingDurationMiliseconds;
            MinimumThroughput = minimumThroughput;
            DurationMiliseconds = durationMiliseconds;
        }

        public double FailureThreshhold { get; private set; }
        public int SamplingDurationMiliseconds { get; private set; }
        public int MinimumThroughput { get; private set; }
        public int DurationMiliseconds { get; private set; }

        public class Default
        {
            public const double FailureThreshhold = 0.5;
            public const int SamplingDurationMiliseconds = 10000;
            public const int MinimumThroughput = 20;
            public const int DurationMiliseconds = 5000;
        }

        public static CircuitBreakerSettings DefaultCircuitBreakerSettings => new CircuitBreakerSettings();
    }
}
