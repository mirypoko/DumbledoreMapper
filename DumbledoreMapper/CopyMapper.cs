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
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, Object>> CopyMappersDictionaries = new
            ConcurrentDictionary<Type, ConcurrentDictionary<Type, Object>>();

        /// <typeparam name="TSource">Source type.</typeparam>
        /// <typeparam name="TTarget">Target target.</typeparam>
        /// <param name="source">The source whose fields will be copy to the target object.</param>
        /// <param name="target">The object into which the fields will be copied.</param>
        public static void CopyProperties<TSource, TTarget>(TSource source, TTarget target)
        {
            GetOrCreateMapper<TSource, TTarget>().Invoke(source, target);
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
    }
}
