using System;
using System.Collections.Generic;
using System.Reflection;

namespace CoreUtils
{
    public static class Reflection
    {
        public static List<string> GetPropertyNames<T>(T obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            List<string> propertyNames = new List<string>();

            // Get all public instance properties
            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                propertyNames.Add(prop.Name);
            }

            return propertyNames;
        }
    }
}