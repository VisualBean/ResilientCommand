using System.Collections.Generic;

namespace ResilientCommand.Tests
{
    public class TestNotifier : ResilientCommandEventNotifier
    {
        public Dictionary<CommandKey, List<ResilientCommandEventType>> events = new Dictionary<CommandKey, List<ResilientCommandEventType>>();

        public override void RaiseEvent(ResilientCommandEventType eventType, CommandKey commandKey)
        {
            if (!events.ContainsKey(commandKey))
            {
                events[commandKey] = new List<ResilientCommandEventType> { eventType };
            }
            else
            {
                events[commandKey].Add(eventType);
            }
        }
    }
}

