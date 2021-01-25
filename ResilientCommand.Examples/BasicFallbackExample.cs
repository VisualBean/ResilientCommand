using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientCommand.Examples
{

    class BasicFallbackExample
    {
        static async Task FallbackExample(string[] args)
        {
            // Set notifier to use consolenotifier.
            EventNotifierFactory.GetInstance().SetEventNotifier(new ConsoleEventNotifier());

            // Create an instance of our command.
            var command = new MyBasicCircuitBreakerFallCommand();

            // Run it.
            var response = await command.ExecuteAsync(default);

            Console.WriteLine($"Response: {response}");

            /* Console Output: 
            1: First call.
                BasicFallbackCommand: Failure
                BasicFallbackCommand: FallbackSuccess
                
                Response: A fallback response
            */
        }
    }


    class BasicFallbackCommand : ResilientCommand<string>
    {
        protected override Task<string> RunAsync(CancellationToken cancellationToken)
        {
            throw new Exception();
        }

        // Enabled fallback.
        protected override string Fallback()
        {
            return "A fallback response";
        }
    }
}
