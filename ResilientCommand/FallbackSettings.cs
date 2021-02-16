// <copyright file="FallbackSettings.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand
{
    /// <summary>
    /// A fallback settings class.
    /// </summary>
    public sealed class FallbackSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FallbackSettings"/> class.
        /// </summary>
        /// <param name="isEnabled">if set to <c>true</c> [is enabled].</param>
        public FallbackSettings(
            bool isEnabled = Default.IsEnabled)
        {
            this.IsEnabled = isEnabled;
        }

        /// <summary>
        /// Gets the default fallback settings.
        /// </summary>
        /// <value>
        /// The default fallback settings.
        /// </value>
        public static FallbackSettings DefaultFallbackSettings => new FallbackSettings();

        /// <summary>
        /// Gets or sets a value indicating whether fallback is enabled.
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
            /// Fallback is enabled by default.
            /// </summary>
            public const bool IsEnabled = true;
        }
    }
}