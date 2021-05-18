// <copyright file="SemaphoreFactory.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    /// <summary>
    /// A semaphore factory.
    /// </summary>
    internal static class SemaphoreFactory
    {
        /// <summary>
        /// The semaphore by group key.
        /// </summary>
        private static readonly ConcurrentDictionary<CommandKey, Lazy<Semaphore>> SemaphoreByGroupKey = new ConcurrentDictionary<CommandKey, Lazy<Semaphore>>();

        /// <summary>
        /// Gets the or create semaphore.
        /// </summary>
        /// <param name="commandKey">The command key.</param>
        /// <param name="eventNotifier">The event notifier.</param>
        /// <param name="semaphoreSettings">The semaphore settings.</param>
        /// <returns>
        /// A <see cref="SemaphoreSlim" />.
        /// </returns>
        internal static Semaphore GetOrCreateSemaphore(CommandKey commandKey, ResilientCommandEventNotifier eventNotifier, SemaphoreSettings semaphoreSettings)
        {
            return SemaphoreByGroupKey.GetOrAdd(commandKey, new Lazy<Semaphore>(() => new Semaphore(commandKey, eventNotifier, semaphoreSettings))).Value;
        }
    }
}