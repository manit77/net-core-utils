using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CoreUtils
{
    public static class Reflection
    {
        public static void Copy(object source, object destination)
        {
            Copy(source, destination, null);
        }

        /// <summary>
        /// Copies matching properties from source to destination.
        /// </summary>
        public static void Copy(object source, object destination, List<string> skipFields = null)
        {
            if (source == null || destination == null) return;

            var sourceProps = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var destProps = destination.GetType()
                                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .ToDictionary(p => p.Name);

            foreach (var sourcePi in sourceProps)
            {
                if (skipFields?.Contains(sourcePi.Name) == true) continue;
                if (!destProps.TryGetValue(sourcePi.Name, out var destPi)) continue;
                if (!destPi.CanWrite) continue;

                try
                {
                    if (!destPi.PropertyType.IsAssignableFrom(sourcePi.PropertyType)) continue;

                    object value = sourcePi.GetValue(source);
                    if (value == null)
                    {
                        destPi.SetValue(destination, null);
                        continue;
                    }

                    if (value is ICloneable cloneable)
                    {
                        destPi.SetValue(destination, cloneable.Clone());
                    }
                    else
                    {
                        destPi.SetValue(destination, value);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error copying property {sourcePi.Name}: {ex.Message}");
                }
            }
        }

        public static List<string> GetPropertyNames<T>(T obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            return typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => p.Name)
                .ToList();
        }
    }
}