//https://github.com/mirypoko/DumbledoreMapper

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DumbledoreMapper
{
    public static partial class Mapper
    {
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, Func<object, object>>> MappersDictionaries = new
            ConcurrentDictionary<Type, ConcurrentDictionary<Type, Func<object, object>>>();

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

        /// <param name="source">The source whose fields will be copied to the new object</param>
        /// <returns>A new object of type T</returns>
        public static T Map<T>(object source) where T : class
        {
            var destinationType = typeof(T);
            return (T)Map(source, destinationType);
        }

        public static object Map(object source, Type destinationType)
        {
            var sourceType = source.GetType();
            var mapper = GetOrCreateMapper(sourceType, destinationType);
            return mapper.Invoke(source);
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
            var targetPropertyes = GetVisibleProperties(targetType);

            var paramExpr = Expression.Parameter(typeof(object));
            var sourceExpr = Expression.Convert(paramExpr, sourceType);

            var bindings = new List<MemberBinding>();

            foreach (var targetProperty in targetPropertyes)
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
