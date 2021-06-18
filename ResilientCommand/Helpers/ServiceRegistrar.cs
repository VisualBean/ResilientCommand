// <copyright file="ServiceRegistrar.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    internal static class ServiceRegistrar
    {
        /// <summary>
        /// Adds the resilient command.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="assembliesToScan">The assemblies to scan.</param>
        public static void AddResilientCommand(IServiceCollection services, IEnumerable<Assembly> assembliesToScan)
        {
            assembliesToScan = assembliesToScan.Distinct().ToArray();
            ConnectImplementationsToTypesClosing(typeof(IResilientCommand<>), services, assembliesToScan, true);

            var concretions = assembliesToScan
                    .SelectMany(a => a.DefinedTypes)
                    .Where(type => type.FindInterfacesThatClose(typeof(IResilientCommand<>)).Any())
                    .Where(type => type.IsConcrete() && type.IsOpenGeneric())
                    .ToList();

            foreach (var type in concretions)
            {
                services.AddTransient(typeof(IResilientCommand<>), type);
            }
        }

        /// <summary>
        /// Finds the interfaces that close.
        /// </summary>
        /// <param name="pluggedType">Type of the plugged.</param>
        /// <param name="templateType">Type of the template.</param>
        /// <returns>A list of <see cref="Type"/>.</returns>
        public static IEnumerable<Type> FindInterfacesThatClose(this Type pluggedType, Type templateType)
        {
            return FindInterfacesThatClosesCore(pluggedType, templateType).Distinct();
        }

        /// <summary>
        /// Determines whether [is open generic].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if [is open generic] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsOpenGeneric(this Type type)
        {
            return type.GetTypeInfo().IsGenericTypeDefinition || type.GetTypeInfo().ContainsGenericParameters;
        }

        /// <summary>
        /// Adds the concretions that could be closed.
        /// </summary>
        /// <param name="interface">The interface.</param>
        /// <param name="concretions">The concretions.</param>
        /// <param name="services">The services.</param>
        private static void AddConcretionsThatCouldBeClosed(Type @interface, List<Type> concretions, IServiceCollection services)
        {
            foreach (var type in concretions
                .Where(x => x.IsOpenGeneric() && x.CouldCloseTo(@interface)))
            {
                try
                {
                    services.TryAddTransient(@interface, type.MakeGenericType(@interface.GenericTypeArguments));
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// Determines whether this instance [can be cast to] the specified plugin type.
        /// </summary>
        /// <param name="pluggedType">Type of the plugged.</param>
        /// <param name="pluginType">Type of the plugin.</param>
        /// <returns>
        ///   <c>true</c> if this instance [can be cast to] the specified plugin type; otherwise, <c>false</c>.
        /// </returns>
        private static bool CanBeCastTo(this Type pluggedType, Type pluginType)
        {
            if (pluggedType == null)
            {
                return false;
            }

            if (pluggedType == pluginType)
            {
                return true;
            }

            return pluginType.GetTypeInfo().IsAssignableFrom(pluggedType.GetTypeInfo());
        }

        /// <summary>
        /// Helper method use to differentiate behavior between request handlers and notification handlers.
        /// Request handlers should only be added once (so set addIfAlreadyExists to false)
        /// Notification handlers should all be added (set addIfAlreadyExists to true).
        /// </summary>
        /// <param name="openRequestInterface">The open request interface.</param>
        /// <param name="services">The services.</param>
        /// <param name="assembliesToScan">The assemblies to scan.</param>
        /// <param name="addIfAlreadyExists">if set to <c>true</c> [add if already exists].</param>
        private static void ConnectImplementationsToTypesClosing(
            Type openRequestInterface,
            IServiceCollection services,
            IEnumerable<Assembly> assembliesToScan,
            bool addIfAlreadyExists)
        {
            var concretions = new List<Type>();
            var interfaces = new List<Type>();
            foreach (var type in assembliesToScan.SelectMany(a => a.DefinedTypes).Where(t => !t.IsOpenGeneric()))
            {
                var interfaceTypes = type.FindInterfacesThatClose(openRequestInterface).ToArray();
                if (!interfaceTypes.Any())
                {
                    continue;
                }

                if (type.IsConcrete())
                {
                    concretions.Add(type);
                }

                foreach (var interfaceType in interfaceTypes)
                {
                    interfaces.Fill(interfaceType);
                }
            }

            foreach (var @interface in interfaces)
            {
                var exactMatches = concretions.Where(x => x.CanBeCastTo(@interface)).ToList();
                if (addIfAlreadyExists)
                {
                    foreach (var type in exactMatches)
                    {
                        services.AddTransient(@interface, type);
                    }
                }
                else
                {
                    if (exactMatches.Count > 1)
                    {
                        exactMatches.RemoveAll(m => !IsMatchingWithInterface(m, @interface));
                    }

                    foreach (var type in exactMatches)
                    {
                        services.TryAddTransient(@interface, type);
                    }
                }

                if (!@interface.IsOpenGeneric())
                {
                    AddConcretionsThatCouldBeClosed(@interface, concretions, services);
                }
            }
        }

        /// <summary>
        /// Coulds the close to.
        /// </summary>
        /// <param name="openConcretion">The open concretion.</param>
        /// <param name="closedInterface">The closed interface.</param>
        /// <returns>
        /// A bool.
        /// </returns>
        private static bool CouldCloseTo(this Type openConcretion, Type closedInterface)
        {
            var openInterface = closedInterface.GetGenericTypeDefinition();
            var arguments = closedInterface.GenericTypeArguments;

            var concreteArguments = openConcretion.GenericTypeArguments;
            return arguments.Length == concreteArguments.Length && openConcretion.CanBeCastTo(openInterface);
        }

        /// <summary>
        /// Fills the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <param name="value">The value.</param>
        private static void Fill<T>(this IList<T> list, T value)
        {
            if (list.Contains(value))
            {
                return;
            }

            list.Add(value);
        }

        /// <summary>
        /// Finds the interfaces that closes core.
        /// </summary>
        /// <param name="pluggedType">Type of the plugged.</param>
        /// <param name="templateType">Type of the template.</param>
        /// <returns>A list of <see cref="Type"/>.</returns>
        private static IEnumerable<Type> FindInterfacesThatClosesCore(Type pluggedType, Type templateType)
        {
            if (pluggedType == null)
            {
                yield break;
            }

            if (!pluggedType.IsConcrete())
            {
                yield break;
            }

            if (templateType.GetTypeInfo().IsInterface)
            {
                foreach (
                    var interfaceType in
                    pluggedType.GetInterfaces()
                        .Where(type => type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == templateType))
                {
                    yield return interfaceType;
                }
            }
            else if (pluggedType.GetTypeInfo().BaseType.GetTypeInfo().IsGenericType &&
                     pluggedType.GetTypeInfo().BaseType.GetGenericTypeDefinition() == templateType)
            {
                yield return pluggedType.GetTypeInfo().BaseType;
            }

            if (pluggedType.GetTypeInfo().BaseType == typeof(object))
            {
                yield break;
            }

            foreach (var interfaceType in FindInterfacesThatClosesCore(pluggedType.GetTypeInfo().BaseType, templateType))
            {
                yield return interfaceType;
            }
        }

        /// <summary>
        /// Determines whether this instance is concrete.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is concrete; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsConcrete(this Type type)
        {
            return !type.GetTypeInfo().IsAbstract && !type.GetTypeInfo().IsInterface;
        }

        /// <summary>
        /// Determines whether [is matching with interface] [the specified handler type].
        /// </summary>
        /// <param name="handlerType">Type of the handler.</param>
        /// <param name="handlerInterface">The handler interface.</param>
        /// <returns>
        ///   <c>true</c> if [is matching with interface] [the specified handler type]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsMatchingWithInterface(Type handlerType, Type handlerInterface)
        {
            if (handlerType == null || handlerInterface == null)
            {
                return false;
            }

            if (handlerType.IsInterface)
            {
                if (handlerType.GenericTypeArguments.SequenceEqual(handlerInterface.GenericTypeArguments))
                {
                    return true;
                }
            }
            else
            {
                return IsMatchingWithInterface(handlerType.GetInterface(handlerInterface.Name), handlerInterface);
            }

            return false;
        }
    }
}