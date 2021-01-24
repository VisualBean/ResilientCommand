using ResilientCommand;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace something.Examples
{
    // Create a ResilientCommandEventNotifier implementation.
    public class ConsoleEventNotifier : ResilientCommandEventNotifier
    {
        public override void markEvent(ResillientCommandEventType eventType, CommandKey commandKey)
        {
            Console.WriteLine($"{commandKey}: {eventType}");
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            // Set the new implementation as the notifier to be used.
            EventNotifierFactory.GetInstance().SetEventNotifier(new ConsoleEventNotifier());

            // Create an instance of our command.
            var command = new MySuperCommand();

            // Run it a couple of times.
            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);

            /* Output: 
            1: First call.
                MySuperCommand: ExceptionThrown
                MySuperCommand: FallbackSuccess
                
            2: Second call, trips the circuit.
                MySuperCommand: CircuitBroken
                MySuperCommand: ExceptionThrown
                MySuperCommand: FallbackSuccess

            3: Third call, goes directly to the fallback.
                MySuperCommand: FallbackSuccess
            */
        }
    }


    class MySuperCommand : ResilientCommand<string>
    {
        public MySuperCommand() : base(configuration: CommandConfiguration.CreateConfiguration(config =>
        {
            // Setup circuitbreaker to be broken after 2 tries and 10% failures.
            config.CircuitBreakerSettings = new CircuitBreakerSettings(failureThreshhold: 0.1, minimumThroughput: 2);
        }))
        {
        }
        protected override Task<string> RunAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        // Enabled fallback.
        protected override string Fallback()
        {
            return "A fallback response";
        }
    }
}
