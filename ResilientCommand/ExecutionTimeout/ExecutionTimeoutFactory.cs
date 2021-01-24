using System;
using System.Collections.Concurrent;

namespace ResilientCommand
{
    internal class ExecutionTimeoutFactory
    {
        private static readonly Lazy<ExecutionTimeoutFactory>
           instance =
           new Lazy<ExecutionTimeoutFactory>
               (() => new ExecutionTimeoutFactory());

        private ConcurrentDictionary<CommandKey, Lazy<ExecutionTimeout>> executionTimeouts = new ConcurrentDictionary<CommandKey, Lazy<ExecutionTimeout>>();

        internal static ExecutionTimeoutFactory GetInstance() => instance.Value;

        internal ExecutionTimeout GetOrCreateExecutionTimeout(CommandKey commandKey, ResilientCommandEventNotifier eventNotifier, ExecutionTimeoutSettings executionTimeoutSettings)
        {
           return executionTimeouts.GetOrAdd(commandKey, new Lazy<ExecutionTimeout>(()=> new ExecutionTimeout(commandKey, eventNotifier, executionTimeoutSettings))).Value;
        }
    }
}