using System;
using System.Threading.Tasks;

namespace ResilientCommand
{
    public class NullEventNotifier : ResilientCommandEventNotifier
    {
        public override void markEvent(ResillientCommandEventType eventType, CommandKey key)
        {}
    }
}