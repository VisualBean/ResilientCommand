// <copyright file="InMemoryCache.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ResilientCommand.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace ResilientCommand
{
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// An In memory cache implementation.
    /// </summary>
    /// <seealso cref="ICache" />
    internal class InMemoryCache : ICache
    {
        /// <summary>
        /// The result cache.
        /// </summary>
        private static readonly ConcurrentDictionary<string, object> ResultCache = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <exception cref="ArgumentException">Thrown when key is null or empty.</exception>
        public void Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Key must not be null or empty.", nameof(key));
            }

            ResultCache.TryRemove(key, out _);
        }

        /// <summary>
        /// Tries the add a value to the cache.
        /// </summary>
        /// <typeparam name="T">The type to add.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// A boolean, representing success.
        /// </returns>
        /// <exception cref="ArgumentException">thrown when key is null or empty.</exception>
        /// <exception cref="ArgumentNullException">thrown when value is null.</exception>
        public bool TryAdd<T>(string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Key must not be null or empty.", nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return ResultCache.TryAdd(key, value);
        }

        /// <summary>
        /// Tries the get a value from the cache.
        /// </summary>
        /// <typeparam name="T">The type to store in the cache.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// A boolean representing success.
        /// </returns>
        /// <exception cref="ArgumentException">If key is null or empty.</exception>
        public bool TryGet<T>(string key, out T value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Key must not be null or empty.", nameof(key));
            }

            if (ResultCache.TryGetValue(key, out object innerValue))
            {
                value = (T)innerValue;
                return true;
            }

            value = default(T);
            return false;
        }
    }
}
