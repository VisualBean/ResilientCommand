// <copyright file="CircuitBrokenException.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand
{
    using System;

    /// <summary>
    /// An exception representing that the circuitbreaker is open.
    /// </summary>
    /// <seealso cref="Exception" />
    [Serializable]
    public class CircuitBrokenException : Exception
    {
        private const string ExceptionMessage = "Circuit has been broken";

        /// <summary>
        /// Initializes a new instance of the <see cref="CircuitBrokenException"/> class.
        /// </summary>
        /// <param name="commandKey">The commandKey that caused the exception.</param>
        public CircuitBrokenException(CommandKey commandKey)
            : base($"{ExceptionMessage} for {commandKey}.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CircuitBrokenException"/> class.
        /// </summary>
        /// <param name="commandKey">The commandKey that caused the exception.</param>
        /// <param name="innerException">The exception that caused the circuit to break.</param>
        public CircuitBrokenException(CommandKey commandKey, Exception innerException)
            : base($"{ExceptionMessage} for {commandKey}.", innerException)
        {
        }
    }
}