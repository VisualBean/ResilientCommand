// <copyright file="CircuitBreakerSettings.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand
{
    /// <summary>
    /// Settings class for the CircuitBreaker.
    /// </summary>
    public sealed class CircuitBreakerSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CircuitBreakerSettings"/> class.
        /// </summary>
        /// <param name="isEnabled">if set to <c>true</c> [is enabled].</param>
        /// <param name="failureThreshhold">The failure threshhold.</param>
        /// <param name="samplingDurationMilliseconds">The sampling duration milliseconds.</param>
        /// <param name="minimumThroughput">The minimum throughput.</param>
        /// <param name="durationMilliseconds">The duration milliseconds.</param>
        public CircuitBreakerSettings(
            bool isEnabled = Default.IsEnabled,
            double failureThreshhold = Default.FailureThreshhold,
            int samplingDurationMilliseconds = Default.SamplingDurationMilliseconds,
            int minimumThroughput = Default.MinimumThroughput,
            int durationMilliseconds = Default.DurationMilliseconds)
        {
            this.IsEnabled = isEnabled;
            this.FailureThreshhold = failureThreshhold;
            this.SamplingDurationMilliseconds = samplingDurationMilliseconds;
            this.MinimumThroughput = minimumThroughput;
            this.DurationMilliseconds = durationMilliseconds;
        }

        /// <summary>
        /// Gets the default circuit breaker settings.
        /// </summary>
        /// <value>
        /// The default circuit breaker settings.
        /// </value>
        public static CircuitBreakerSettings DefaultCircuitBreakerSettings => new CircuitBreakerSettings();

        /// <summary>
        /// Gets the duration milliseconds.
        /// </summary>
        /// <value>
        /// The duration milliseconds.
        /// </value>
        public int DurationMilliseconds { get; private set; }

        /// <summary>
        /// Gets the failure threshhold.
        /// </summary>
        /// <value>
        /// The failure threshhold.
        /// </value>
        public double FailureThreshhold { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether circuit breaker is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if circuit breaker is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets the minimum throughput.
        /// </summary>
        /// <value>
        /// The minimum throughput.
        /// </value>
        public int MinimumThroughput { get; private set; }

        /// <summary>
        /// Gets the sampling duration milliseconds.
        /// </summary>
        /// <value>
        /// The sampling duration milliseconds.
        /// </value>
        public int SamplingDurationMilliseconds { get; private set; }

        /// <summary>
        /// Default settings.
        /// </summary>
        public sealed class Default
        {
            /// <summary>
            /// The default duration milliseconds.
            /// </summary>
            public const int DurationMilliseconds = 5000;

            /// <summary>
            /// The default failure threshold.
            /// </summary>
            public const double FailureThreshhold = 0.5;

            /// <summary>
            /// The default is enabled.
            /// </summary>
            public const bool IsEnabled = true;

            /// <summary>
            /// The default minimum throughput.
            /// </summary>
            public const int MinimumThroughput = 20;

            /// <summary>
            /// The default sampling duration milliseconds.
            /// </summary>
            public const int SamplingDurationMilliseconds = 10000;
        }
    }
}
