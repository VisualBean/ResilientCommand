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
        private static readonly ConcurrentDictionary<CommandKey, Lazy<SemaphoreSlim>> SemaphoreByGroupKey = new ConcurrentDictionary<CommandKey, Lazy<SemaphoreSlim>>();

        /// <summary>
        /// Gets the or create semaphore.
        /// </summary>
        /// <param name="commandKey">The command key.</param>
        /// <param name="concurrentThreads">The concurrent threads.</param>
        /// <returns>
        /// A <see cref="SemaphoreSlim"/>.
        /// </returns>
        internal static SemaphoreSlim GetOrCreateSemaphore(CommandKey commandKey, ushort concurrentThreads)
        {
            return SemaphoreByGroupKey.GetOrAdd(commandKey, new Lazy<SemaphoreSlim>(() => new SemaphoreSlim(concurrentThreads, concurrentThreads))).Value;
        }
    }
}