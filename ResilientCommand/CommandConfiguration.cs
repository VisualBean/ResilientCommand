using System;
using static ResilientCommand.CircuitBreakerSettings;
using static ResilientCommand.ExecutionTimeoutSettings;

namespace ResilientCommand
{
    public class CommandConfiguration
    {
        public CircuitBreakerSettings CircuitBreakerSettings { get; set; } = DefaultCircuitBreakerSettings;

        public ExecutionTimeoutSettings ExecutionTimeoutSettings { get; set; } = DefaultExecutionTimeoutSettings;

        public bool FallbackEnabled { get; set; } = true;

        public int MaxParallelism { get; set; } = 10;

        /// <summary>
        /// Creates the configuration with default values.
        /// </summary>
        /// <returns>A <see cref="CommandConfiguration"/></returns>
        public static CommandConfiguration CreateConfiguration()
        {
            return new CommandConfiguration();
        }

        /// <summary>
        /// Creates the configuration initially with default values.
        /// </summary>
        /// <param name="configurationFactory">The configuration factory.</param>
        /// <returns>A <see cref="CommandConfiguration"/></returns>
        public static CommandConfiguration CreateConfiguration(Action<CommandConfiguration> configurationFactory)
        {
            var commandConfiguration = new CommandConfiguration();
            configurationFactory(commandConfiguration);
            return commandConfiguration;
        }

        private CommandConfiguration()
        {
        }
    }
}
