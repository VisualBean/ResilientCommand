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
        /// <param name="maxParalellism">The maximum paralellism.</param>
        /// <returns>The semaphore settings.</returns>
        public SemaphoreSettings(
            ushort maxParalellism = Default.MaxParallelism)
        {
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
        /// Default settings.
        /// </summary>
        public sealed class Default
        {
            /// <summary>
            /// The default timeout window.
            /// </summary>
            public const ushort MaxParallelism = 10;
        }
    }
}
