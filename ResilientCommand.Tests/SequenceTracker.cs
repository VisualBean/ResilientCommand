using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ResilientCommand.Tests
{
    public class SequenceTracker
    {
        int? state;

        public void Next(int newState)
        {
            if (newState <= state)
                Assert.Fail("Bad ordering there! States should be increasing.");

            state = newState;
        }
    }
}
