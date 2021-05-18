// <copyright file="ExecutionDecorator.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientCommand
{
    public interface IExecutionPolicy
    {
        Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> innerAction, CancellationToken cancellationToken);
    }
}