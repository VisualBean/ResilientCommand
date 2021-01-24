using System;

namespace ResilientCommand
{
    public class EventNotifierFactory
    {
        ResilientCommandEventNotifier notifier = new NullEventNotifier();

        private static readonly Lazy<EventNotifierFactory>
        instance =
            new Lazy<EventNotifierFactory>
                (() => new EventNotifierFactory());
        public static EventNotifierFactory GetInstance() => instance.Value;

        public ResilientCommandEventNotifier GetEventNotifier()
        {
            return notifier;
        }

        public void SetEventNotifier(ResilientCommandEventNotifier eventNotifier)
        {
            this.notifier = eventNotifier;
        }
    }
}
