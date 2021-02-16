// <copyright file="ResilientCommandEventNotifier.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand
{
    /// <summary>
    /// An abstract Command Event Notifier.
    /// </summary>
    public abstract class ResilientCommandEventNotifier
    {
        /// <summary>
        /// Raises an event.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="commandKey">The command key.</param>
        public abstract void RaiseEvent(ResilientCommandEventType eventType, CommandKey commandKey);
    }
}
