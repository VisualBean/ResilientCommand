using System;
using System.Threading.Tasks;

namespace ResilientCommand
{
    public class NullEventNotifier : ResilientCommandEventNotifier
    {
        public override void MarkEvent(ResillientCommandEventType eventType, CommandKey key)
        {}
    }
}