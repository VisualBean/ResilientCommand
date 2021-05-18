// <copyright file="ExecutionDecorator.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// An abstract class representing an ExecutionStrategy.
    /// </summary>
    public abstract class ExecutionDecorator : IExecutionPolicy
    {
        /// <summary>
        /// Executes the asynchronous.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="innerAction">The inner action.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task.</returns>
        public abstract Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> innerAction, CancellationToken cancellationToken);

        /// <summary>
        /// Wraps the specified inner decorator in the outer decorator.
        /// </summary>
        /// <param name="inner">The inner.</param>
        /// <returns>A wrapper.</returns>
        /// <exception cref="ArgumentNullException">nameof(inner).</exception>
        internal Wrapper Wrap(IExecutionPolicy inner)
        {
            if (inner is null)
            {
                throw new ArgumentNullException(nameof(inner));
            }

            return new Wrapper(this, inner);
        }
    }
}