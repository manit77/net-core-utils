using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace CoreUtils
{
    public static class Reflection
    {
        // Caches the "Plan" for copying Type A to Type B
        private static readonly ConcurrentDictionary<(Type, Type), List<PropertyMap>> _mapCache = new();

        // Cache the MethodInfo for ParseIt
        private static readonly MethodInfo _parseMethod =
            typeof(CoreUtils.Data).GetMethod(nameof(CoreUtils.Data.ParseIt), new[] { typeof(object) });

        public static T Copy<T>(object source, T destination, List<string> skipFields = null)
        {
            if (source == null || destination == null) return destination;

            var sourceType = source.GetType();
            var destType = typeof(T);
            var cacheKey = (sourceType, destType);

            // Get or Create the mapping plan
            var plan = _mapCache.GetOrAdd(cacheKey, key => BuildPlan(key.Item1, key.Item2));

            foreach (var map in plan)
            {
                if (skipFields?.Contains(map.SourceProperty.Name) == true) continue;

                try
                {
                    object val = map.SourceProperty.GetValue(source);

                    if (map.RequiresParse)
                    {
                        // Call the pre-constructed generic ParseIt<T>
                        val = map.GenericParseMethod.Invoke(null, new[] { val });
                    }

                    map.DestProperty.SetValue(destination, val);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error copying {map.SourceProperty.Name}: {ex.Message}");
                }
            }

            return destination;
        }

        private static List<PropertyMap> BuildPlan(Type source, Type dest)
        {
            var plan = new List<PropertyMap>();
            var destProps = dest.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .Where(p => p.CanWrite)
                                .ToDictionary(p => p.Name);

            foreach (var sProp in source.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (destProps.TryGetValue(sProp.Name, out var dProp))
                {
                    var map = new PropertyMap
                    {
                        SourceProperty = sProp,
                        DestProperty = dProp
                    };

                    // Check if types match exactly
                    if (sProp.PropertyType != dProp.PropertyType)
                    {
                        map.RequiresParse = true;
                        map.GenericParseMethod = _parseMethod.MakeGenericMethod(dProp.PropertyType);
                    }

                    plan.Add(map);
                }
            }
            return plan;
        }

        private class PropertyMap
        {
            public PropertyInfo SourceProperty { get; set; }
            public PropertyInfo DestProperty { get; set; }
            public bool RequiresParse { get; set; }
            public MethodInfo GenericParseMethod { get; set; }
        }
    }
}