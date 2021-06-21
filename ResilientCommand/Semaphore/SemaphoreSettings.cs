// <copyright file="SemaphoreSettings.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand
{
    /// <summary>
    /// Semaphore settings.
    /// </summary>
    public sealed class SemaphoreSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SemaphoreSettings" /> class.
        /// </summary>
        /// <param name="isEnabled">if set to <c>true</c> [is enabled].</param>
        /// <param name="maxParalellism">The maximum paralellism.</param>
        /// <returns>The semaphore settings.</returns>
        public SemaphoreSettings(
            bool isEnabled = Default.IsEnabled,
            ushort maxParalellism = Default.MaxParallelism)
        {
            this.IsEnabled = isEnabled;
            this.MaxParallelism = maxParalellism;
        }

        /// <summary>
        /// Gets the default execution timeout settings.
        /// </summary>
        /// <value>
        /// The default execution timeout settings.
        /// </value>
        public static SemaphoreSettings DefaultSemaphoreSettings => new SemaphoreSettings();

        /// <summary>
        /// Gets the execution timeout in milliseconds.
        /// </summary>
        /// <value>
        /// The execution timeout in milliseconds.
        /// </value>
        public ushort MaxParallelism { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
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
            public const ushort MaxParallelism = 10;

            /// <summary>
            /// The default value indicating whether bulkheading has been enabled.
            /// </summary>
            public const bool IsEnabled = true;
        }
    }
}
