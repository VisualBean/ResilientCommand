// <copyright file="CollapserFactory.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand
{
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// A Collapser factory.
    /// </summary>
    internal class CollapserFactory
    {
        /// <summary>
        /// The singeton instance.
        /// </summary>
        private static readonly Lazy<CollapserFactory>
         Instance =
         new Lazy<CollapserFactory>(
             () => new CollapserFactory());

        /// <summary>
        /// The collapser by command.
        /// </summary>
        private readonly ConcurrentDictionary<CommandKey, Lazy<Collapser>> collapserByCommand = new ConcurrentDictionary<CommandKey, Lazy<Collapser>>();

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <returns>A <see cref="CollapserFactory"/>.</returns>
        internal static CollapserFactory GetInstance() => Instance.Value;

        /// <summary>
        /// Gets the or create collapser.
        /// </summary>
        /// <param name="commandKey">The command key.</param>
        /// <param name="eventNotifier">The event notifier.</param>
        /// <param name="collapserSettings">The collapser settings.</param>
        /// <returns>A <see cref="Collapser"/>.</returns>
        internal Collapser GetOrCreateCollapser(CommandKey commandKey, ResilientCommandEventNotifier eventNotifier, CollapserSettings collapserSettings)
        {
            return this.collapserByCommand.GetOrAdd(commandKey, new Lazy<Collapser>(() => new Collapser(commandKey, eventNotifier, collapserSettings))).Value;
        }
    }
}