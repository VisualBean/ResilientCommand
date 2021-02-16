// <copyright file="EventNotifierFactory.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand
{
    using System;

    /// <summary>
    /// A singleton notifier factory.
    /// </summary>
    public class EventNotifierFactory
    {
        private static readonly Lazy<EventNotifierFactory>
        Instance =
            new Lazy<EventNotifierFactory>(
                () => new EventNotifierFactory());

        private ResilientCommandEventNotifier notifier = new NullEventNotifier();

        /// <summary>
        /// Gets an instance of <see cref="EventNotifierFactory"/>.
        /// </summary>
        /// <returns>An instance of a <see cref="EventNotifierFactory"/>.</returns>
        public static EventNotifierFactory GetInstance() => Instance.Value;

        /// <summary>
        /// Gets the event notifier.
        /// </summary>
        /// <returns>A <see cref="ResilientCommandEventNotifier"/>.</returns>
        public ResilientCommandEventNotifier GetEventNotifier()
        {
            return this.notifier;
        }

        /// <summary>
        /// Sets the event notifier.
        /// </summary>
        /// <param name="eventNotifier">The event notifier.</param>
        public void SetEventNotifier(ResilientCommandEventNotifier eventNotifier)
        {
            this.notifier = eventNotifier;
        }
    }
}
