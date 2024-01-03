using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using System.Diagnostics;

namespace CoreUtils
{
    public delegate T ParseDelegate<T>(string value);
    public delegate bool TryParseDelegate<T>(string value, out T result);
    public static class ParseDelegateStore<T>
    {
        public static ParseDelegate<T> Parse;
        public static TryParseDelegate<T> TryParse;
    }

    public static class Data
    {
        public static int CalculateAge(DateTime dob, DateTime? ageAtDate = null)
        {            
            var now = ageAtDate ?? DateTime.Today;
            var age = now.Year - dob.Year;
            if (dob > now.AddYears(-age))
            {
                age--;
            }
            return age;
        }

        public static DateTime SQLMinDateTime { get { return new DateTime(1753, 1, 1); } }
        public static DateTimeOffset SQLMinDateTimeOffset { get { return new DateTimeOffset(1753, 1, 1, 0, 0, 0, new TimeSpan()); } }

        public static DateTime DateTimeToSQLMin(DateTime? value)
        {
            if (value == null || value < SQLMinDateTime)
            {
                return SQLMinDateTime;
            }
            return value.Value;
        }
        public static DateTimeOffset DateTimeOffsetToSQLMin(DateTimeOffset? value)
        {
            if (value == null || value < SQLMinDateTimeOffset)
            {
                return SQLMinDateTimeOffset;
            }
            return value.Value;
        }

        public static DateTime GetDateTimeInTimezone(string tzName, DateTimeOffset utcDate)
        {
            TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById(tzName);
            if (!tzInfo.IsInvalidTime(utcDate.DateTime))
            {
                DateTime destDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDate.DateTime, tzInfo);
                return destDateTime;
            }
            return utcDate.DateTime;
        }

        public static DateTime GetDateTimeInTimezone(string tzName, DateTime date)
        {
            TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById(tzName);
            if (!tzInfo.IsInvalidTime(date))
            {
                DateTime destDateTime = TimeZoneInfo.ConvertTimeFromUtc(date, tzInfo);
                return destDateTime;
            }
            return date;
        }

        public static string ReplaceAllInstances(string inval, string findval, string replaceval)
        {
            while (inval.Contains(findval))
            {
                inval = inval.Replace(findval, replaceval);
            }
            return inval;
        }

        /// <summary>
        /// Will cast an object, if the object is null or dbnull return the default(T)
        /// This function will not convert from one datatype to another
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T CastIt<T>(object value)
        {
            T outputIfNull = default(T);
            return CastIt<T>(value, outputIfNull);
        }
        public static T CastIt<T>(object value, T ouputIfNull)
        {
            if (value is T variable)
            {
                return variable;
            }
            if (value == null || value == DBNull.Value)
            {
                if (typeof(T) == typeof(string))
                {
                    object rv = string.Empty;
                    return ((T)rv);
                }
                else
                {
                    return ouputIfNull;
                }
            }
            return default(T);
        }

