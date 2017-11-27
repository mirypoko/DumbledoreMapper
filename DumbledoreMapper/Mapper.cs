//https://github.com/mirypoko/DumbledoreMapper

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DumbledoreMapper
{
    public static class Mapper
    {
        private static readonly Dictionary<Type, Func<IList>> _listsConstructors = new Dictionary<Type, Func<IList>>();

        private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> _propertiesDictionaries
            = new Dictionary<Type, Dictionary<string, PropertyInfo>>();

        private static readonly Dictionary<Type, Dictionary<Type, Func<object, object>>> _mappersDictionaries = new
            Dictionary<Type, Dictionary<Type, Func<object, object>>>();

        /// <param name="source">The source whose items will be map to the new collection</param>
        /// <returns>A new object of type T</returns>
        public static T Map<T>(IEnumerable<object> source) where T : IEnumerable<object>
        {
            var resultType = typeof(T);

            var listConstructor = GetOrCreateListConstructor(resultType);

            var resultList = listConstructor.Invoke();

            var resultElemetnType = resultType.GetGenericArguments().Single();

            var sourceElemetnType = source.GetType().GetGenericArguments().Single();

            var itemsMapper = GetOrCreateMapper(sourceElemetnType, resultElemetnType);

            foreach (var item in source)
            {
                resultList.Add(itemsMapper.Invoke(item));
            }

            return (T)resultList;
        }

        ///// <summary>
        ///// </summary>
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

        private static Func<IList> GetOrCreateListConstructor(Type listType)
        {

            if (!_listsConstructors.TryGetValue(listType, out var listCounstructor))
            {
                listCounstructor = Expression.Lambda<Func<IList>>(Expression.New(listType)).Compile();
                _listsConstructors.Add(listType, listCounstructor);
            }

            return listCounstructor;
        }

        private static Func<object, object> GetOrCreateMapper(Type sourceType, Type destinationType)
        {
            if (_mappersDictionaries.TryGetValue(destinationType, out var targetTypeMappers))
            {
                if (targetTypeMappers.TryGetValue(sourceType, out var mapper))
                {
                    return mapper;
                }

                mapper = CreateMapper(sourceType, destinationType);
                targetTypeMappers.Add(sourceType, mapper);
                return mapper;
            }
            else
            {
                targetTypeMappers = new Dictionary<Type, Func<object, object>>();
                var mapper = CreateMapper(sourceType, destinationType);
                targetTypeMappers.Add(sourceType, mapper);
                _mappersDictionaries.Add(destinationType, targetTypeMappers);
                return mapper;
            }
        }

        private static Func<object, object> CreateMapper(Type sourceType, Type destinationType)
        {
            var sourceProperties = GetProperties(sourceType);
            var targetPropertyes = GetProperties(destinationType);

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
            var resultExpr = Expression.MemberInit(Expression.New(destinationType), bindings);
            var mapperExpr = Expression.Lambda<Func<object, object>>(resultExpr, paramExpr);
            return mapperExpr.Compile();
        }

        private static Dictionary<string, PropertyInfo> GetProperties(Type objType)
        {
            if (!_propertiesDictionaries.TryGetValue(objType, out var propertiesInfoDictionary))
            {
                var infos = objType.GetProperties();
                propertiesInfoDictionary = new Dictionary<string, PropertyInfo>();
                foreach (var propertyInfo in infos)
                {
                    propertiesInfoDictionary.Add(propertyInfo.Name, propertyInfo);
                }
                _propertiesDictionaries.Add(objType, propertiesInfoDictionary);
            }
            return propertiesInfoDictionary;
        }
    }
}
