﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientCommand.Examples
{
    class BasicCachingCommand : ResilientCommand<int>
    {
        static int count;
        protected override string GetCacheKey()
        {
            return "key";
        }

        protected override Task<int> RunAsync(CancellationToken cancellationToken)
        {
            count++;
            return Task.FromResult(count);
        }
    }

    class BasicCachingExample
    {
        static async Task CachingExample()
        {
            // Set notifier to use consolenotifier.
            EventNotifierFactory.GetInstance().SetEventNotifier(new ConsoleEventNotifier());

            // Create an instance of our command.
            var command = new BasicCachingCommand();

            // Run it a couple of times.
            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);
            var response = await command.ExecuteAsync(default);
            Console.WriteLine($"Response: {response}");

            /* Console Output:
            1: First run, get result, save to cache.
                BasicCachingCommand: Success
            2: Second run, get result from cache.
                BasicCachingCommand: ResponseFromCache
            3: Third run, get result from cache.
                BasicCachingCommand: ResponseFromCache
            
                Response: 1
            */
        }
    }
}
