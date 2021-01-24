using ResilientCommand;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace something.Examples
{

    public class ConsoleEventNotifier : ResilientCommandEventNotifier
    {
        public override void markEvent(ResillientCommandEventType eventType, CommandKey commandKey)
        {
            Console.WriteLine($"{commandKey}: {eventType}");
        }
    }

    class MySuperCommand : ResilientCommand<string>
    {
        public MySuperCommand() : base(configuration: CommandConfiguration.CreateConfiguration(config => 
        {
            config.CircuitBreakerSettings = new CircuitBreakerSettings(failureThreshhold: 0.1, minimumThroughput: 2);
        }))
        {

        }
        protected override Task<string> RunAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override string Fallback()
        {
            return "Oh noooooo!";
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            // setup
            EventNotifierFactory.GetInstance().SetEventNotifier(new ConsoleEventNotifier());

            var command = new MySuperCommand();
            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);
        }
    }
}
