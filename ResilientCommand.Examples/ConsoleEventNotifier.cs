using System;

namespace ResilientCommand.Examples
{
    // Create a ResilientCommandEventNotifier implementation.
    public class ConsoleEventNotifier : ResilientCommandEventNotifier
    {
        public override void MarkEvent(ResillientCommandEventType eventType, CommandKey commandKey)
        {
            Console.WriteLine($"{commandKey}: {eventType}");
        }
    }
}
