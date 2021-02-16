// <copyright file="NullEventNotifier.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand
{
    /// <summary>
    /// A null event notifier.
    /// </summary>
    /// <remarks>
    /// This does not notify.
    /// </remarks>
    /// <seealso cref="ResilientCommandEventNotifier" />
    public class NullEventNotifier : ResilientCommandEventNotifier
    {
        /// <inheritdoc/>
        public override void RaiseEvent(ResilientCommandEventType eventType, CommandKey key)
        {
        }
    }
}