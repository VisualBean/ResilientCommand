namespace ResilientCommand
{
    public abstract class ResilientCommandEventNotifier
    {
        public abstract void MarkEvent(ResillientCommandEventType eventType, CommandKey commandKey);
    }
}
