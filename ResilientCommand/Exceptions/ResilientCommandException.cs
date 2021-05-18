// <copyright file="ResilientCommandException.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// An exception that will NOT be handled by the resilientCommand infrastructure.
    /// </summary>
    /// <seealso cref="Exception" />
    internal class ResilientCommandException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResilientCommandException"/> class.
        /// </summary>
        public ResilientCommandException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResilientCommandException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ResilientCommandException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResilientCommandException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public ResilientCommandException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResilientCommandException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"></see> that contains contextual information about the source or destination.</param>
        protected ResilientCommandException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
