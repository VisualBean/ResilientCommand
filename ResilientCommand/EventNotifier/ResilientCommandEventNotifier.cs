using System;
using System.Threading.Tasks;

namespace ResilientCommand
{
    public abstract class ResilientCommandEventNotifier
    {
        public abstract void markEvent(ResillientCommandEventType fallbackCalled, CommandKey commandKey);
    }
}
