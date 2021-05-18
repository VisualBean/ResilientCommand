// <copyright file="Wrapper.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A decorator for wrapping other decorators.
    /// </summary>
    /// <seealso cref="ExecutionDecorator" />
    internal sealed class Wrapper : ExecutionDecorator
    {
        /// <summary>
        /// The inner decorator.
        /// </summary>
        private readonly IExecutionPolicy inner;

        /// <summary>
        /// The outer decotartor.
        /// </summary>
        private readonly IExecutionPolicy outer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Wrapper"/> class.
        /// </summary>
        /// <param name="outer">The outer.</param>
        /// <param name="inner">The inner.</param>
        public Wrapper(IExecutionPolicy outer, IExecutionPolicy inner)
        {
            this.outer = outer;
            this.inner = inner;
        }

        /// <summary>
        /// Executes the asynchronous.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="innerAction">The inner action.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task.
        /// </returns>
        public override async Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> innerAction, CancellationToken cancellationToken)
        {
            return await ExecuteAsync(innerAction, cancellationToken, this.outer, this.inner);
        }

        /// <summary>
        /// Executes the asynchronous.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="outer">The outer.</param>
        /// <param name="inner">The inner.</param>
        /// <returns>A TResult.</returns>
        internal static async Task<TResult> ExecuteAsync<TResult>(
          Func<CancellationToken, Task<TResult>> func,
          CancellationToken cancellationToken,
          IExecutionPolicy outer,
          IExecutionPolicy inner)
           => await outer.ExecuteAsync(
               async (ct) => await inner.ExecuteAsync(
                   func,
                   ct),
               cancellationToken);
    }
}
