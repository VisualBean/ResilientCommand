// <copyright file="DependencyInjectionExtensions.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand.Helpers
{
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;

    public static class DependencyInjectionExtensions
    {
        /// <summary>
        ///  Automagically adds all ResilientCommands from the given assemblies.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="assemblies">The assemblies.</param>
        public static void AddResilientCommands(this IServiceCollection services, params Assembly[] assemblies)
        {
             ServiceRegistrar.AddResilientCommand(services, assemblies);
        }
    }
}
