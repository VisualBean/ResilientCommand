// <copyright file="ExecutionTimeoutSettings.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand
{
    /// <summary>
    /// Settings class for the Execution timeout.
    /// </summary>
    public sealed class ExecutionTimeoutSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionTimeoutSettings" /> class.
        /// </summary>
        /// <param name="isEnabled">if set to <c>true</c> [is enabled].</param>
        /// <param name="executionTimeoutInMilliseconds">The execution timeout in milliseconds.</param>
        public ExecutionTimeoutSettings(
            bool isEnabled = Default.IsEnabled,
            ushort executionTimeoutInMilliseconds = Default.ExecutionTimeoutInMilliseconds)
        {
            this.IsEnabled = isEnabled;
            this.ExecutionTimeoutInMilliseconds = executionTimeoutInMilliseconds;
        }

        /// <summary>
        /// Gets the default execution timeout settings.
        /// </summary>
        /// <value>
        /// The default execution timeout settings.
        /// </value>
        public static ExecutionTimeoutSettings DefaultExecutionTimeoutSettings => new ExecutionTimeoutSettings();

        /// <summary>
        /// Gets the execution timeout in milliseconds.
        /// </summary>
        /// <value>
        /// The execution timeout in milliseconds.
        /// </value>
        public ushort ExecutionTimeoutInMilliseconds { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether execution timeout is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this execution timeout is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Default settings.
        /// </summary>
        public sealed class Default
        {
            /// <summary>
            /// The default timeout window.
            /// </summary>
            public const ushort ExecutionTimeoutInMilliseconds = 1000;

            /// <summary>
            /// The default value indicating whether execution timeout has been enabled.
            /// </summary>
            public const bool IsEnabled = true;
        }
    }
}
