// <copyright file="IExecutionPolicy.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The interface for ExecutionPolicies.
    /// </summary>
    public interface IExecutionPolicy
    {
        /// <summary>
        /// Executes the asynchronous.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="innerAction">The inner action.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task.</returns>
        Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> innerAction, CancellationToken cancellationToken);
    }
}