//https://github.com/mirypoko/DumbledoreMapper

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DumbledoreMapper
{
    public static partial class Mapper
    {
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, Func<object, object>>> MappersDictionaries = new
            ConcurrentDictionary<Type, ConcurrentDictionary<Type, Func<object, object>>>();

        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, Func<object, object>>> MappersDictionariesWithIgnoreTypeConflicts = new
            ConcurrentDictionary<Type, ConcurrentDictionary<Type, Func<object, object>>>();

        /// <summary>
        /// Map source objects to the new objects T
        /// </summary>
        /// <param name="source">The source whose items will be map to the new collection</param>
        /// <param name="ignoreTypeConflicts">Set true for the ability to copy different types (for example int? to int). 
        /// The parameter is unsafe to use.</param>
        /// <returns>New collection of objects of type T</returns>
        public static List<T> Map<T>(IEnumerable<object> source, bool ignoreTypeConflicts = false) where T : class
        {
            var resultElemetnType = typeof(T);

            var sourceElemetnType = source.GetType().GetGenericArguments().Single();

            Func<object, object> itemsMapper;
            if (ignoreTypeConflicts)
            {
                itemsMapper = GetOrCreateMapperWithIgnoreTypeConflicts(sourceElemetnType, resultElemetnType);
            }
            else
            {
                itemsMapper = GetOrCreateMapper(sourceElemetnType, resultElemetnType);
            }

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
        /// <param name="ignoreTypeConflicts">Set true for the ability to copy different types (for example int? to int). 
        /// The parameter is unsafe to use.</param>
        /// <returns>A new object of type T</returns>
        public static T Map<T>(object source, bool ignoreTypeConflicts = false) where T : class
        {
            var destinationType = typeof(T);
            return (T)Map(source, destinationType, ignoreTypeConflicts);
        }

        /// <summary>
        /// Map source object to the new destinationType object
        /// </summary>
        /// <param name="source">The source whose fields will be copied to the new object.</param>
        /// <param name="destinationType">Target type.</param>
        /// <param name="ignoreTypeConflicts">Set true for the ability to copy different types (for example int? to int). 
        /// The parameter is unsafe to use.</param>
        /// <returns></returns>
        public static object Map(object source, Type destinationType, bool ignoreTypeConflicts = false)
        {
            var sourceType = source.GetType();
            if (ignoreTypeConflicts)
            {
                return GetOrCreateMapperWithIgnoreTypeConflicts(sourceType, destinationType).Invoke(source);
            }
            return GetOrCreateMapper(sourceType, destinationType).Invoke(source);
        }


        private static Func<object, object> GetOrCreateMapperWithIgnoreTypeConflicts(Type sourceType, Type destinationType)
        {
            if (MappersDictionariesWithIgnoreTypeConflicts.TryGetValue(destinationType, out var targetTypeMappers))
            {
                if (targetTypeMappers.TryGetValue(sourceType, out var mapper))
                {
                    return mapper;
                }

                mapper = CreateCopyMapperWithIgnoreTypeConflicts(sourceType, destinationType);
                targetTypeMappers.GetOrAdd(sourceType, mapper);
                return mapper;
            }
            else
            {
                targetTypeMappers = new ConcurrentDictionary<Type, Func<object, object>>();
                var mapper = CreateCopyMapperWithIgnoreTypeConflicts(sourceType, destinationType);
                targetTypeMappers.GetOrAdd(sourceType, mapper);
                MappersDictionariesWithIgnoreTypeConflicts.GetOrAdd(destinationType, targetTypeMappers);
                return mapper;
            }
        }

        private static Func<object, object> GetOrCreateMapper(Type sourceType, Type destinationType)
        {
            if (MappersDictionaries.TryGetValue(destinationType, out var targetTypeMappers))
            {
                if (targetTypeMappers.TryGetValue(sourceType, out var mapper))
                {
                    return mapper;
                }

                mapper = CreateCopyMapper(sourceType, destinationType);
                targetTypeMappers.GetOrAdd(sourceType, mapper);
                return mapper;
            }
            else
            {
                targetTypeMappers = new ConcurrentDictionary<Type, Func<object, object>>();
                var mapper = CreateCopyMapper(sourceType, destinationType);
                targetTypeMappers.GetOrAdd(sourceType, mapper);
                MappersDictionaries.GetOrAdd(destinationType, targetTypeMappers);
                return mapper;
            }
        }

        private static Func<object, object> CreateCopyMapper(Type sourceType, Type targetType)
        {
            var sourceProperties = GetVisibleProperties(sourceType);
            var targetProperties = GetVisibleProperties(targetType);

            var paramExpr = Expression.Parameter(typeof(object));
            var sourceExpr = Expression.Convert(paramExpr, sourceType);

            var bindings = new List<MemberBinding>();

            foreach (var targetProperty in targetProperties)
            {
                if (sourceProperties.TryGetValue(targetProperty.Key, out var sourceProperty))
                {
                    if (targetProperty.Value.PropertyType != sourceProperty.PropertyType)
                    {
                        Trace.TraceWarning($"Fields with the name {sourceProperty.Name} have different types and will not be copied.");
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

        private static Func<object, object> CreateCopyMapperWithIgnoreTypeConflicts(Type sourceType, Type targetType)
        {
            var sourceProperties = GetVisibleProperties(sourceType);
            var targetProperties = GetVisibleProperties(targetType);

            var paramExpr = Expression.Parameter(typeof(object));
            var sourceExpr = Expression.Convert(paramExpr, sourceType);

            var bindings = new List<MemberBinding>();

            foreach (var targetProperty in targetProperties)
            {
                if (sourceProperties.TryGetValue(targetProperty.Key, out var sourceProperty))
                {
                    bindings.Add(Expression.Bind(targetProperty.Value,
                        Expression.Property(sourceExpr, sourceProperty)));
                }
            }
            var resultExpr = Expression.MemberInit(Expression.New(targetType), bindings);
            var mapperExpr = Expression.Lambda<Func<object, object>>(resultExpr, paramExpr);
            return mapperExpr.Compile();
        }

    }
}
