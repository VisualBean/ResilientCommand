using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientCommand.Examples
{

    class BasicCircuitBreakerWithFallbackExample
    {
        static async Task FallbackExample(string[] args)
        {
            // Set notifier to use consolenotifier.
            EventNotifierFactory.GetInstance().SetEventNotifier(new ConsoleEventNotifier());

            // Create an instance of our command.
            var command = new MyBasicCircuitBreakerFallCommand();

            // Run it a couple of times.
            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);
            var response = await command.ExecuteAsync(default);

            Console.WriteLine($"Response: {response}");

            /* Console Output: 
            1: First call.
                MyBasicFallCommand: Failure
                MyBasicFallCommand: FallbackSuccess
                
            2: Second call, trips the circuit.
                MyBasicFallCommand: CircuitBroken
                MyBasicFallCommand: Failure
                MyBasicFallCommand: FallbackSuccess

            3: Third call, skips the circuit logic due to it being open and goes directly to the fallback.
                MyBasicFallCommand: Failure
                MyBasicFallCommand: FallbackSuccess
                
                Response: A fallback response
            */
        }
    }


    class MyBasicCircuitBreakerFallCommand : ResilientCommand<string>
    {
        public MyBasicCircuitBreakerFallCommand() : base(configuration: CommandConfiguration.CreateConfiguration(config =>
        {
            // Setup circuitbreaker to be broken after 2 tries and 10% failures.
            config.CircuitBreakerSettings = new CircuitBreakerSettings(failureThreshhold: 0.1, minimumThroughput: 2);
        }))
        {
        }
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
