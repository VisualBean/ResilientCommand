// <copyright file="CollapserSettings.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand
{
    using System;

    /// <summary>
    /// Settings class for the Collapser.
    /// </summary>
    public sealed class CollapserSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CollapserSettings" /> class.
        /// </summary>
        /// <param name="isEnabled">if set to <c>true</c> [is enabled].</param>
        /// <param name="window">The window for collapsing requests.</param>
        public CollapserSettings(
            bool isEnabled = Default.IsEnabled,
            TimeSpan? window = null)
        {
            this.IsEnabled = isEnabled;
            this.Window = window ?? Default.Window;
        }

        /// <summary>
        /// Gets the default collapser settings.
        /// </summary>
        /// <value>
        /// The default collapser settings.
        /// </value>
        public static CollapserSettings DefaultCollapserSettings => new CollapserSettings();

        /// <summary>
        /// Gets or sets a value indicating whether collapsing is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if collapsing is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets the request window.
        /// </summary>
        /// <value>
        /// The window.
        /// </value>
        public TimeSpan Window { get; private set; }

        /// <summary>
        /// Default settings.
        /// </summary>
        public static class Default
        {
            /// <summary>
            /// The default is enabled.
            /// </summary>
            public const bool IsEnabled = false;

            /// <summary>
            /// The default request window.
            /// </summary>
            public static readonly TimeSpan Window;

            /// <summary>
            /// Initializes static members of the <see cref="Default"/> class.
            /// </summary>
            static Default()
            {
                Window = TimeSpan.FromMilliseconds(100);
            }
        }
    }
}