        public static bool IsNullableType(Type theType)
        {
            return (theType.IsGenericType && theType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));
        }

        public static string RemoveFirstOf(string inValue, string remove)
        {
            if (string.IsNullOrEmpty(inValue))
            {
                return "";
            }
            string rv = inValue;
            if (inValue.StartsWith(remove))
            {
                rv = inValue.Substring(remove.Length);
            }
            return rv;
        }
        public static string RemoveLastOf(string inValue, string remove)
        {
            if (string.IsNullOrEmpty(inValue))
            {
                return "";
            }
            string rv = inValue;
            if (inValue.EndsWith(remove))
            {
                rv = inValue.Substring(0, inValue.Length - remove.Length);
            }
            return rv;
        }

        /// <summary>
        /// parse from one datatype to another
        /// will return a default(T) if null or dbnull
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T ParseIt<T>(object value)
        {
            return ParseIt<T>(value, default(T));
        }

        public static T ParseIt<T>(object value, T outputifnull)
        {
            if (value is T variable)
            {
                return variable;
            }

            if (value == null || value == DBNull.Value)
            {
                if (typeof(T) == typeof(string))
                {
                    object rv = string.Empty;
                    return ((T)rv);
                }
                else
                {
                    return outputifnull;
                }
            }

            try
            {
                if (Nullable.GetUnderlyingType(typeof(T)) != null || typeof(T) == typeof(DateTimeOffset))
                {
                    return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFrom(value);
                }
                
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return outputifnull;
            }
        }

        public static T Parse<T>(string value)
        {
            if (value == null)
            {
                value = string.Empty;
            }

            ParseDelegate<T> parse = ParseDelegateStore<T>.Parse;
            if (parse == null)
            {
                parse = (ParseDelegate<T>)Delegate.CreateDelegate(typeof(ParseDelegate<T>), typeof(T), "Parse", true);
                ParseDelegateStore<T>.Parse = parse;
            }
            return parse(value);
        }

        public static bool TryParse<T>(string value, out T result)
        {
            if (value == null)
            {
                value = string.Empty;
            }

            TryParseDelegate<T> tryParse = ParseDelegateStore<T>.TryParse;
            if (tryParse == null)
            {
                tryParse = (TryParseDelegate<T>)Delegate.CreateDelegate(typeof(TryParseDelegate<T>), typeof(T), "TryParse", true);
                ParseDelegateStore<T>.TryParse = tryParse;
            }
            return tryParse(value, out result);
        }

        public static string ToString(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return "";
            }
            return value.ToString();
        }

        public static void CopyProperties(object source, object destination)
        {
            CopyProperties(source: source, destination: destination, types: null);
        }

        public static void CopyProperties(object source, object destination, List<Type> types)
        {
            PropertyInfo[] sourceproperties = source.GetType().GetProperties();
            PropertyInfo[] destinationproperties = destination.GetType().GetProperties();

            foreach (PropertyInfo sourcepi in sourceproperties)
            {
                PropertyInfo destinationpi = null;

                foreach (var pi in destinationproperties)
                {
                    if (pi.Name == sourcepi.Name && pi.GetType() == sourcepi.GetType())
                    {
                        destinationpi = pi;
                    }
                }

                if (destinationpi == null)
                {
                    continue;
                }

                #region

                Type ICloneType = sourcepi.PropertyType.GetInterface("ICloneable", true);

                if (ICloneType != null)
                {

                    ICloneable IClone = (ICloneable)sourcepi.GetValue(source, null);

                    if (IClone != null)
                    {
                        if (destinationpi.CanWrite && (types == null || types.Contains(destinationpi.GetType())))
                        {
                            destinationpi.SetValue(destination, IClone.Clone(), null);
                        }
                    }
                }
                else
                {
                    if (destinationpi.CanWrite && (types == null || types.Contains(destinationpi.GetType())))
                    {
                        destinationpi.SetValue(destination, sourcepi.GetValue(source, null), null);
                    }
                }

                #endregion
            }
        }
        
        public static void CopyProperties(object source, object destination, List<string> skipfields = null)
        {

            if (source == null || destination == null)
            {
                return;
            }

            PropertyInfo[] sourceproperties = source.GetType().GetProperties();
            PropertyInfo[] destinationproperties = destination.GetType().GetProperties();

            foreach (PropertyInfo sourcepi in sourceproperties)
            {
                if (skipfields == null || !skipfields.Contains(sourcepi.Name))
                {
                    try
                    {

                        PropertyInfo destinationpi = null;
                        //find destination pi
                        foreach (var pi in destinationproperties)
                        {
                            if (pi.Name == sourcepi.Name)
                            {
                                destinationpi = pi;
                            }
                        }

                        if (destinationpi == null)
                        {
                            continue;
                        }

                        #region

                        //We query if the fields support the ICloneable interface.
                        Type ICloneType = sourcepi.PropertyType.GetInterface("ICloneable", true);

                        if (ICloneType != null)
                        {
                            //Getting the ICloneable interface from the object.
                            ICloneable IClone = (ICloneable)sourcepi.GetValue(source, null);

                            //We use the clone method to set the new value to the field.
                            if (IClone != null)
                            {
                                if (destinationpi.CanWrite)
                                {
                                    destinationpi.SetValue(destination, IClone.Clone(), null);
                                }
                            }
                        }
                        else
                        {
                            if (destinationpi.CanWrite)
                            {
                                // If the field doesn't support the ICloneable
                                // interface then just set it.
                                if (destinationpi.PropertyType != sourcepi.PropertyType)
                                {
                                    if (destinationpi.PropertyType == typeof(DateTime))
                                    {
                                        object value = sourcepi.GetValue(source, null);
                                        destinationpi.SetValue(destination, value);
                                    }

                                }
                                else
                                {
                                    destinationpi.SetValue(destination, sourcepi.GetValue(source, null), null);
                                }
                            }
                        }
                        #endregion

                    }
                    catch (Exception exp)
                    {
                        Console.WriteLine(exp.ToString());

                    }
                }
            }
        }

        public static string Base64Encode(string plainText)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
        }
        public static string Base64Decode(string base64EncodedData)
        {         
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedData));
        }

        public static string FindReplaceEx(string source, string findRegEx, string replacewith)
        {
            return System.Text.RegularExpressions.Regex.Replace(source, findRegEx, replacewith);
        }
    }
}
