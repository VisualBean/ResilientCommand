using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientCommand
{
    internal interface IExecutionStrategy
    {
        Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> innerAction, CancellationToken cancellationToken);
    }
}
