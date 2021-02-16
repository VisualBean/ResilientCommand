// <copyright file="CommandConfiguration.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand
{
    using System;
    using static ResilientCommand.CircuitBreakerSettings;
    using static ResilientCommand.CollapserSettings;
    using static ResilientCommand.ExecutionTimeoutSettings;
    using static ResilientCommand.FallbackSettings;

    /// <summary>
    /// Command Configuration.
    /// </summary>
    public class CommandConfiguration
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="CommandConfiguration"/> class from being created.
        /// </summary>
        private CommandConfiguration()
        {
        }

        /// <summary>
        /// Gets or sets the circuit breaker settings.
        /// </summary>
        /// <value>
        /// The circuit breaker settings.
        /// </value>
        public CircuitBreakerSettings CircuitBreakerSettings { get; set; } = DefaultCircuitBreakerSettings;

        /// <summary>
        /// Gets or sets the collapser settings.
        /// </summary>
        /// <value>
        /// The collapser settings.
        /// </value>
        public CollapserSettings CollapserSettings { get; set; } = DefaultCollapserSettings;

        /// <summary>
        /// Gets or sets the execution timeout settings.
        /// </summary>
        /// <value>
        /// The execution timeout settings.
        /// </value>
        public ExecutionTimeoutSettings ExecutionTimeoutSettings { get; set; } = DefaultExecutionTimeoutSettings;

        /// <summary>
        /// Gets or sets the fallback settings.
        /// </summary>
        /// <value>
        /// The fallback settings.
        /// </value>
        public FallbackSettings FallbackSettings { get; set; } = DefaultFallbackSettings;

        /// <summary>
        /// Gets or sets the maximum parallelism.
        /// </summary>
        /// <value>
        /// The maximum parallelism.
        /// </value>
        public ushort MaxParallelism { get; set; } = 10;

        /// <summary>
        /// Creates the configuration with default values.
        /// </summary>
        /// <returns>
        /// A <see cref="CommandConfiguration" />.
        /// </returns>
        public static CommandConfiguration CreateConfiguration()
        {
            return new CommandConfiguration();
        }

        /// <summary>
        /// Creates the configuration initially with default values.
        /// </summary>
        /// <param name="configurationFactory">The configuration factory.</param>
        /// <returns>
        /// A <see cref="CommandConfiguration" />.
        /// </returns>
        public static CommandConfiguration CreateConfiguration(Action<CommandConfiguration> configurationFactory)
        {
            var commandConfiguration = new CommandConfiguration();
            configurationFactory(commandConfiguration);
            ValidateConfiguration(commandConfiguration);

            return commandConfiguration;
        }

        /// <summary>
        /// Validates the configuration.
        /// </summary>
        /// <param name="commandConfiguration">The command configuration.</param>
        /// <exception cref="ArgumentNullException">
        /// CircuitBreakerSettings
        /// or
        /// CollapserSettings
        /// or
        /// ExecutionTimeoutSettings
        /// or
        /// FallbackSettings.
        /// </exception>
        private static void ValidateConfiguration(CommandConfiguration commandConfiguration)
        {
            if (commandConfiguration.CircuitBreakerSettings == null)
            {
                throw new ArgumentNullException(nameof(commandConfiguration.CircuitBreakerSettings));
            }

            if (commandConfiguration.CollapserSettings == null)
            {
                throw new ArgumentNullException(nameof(commandConfiguration.CollapserSettings));
            }

            if (commandConfiguration.ExecutionTimeoutSettings == null)
            {
                throw new ArgumentNullException(nameof(commandConfiguration.ExecutionTimeoutSettings));
            }

            if (commandConfiguration.FallbackSettings == null)
            {
                throw new ArgumentNullException(nameof(commandConfiguration.FallbackSettings));
            }
        }
    }
}
