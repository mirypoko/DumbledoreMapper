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
    public static class Mapper
    {
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, Object>> CopyMappersDictionaries = new
            ConcurrentDictionary<Type, ConcurrentDictionary<Type, Object>>();

        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, PropertyInfo>> PropertiesDictionaries
            = new ConcurrentDictionary<Type, ConcurrentDictionary<string, PropertyInfo>>();

        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, Func<object, object>>> MappersDictionaries = new
            ConcurrentDictionary<Type, ConcurrentDictionary<Type, Func<object, object>>>();

        #region Copy

        /// <summary>
        /// Copy public properties from source to target.
        /// </summary>
        /// <typeparam name="TSource">Source type.</typeparam>
        /// <typeparam name="TTarget">Target target.</typeparam>
        /// <param name="source">The source whose fields will be copy to the target object.</param>
        /// <param name="target">The object into which the fields will be copied.</param>
        /// <param name="copyNullValues">Copy null values.</param>
        /// <param name="copyNullableProperties">Copy nullable values.</param>
        public static void CopyProperties<TSource, TTarget>(TSource source, TTarget target, bool copyNullValues = true, bool copyNullableProperties = false) where TSource : class where TTarget : class
        {
            if (!copyNullValues || copyNullableProperties)
            {
                CopyWithReflection(source, target, copyNullValues, copyNullableProperties);
            }
            else
            {
                GetOrCreateCopyMapper<TSource, TTarget>().Invoke(source, target);
            }
        }

        private static void CopyWithReflection<TSource, TTarget>(TSource source, TTarget target, bool copyNullValues, bool copyNullableValues)
            where TSource : class where TTarget : class
        {
            var sourceProperties = GetOrAddVisiblePropertiesToDictionary(source.GetType());
            var targetProperties = GetOrAddVisiblePropertiesToDictionary(target.GetType());
            foreach (var targetProperty in targetProperties)
            {
                if (sourceProperties.TryGetValue(targetProperty.Key, out var sourceProperty))
                {
                    var sourceValue = sourceProperty.GetValue(source);
                    if (sourceValue == null && !copyNullValues)
                    {
                        continue;
                    }
                    if (targetProperty.Value.PropertyType != sourceProperty.PropertyType)
                    {
                        if (IsNulableTypeProperty(sourceProperty) || IsNulableTypeProperty(targetProperty.Value) && copyNullableValues)
                        {
                            if (IsNullablePropertyOfType(sourceProperty, targetProperty.Value.PropertyType) ||
                                IsNullablePropertyOfType(targetProperty.Value, sourceProperty.PropertyType))
                            {
                                try
                                {
                                    targetProperty.Value.SetValue(target, sourceValue);
                                }
                                catch (Exception ex)
                                {
                                    Trace.TraceWarning($"Fields with the name {sourceProperty.Name} will not be copied. " + ex.Message);
                                }
                                continue;
                            }
                        }
                        Trace.TraceWarning($"Fields with the name {sourceProperty.Name} have different types and will not be copied.");
                    }
                    else
                    {
                        try
                        {
                            targetProperty.Value.SetValue(target, sourceValue);
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceWarning($"Fields with the name {sourceProperty.Name} will not be copied. " + ex.Message);
                        }
                    }
                }
            }
        }

        private static bool IsNulableTypeProperty(PropertyInfo property)
        {
            return property.PropertyType.IsGenericType &&
                   property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        private static bool IsNullablePropertyOfType(PropertyInfo property, Type type)
        {
            return property.PropertyType.GenericTypeArguments.Any() &&
                   property.PropertyType.GenericTypeArguments.Contains(type);
        }

        private static Action<TSource, TTarget> GetOrCreateCopyMapper<TSource, TTarget>()
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
            var sourceProperties = GetOrAddVisiblePropertiesToDictionary(sourceType);
            var targetProperties = GetOrAddVisiblePropertiesToDictionary(targetType);

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
                    Trace.TraceWarning(ex.Message);
                    return null;
                }
            }

            List<Expression> assignExpressions = new List<Expression>();
            foreach (var key in targetProperties.Keys)
            {
                var exp = CreateCopyProperty(key);
                if (exp != null)
                {
                    assignExpressions.Add(exp);
                }
            }

            var assignment = Expression.Block(assignExpressions);

            var lambda = Expression.Lambda<Action<TSource, TTarget>>(assignment, fromVar, toVar);

            return lambda.Compile();
        }

        #endregion

        /// <summary>
        /// Map source objects to the new objects T
        /// </summary>
        /// <param name="source">The source whose items will be map to the new collection</param>
        /// <returns>New collection of objects of type T</returns>
        public static List<T> Map<T>(IEnumerable<object> source) where T : class
        {
            var resultElemetnType = typeof(T);

            var sourceElemetnType = source.GetType().GetGenericArguments().Single();

            var itemsMapper = GetOrCreateMapper(sourceElemetnType, resultElemetnType);
            var resultList = new List<T>();

            foreach (var item in source)
            {
                resultList.Add((T)itemsMapper.Invoke(item));
            }

            return resultList;
        }

        /// <summary>
        /// Map source object to the new object T
        /// </summary>
        /// <param name="source">The source whose fields will be copied to the new object.</param>
        /// <returns>A new object of type T</returns>
        public static T Map<T>(object source) where T : class
        {
            var destinationType = typeof(T);
            var sourceType = source.GetType();
            return (T)GetOrCreateMapper(sourceType, destinationType).Invoke(source);
        }

        private static Func<object, object> GetOrCreateMapper(Type sourceType, Type destinationType)
        {
            if (MappersDictionaries.TryGetValue(destinationType, out var targetTypeMappers))
            {
                if (targetTypeMappers.TryGetValue(sourceType, out var mapper))
                {
                    return mapper;
                }

                mapper = CreateMapper(sourceType, destinationType);
                targetTypeMappers.GetOrAdd(sourceType, mapper);
                return mapper;
            }
            else
            {
                targetTypeMappers = new ConcurrentDictionary<Type, Func<object, object>>();
                var mapper = CreateMapper(sourceType, destinationType);
                targetTypeMappers.GetOrAdd(sourceType, mapper);
                MappersDictionaries.GetOrAdd(destinationType, targetTypeMappers);
                return mapper;
            }
        }

        private static Func<object, object> CreateMapper(Type sourceType, Type targetType)
        {
            var sourceProperties = GetOrAddVisiblePropertiesToDictionary(sourceType);
            var targetProperties = GetOrAddVisiblePropertiesToDictionary(targetType);

            var paramExpr = Expression.Parameter(typeof(object));
            var sourceExpr = Expression.Convert(paramExpr, sourceType);

            var bindings = new List<MemberBinding>();

            foreach (var targetProperty in targetProperties)
            {
                if (sourceProperties.TryGetValue(targetProperty.Key, out var sourceProperty))
                {
                    if (targetProperty.Value.PropertyType != sourceProperty.PropertyType)
                    {
                        if (sourceProperty.PropertyType.IsGenericType && sourceProperty.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ||
                            targetProperty.Value.PropertyType.IsGenericType && targetProperty.Value.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            if (!(sourceProperty.PropertyType.GenericTypeArguments.Any() &&
                                sourceProperty.PropertyType.GenericTypeArguments.Contains(targetProperty.Value.PropertyType)))
                            {
                                if (!(targetProperty.Value.PropertyType.GenericTypeArguments.Any() &&
                                      targetProperty.Value.PropertyType.GenericTypeArguments.Contains(sourceProperty.PropertyType)))
                                {
                                    bindings.Add(Expression.Bind(targetProperty.Value,
                                        Expression.Property(sourceExpr, sourceProperty)));
                                    continue;
                                }
                            }
                        }

                        Trace.TraceWarning(
                            $"Fields with the name {sourceProperty.Name} have different types and will not be copied.");
                        continue;
                    }

                    bindings.Add(Expression.Bind(targetProperty.Value,
                        Expression.Property(sourceExpr, sourceProperty)));
                }
            }
            var resultExpr = Expression.MemberInit(Expression.New(targetType), bindings);
            var mapperExpr = Expression.Lambda<Func<object, object>>(resultExpr, paramExpr);
            return mapperExpr.Compile();
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
