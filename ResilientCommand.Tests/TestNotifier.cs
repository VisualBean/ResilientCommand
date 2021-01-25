using System.Collections.Generic;

namespace ResilientCommand.Tests
{
    public class TestNotifier : ResilientCommandEventNotifier
    {
        public Dictionary<CommandKey, List<ResillientCommandEventType>> events = new Dictionary<CommandKey, List<ResillientCommandEventType>>();

        public override void MarkEvent(ResillientCommandEventType eventType, CommandKey commandKey)
        {
            if (!events.ContainsKey(commandKey))
            {
                events[commandKey] = new List<ResillientCommandEventType> { eventType };
            }
            else
            {
                events[commandKey].Add(eventType);
            }
        }
    }
}

