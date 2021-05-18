// <copyright file="ResillientCommandEventType.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand
{
    /// <summary>
    /// Event types.
    /// </summary>
    public enum ResilientCommandEventType
    {
        /// <summary>
        /// Default EventType
        /// </summary>
        None = 0,

        /// <summary>
        /// Command failed.
        /// </summary>
        /// <remarks>
        /// Raised when any exception is thrown as part of the execution.
        /// This does however not mean that the fallback, didn't work.
        /// </remarks>
        Failure,

        /// <summary>
        /// Command succeeded.
        /// </summary>
        Success,

        /// <summary>
        /// The command timed out, as part of its execution.
        /// </summary>
        /// <remarks>
        /// Raised when the execution timeout is hit.
        /// </remarks>
        TimedOut,

        /// <summary>
        /// The circuit broke.
        /// </summary>
        CircuitBroken,

        /// <summary>
        /// The circuit reset
        /// </summary>
        CircuitReset,

        /// <summary>
        /// The fallback success
        /// </summary>
        /// <remarks>
        /// Fallback was used, and succeeded (should always succeed).
        /// </remarks>
        FallbackSuccess,

        /// <summary>
        /// The fallback missing
        /// </summary>
        /// <remarks>
        /// If the fallback wasnt implemented, this is raised.
        /// </remarks>
        FallbackMissing,

        /// <summary>
        /// The fallback disabled
        /// </summary>
        /// <remarks>
        /// If the fallback was disabeld through configuration.
        /// </remarks>
        FallbackDisabled,

        /// <summary>
        /// The response from cache
        /// </summary>
        /// <remarks>
        /// The response was found in cache.
        /// </remarks>
        ResponseFromCache,

        /// <summary>
        /// The collapsed
        /// </summary>
        /// <remarks>
        /// If execution was collapsed.
        /// </remarks>
        Collapsed,
        SemaphoreRejected,
    }
}
