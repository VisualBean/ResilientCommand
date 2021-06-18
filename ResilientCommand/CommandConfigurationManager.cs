// <copyright file="CommandConfiguration.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand
{
    using System;
    using System.Collections.Concurrent;
    using static ResilientCommand.CircuitBreakerSettings;
    using static ResilientCommand.CollapserSettings;
    using static ResilientCommand.ExecutionTimeoutSettings;
    using static ResilientCommand.FallbackSettings;
    using static ResilientCommand.SemaphoreSettings;

    public sealed class CommandConfigurationManager
    {
        private static readonly ConcurrentDictionary<CommandKey, Lazy<CommandConfiguration>> ConfigurationByKey = new ConcurrentDictionary<CommandKey, Lazy<CommandConfiguration>>();

        /// <summary>
        /// Gets or creates a circuit breaker.
        /// </summary>
        /// <param name="commandKey">The command key.</param>
        /// <param name="configuration">The  settings.</param>
        /// <returns>
        /// A <see cref="CommandConfiguration" />.
        /// </returns>
        internal static CommandConfiguration GetOrCreateCommandConfiguration(CommandKey commandKey, CommandConfiguration configuration)
        {
            return ConfigurationByKey.GetOrAdd(commandKey, new Lazy<CommandConfiguration>(() => configuration)).Value;
        }

        /// <summary>
        /// Gets or creates a circuit breaker.
        /// </summary>
        /// <param name="commandKey">The command key.</param>
        /// <returns>
        /// A <see cref="CommandConfiguration" />.
        /// </returns>
        public static CommandConfiguration GetCommandConfiguration(CommandKey commandKey)
        {
            if (ConfigurationByKey.TryGetValue(commandKey, out var configuration))
            {
                return configuration.Value;
            }

            return null;
        }
    }
}
