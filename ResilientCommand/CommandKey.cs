// <copyright file="CommandKey.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand
{
    using System;

    /// <summary>
    /// A Key that represents a <see cref="ResilientCommand{TResult}"/>.
    /// </summary>
    public class CommandKey
    {
        private readonly string key;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandKey"/> class.
        /// </summary>
        /// <param name="key">The key to use.</param>
        public CommandKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or whitespace", nameof(key));
            }

            this.key = key;
        }

        public static implicit operator string(CommandKey key) => key.key;

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as CommandKey);
        }

        /// <summary>
        /// Equalses the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>A boolean representing equality.</returns>
        public bool Equals(CommandKey key)
        {
            return key != null && this.key == key.key;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return this.key.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.key.ToString();
        }
    }
}