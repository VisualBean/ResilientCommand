using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientCommand
{
    public abstract class ExecutionStrategy
    { 
        public abstract Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> innerAction, CancellationToken cancellationToken);
    }
}