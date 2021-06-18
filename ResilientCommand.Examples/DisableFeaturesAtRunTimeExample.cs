using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientCommand.Examples
{
    class BasicCommand : ResilientCommand<bool>
    {
        protected override bool Fallback()
        {
            return false;
        }
        protected override Task<bool> RunAsync(CancellationToken cancellationToken)
        {
            throw new Exception("Totally throws");
        }
    }

    class DisableFeaturesAtRunTimeExample
    {
        static async Task DisableFeaturesAtRunTimeExample()
        {
            // Set notifier to use consolenotifier.
            EventNotifierFactory.GetInstance().SetEventNotifier(new ConsoleEventNotifier());

            // Create an instance of our command.
            var command = new BasicCommand();

            // Run it a couple of times.
            await command.ExecuteAsync(default); // Should fallback

            CommandConfigurationManager.GetCommandConfiguration(command.CommandKey).FallbackSettings.IsEnabled = false;

            await command.ExecuteAsync(default); // Will throw

            /* Console Output: 
            1: First call.
                BasicCommand: Failure
                BasicCommand: FallbackSuccess
                Response: false

            2: Second call.
                BasicCommand: Failure
                BasicCommand: FallbackDisabled
               Throws exception.
            */
        }
    }
}
