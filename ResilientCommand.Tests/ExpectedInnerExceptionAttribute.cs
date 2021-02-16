using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ResilientCommand.Tests
{
    public class ExpectedInnerExceptionAttribute : ExpectedExceptionBaseAttribute
    {
        private readonly Type expectedInnerExceptionType;

        public ExpectedInnerExceptionAttribute(Type expectedInnerExceptionType)
        {
            this.expectedInnerExceptionType = expectedInnerExceptionType;
        }

        protected override void Verify(Exception exception)
        {
            var innerType = exception.InnerException?.GetType();
            if (innerType != null && innerType == expectedInnerExceptionType)
            {
                return;
            }

            throw new Exception($"Expected inner exception type: {expectedInnerExceptionType}, got: {innerType}");
        }
    }
}
