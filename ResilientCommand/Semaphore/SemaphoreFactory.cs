using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ResilientCommand
{
    internal static class SemaphoreFactory
    {
        private static readonly ConcurrentDictionary<CommandKey, Lazy<SemaphoreSlim>> semaphoreByGroupKey = new ConcurrentDictionary<CommandKey, Lazy<SemaphoreSlim>>();

        internal static SemaphoreSlim GetOrCreateSemaphore(CommandKey groupKey, ushort concurrentThreads)
        {
            return semaphoreByGroupKey.GetOrAdd(groupKey, new Lazy<SemaphoreSlim>(() => new SemaphoreSlim(concurrentThreads, concurrentThreads))).Value;
        }
    }
}