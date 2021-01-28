using System;
using System.Collections.Concurrent;

namespace ResilientCommand
{
    internal class CollapserFactory
    {
        private static readonly Lazy<CollapserFactory>
         instance =
         new Lazy<CollapserFactory>
             (() => new CollapserFactory());

        private ConcurrentDictionary<CommandKey, Lazy<Collapser>> collapserByCommand = new ConcurrentDictionary<CommandKey, Lazy<Collapser>>();

        internal static CollapserFactory GetInstance() => instance.Value;

        internal Collapser GetOrCreateCollapser(CommandKey commandKey, ResilientCommandEventNotifier eventNotifier, CollapserSettings collapserSettings)
        {
            return collapserByCommand.GetOrAdd(commandKey, new Lazy<Collapser>(() => new Collapser(commandKey, eventNotifier, collapserSettings))).Value;
        }
    }
}