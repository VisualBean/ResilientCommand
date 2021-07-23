// <copyright file="ServiceCollectionExtensions.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand.DependencyInjection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///  Automagically adds all ResilientCommands from the given assemblies.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="assemblies">The assemblies.</param>
        public static void AddResilientCommands(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            ServiceRegistrar.AddResilientCommand(services, assemblies);
        }

        /// <summary>
        ///  Automagically adds all ResilientCommands from the given assemblies.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="assemblies">The assemblies.</param>
        public static void AddResilientCommands(this IServiceCollection services, params Assembly[] assemblies)
        {
             AddResilientCommands(services, assemblies.ToList());
        }

        /// <summary>
        /// Automagically adds all ResilientCommands from the given assemblies.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="types">The types.</param>
        public static void AddResilientCommands(this IServiceCollection services, params Type[] types)
        {
            AddResilientCommands(services, types.Select(t => t.GetTypeInfo().Assembly));
        }
    }
}
