// <copyright file="IResilientCommand.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The main interface for resilient commands.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public interface IResilientCommand<TResult>
    {
        /// <summary>
        /// Gets the command key.
        /// </summary>
        /// <value>
        /// The command key.
        /// </value>
        CommandKey CommandKey { get; }

        /// <summary>
        /// Executes the asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task.</returns>
        Task<TResult> ExecuteAsync(CancellationToken cancellationToken);
    }
}