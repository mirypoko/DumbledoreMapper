using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DumbledoreMapper
{
    public static partial class Mapper
    {
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, Object>> CopyMappersDictionaries = new
            ConcurrentDictionary<Type, ConcurrentDictionary<Type, Object>>();

        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, PropertyInfo>> PropertiesDictionaries
            = new ConcurrentDictionary<Type, ConcurrentDictionary<string, PropertyInfo>>();

        /// <typeparam name="TSource">Source type.</typeparam>
        /// <typeparam name="TTarget">Target target.</typeparam>
        /// <param name="source">The source whose fields will be copy to the target object.</param>
        /// <param name="target">The object into which the fields will be copied.</param>
        public static void CopyProperties<TSource, TTarget>(TSource source, TTarget target)
        {
            GetOrCreateMapper<TSource, TTarget>().Invoke(source, target);
        }

        /// <typeparam name="TSource">Source type.</typeparam>
        /// <typeparam name="TTarget">Target target.</typeparam>
        /// <param name="source">The source whose fields will be copy to the target object.</param>
        /// <param name="target">The object into which the fields will be copied.</param>
        public static void CopyPropertiesIfNotNull(object source, object target)
        {
            var sourceType = source.GetType();
            var targetType = target.GetType();
            var sourceProperties = GetOrAddVisiblePropertiesToDictionary(sourceType);
            var targetPropertyes = GetOrAddVisiblePropertiesToDictionary(targetType);

            foreach (var targetProperty in targetPropertyes)
            {
                if (sourceProperties.TryGetValue(targetProperty.Key, out var sourceProperty))
                {
                    if (targetProperty.Value.PropertyType.IsClass || targetProperty.Value.PropertyType.IsGenericType && targetProperty.Value.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        var value = sourceProperty.GetValue(source);
                        if (value == null)
                        {
                            continue;
                        }
                    }
                    targetProperty.Value.SetValue(target, sourceProperty.GetValue(source));
                }
            }
        }

        /// <typeparam name="TSource">Source type.</typeparam>
        /// <typeparam name="TTarget">Target target.</typeparam>
        /// <param name="source">The source whose fields will be copy to the target object.</param>
        /// <param name="target">The object into which the fields will be copied.</param>
        /// <param name="ignoreTypeConflicts">Set true for the ability to copy different types (for example int? to int). 
        /// The parameter is unsafe to use.</param>
        public static void CopyPropertiesIfNotNull(object source, object target, bool ignoreTypeConflicts = false)
        {
            var sourceType = source.GetType();
            var targetType = target.GetType();
            var sourceProperties = GetOrAddVisiblePropertiesToDictionary(sourceType);
            var targetPropertyes = GetOrAddVisiblePropertiesToDictionary(targetType);

            foreach (var targetProperty in targetPropertyes)
            {
                if (sourceProperties.TryGetValue(targetProperty.Key, out var sourceProperty))
                {
                    if (!ignoreTypeConflicts && targetProperty.Value.PropertyType != sourceProperty.PropertyType)
                    {
                        Trace.TraceWarning($"Fields with the name {sourceProperty.Name} have different types and will not be copied.");
                        continue;
                    }
                    if (targetProperty.Value.PropertyType.IsClass || targetProperty.Value.PropertyType.IsGenericType && targetProperty.Value.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        var value = sourceProperty.GetValue(source);
                        if (value == null)
                        {
                            continue;
                        }
                    }
                    targetProperty.Value.SetValue(target, sourceProperty.GetValue(source));
                }
            }
        }

        private static Action<TSource, TTarget> GetOrCreateMapper<TSource, TTarget>()
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);

            if (CopyMappersDictionaries.TryGetValue(targetType, out var targetTypeMappers))
            {
                if (targetTypeMappers.TryGetValue(sourceType, out var mapper))
                {
                    return (Action<TSource, TTarget>)mapper;
                }

                mapper = CreateCopyMapper<TSource, TTarget>(sourceType, targetType);
                targetTypeMappers.GetOrAdd(sourceType, mapper);
                return (Action<TSource, TTarget>)mapper;
            }
            else
            {
                targetTypeMappers = new ConcurrentDictionary<Type, Object>();
                var mapper = CreateCopyMapper<TSource, TTarget>(sourceType, targetType);
                targetTypeMappers.GetOrAdd(sourceType, mapper);
                CopyMappersDictionaries.GetOrAdd(targetType, targetTypeMappers);
                return mapper;
            }
        }

        private static Action<TSource, TTarget> CreateCopyMapper<TSource, TTarget>(Type sourceType, Type targetType)
        {
            var sourceProperties = GetVisibleProperties(sourceType);
            var targetProperties = GetVisibleProperties(targetType);

            foreach (var targetProperty in targetProperties)
            {
                if (sourceProperties.TryGetValue(targetProperty.Key, out var sourceProperty))
                {
                    if (targetProperty.Value.PropertyType != sourceProperty.PropertyType)
                    {
                        Trace.TraceWarning($"Fields with the name {sourceProperty.Name} have different types and will not be copied.");
                        sourceProperties.Remove(targetProperty.Key);
                    }
                }
            }

            var fromVar = Expression.Variable(sourceType, "from");
            var toVar = Expression.Variable(targetType, "to");

            Expression CreateCopyProperty(string name)
            {
                try
                {
                    return Expression.Assign(
                        Expression.Property(toVar, targetProperties[name]),
                        Expression.Property(fromVar, sourceProperties[name]));
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Can not copy property {name}", ex);
                }
            }

            var commonProperties = sourceProperties.Keys.Intersect(targetProperties.Keys)
                                                        .OrderBy(n => n);

            var assignExpressions = commonProperties.Select(CreateCopyProperty);

            var assignment = Expression.Block(assignExpressions);

            var lambda = Expression.Lambda<Action<TSource, TTarget>>(assignment, fromVar, toVar);

            return lambda.Compile();
        }

        private static Dictionary<string, PropertyInfo> GetVisibleProperties(Type type)
        {
            IEnumerable<Type> GetTypeChain(Type t) =>
                t == null ? Enumerable.Empty<Type>() : GetTypeChain(t.BaseType).Prepend(t);

            Dictionary<string, PropertyInfo> result = new Dictionary<string, PropertyInfo>();
            foreach (var t in GetTypeChain(type))
            {
                var properties = t.GetProperties(BindingFlags.Instance |
                                                 BindingFlags.Public | BindingFlags.DeclaredOnly);
                foreach (var prop in properties)
                {
                    if (!result.ContainsKey(prop.Name))
                        result.Add(prop.Name, prop);
                }
            }
            return result;
        }

        private static ConcurrentDictionary<string, PropertyInfo> GetOrAddVisiblePropertiesToDictionary(Type type)
        {
            if (!PropertiesDictionaries.TryGetValue(type, out var propertiesInfoDictionary))
            {
                var props = GetVisibleProperties(type);
                propertiesInfoDictionary = new ConcurrentDictionary<string, PropertyInfo>(props);
                PropertiesDictionaries.GetOrAdd(type, propertiesInfoDictionary);
            }
            return propertiesInfoDictionary;
        }
    }
}
