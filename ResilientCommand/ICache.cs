// <copyright file="ICache.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand
{
    /// <summary>
    /// A cache interface.
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        void Remove(string key);

        /// <summary>
        /// Tries the add a value to the cache.
        /// </summary>
        /// <typeparam name="T">The type to add.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>A boolean representing success.</returns>
        bool TryAdd<T>(string key, T value);

        /// <summary>
        /// Tries the get a value by key.
        /// </summary>
        /// <typeparam name="T">The type to add.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>A boolean representing success.</returns>
        bool TryGet<T>(string key, out T value);
    }
}
