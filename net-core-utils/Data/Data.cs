using System;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace CoreUtils;

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

    public static string ReplaceAll(string inval, string findval, string replaceval)
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

    /// <summary>
    /// parse from one datatype to another
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="outputIfNull"></param>
    /// <returns></returns>
    public static T ParseIt<T>(object value, T outputIfNull = default)
    {
        // 1. Already correct type
        if (value is T variable)
            return variable;

        // 2. Null / DBNull
        if (value == null || value == DBNull.Value)
        {
            if (typeof(T) == typeof(string))
                return (T)(object)string.Empty;

            return outputIfNull;
        }

        Type targetType = typeof(T);
        Type underlyingType = Nullable.GetUnderlyingType(targetType);
        Type conversionType = underlyingType ?? targetType;

        // ⭐ NEW: empty string handling
        if (value is string s && string.IsNullOrWhiteSpace(s))
        {
            // nullable → null
            if (underlyingType != null)
                return default;

            // string → empty
            if (targetType == typeof(string))
                return (T)(object)string.Empty;

            return outputIfNull;
        }

       
        try
        {
            if (conversionType == typeof(DateTimeOffset))
                return (T)TypeDescriptor.GetConverter(conversionType).ConvertFrom(value);

            if (conversionType.IsEnum)
                return (T)Enum.Parse(conversionType, value.ToString(), true);

            if (conversionType == typeof(Guid))
                return (T)(object)Guid.Parse(value.ToString());

            return (T)Convert.ChangeType(value, conversionType);
        }
        catch
        {
            try
            {
                string str = value.ToString();
                return (T)Convert.ChangeType(str, conversionType);
            }
            catch
            {
                Console.Error.WriteLine($"unable to convert data ${value} type ${conversionType}");
                return outputIfNull;
            }
        }
    }


    /// <summary>
    /// Uses reflection to call the static Parse method on type T
    /// Equivalent to:  DateTime.Parse(string)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static T Parse<T>(string value)
    {
        // Ensure value isn't null to avoid errors in target Parse methods
        value ??= string.Empty;

        if (ParseDelegateStore<T>.Parse == null)
        {
            // Get the method info for T.Parse(string)
            var method = typeof(T).GetMethod("Parse",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(string) },
                null);

            if (method == null)
                throw new InvalidOperationException($"Type {typeof(T).Name} does not have a static Parse(string) method.");

            // Create and cache the delegate
            ParseDelegateStore<T>.Parse = (ParseDelegate<T>)Delegate.CreateDelegate(typeof(ParseDelegate<T>), method);
        }

        return ParseDelegateStore<T>.Parse(value);
    }

    /// <summary>
    /// Converts an object to string, returning empty string for null or DBNull
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ToString(object value)
    {
        if (value == null || value == DBNull.Value)
        {
            return "";
        }
        return value.ToString();
    }

    public static bool IsEmptyWhiteSpace(string value)
    {
        return string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value);
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

    public static string ToPascalCase(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;

        // Handles snake_case and kebab-case by splitting and capitalizing.
        return string.Concat(str.Split(new[] { '_', '-' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(s => s.Length > 0 ? char.ToUpper(s[0]) + s.Substring(1).ToLower() : ""));
    }


    public static bool IsNumericType(object o)
    {
        Type t = o.GetType();
        Type underlying = Nullable.GetUnderlyingType(t) ?? t;
        return underlying == typeof(byte)
            || underlying == typeof(sbyte)
            || underlying == typeof(short)
            || underlying == typeof(ushort)
            || underlying == typeof(int)
            || underlying == typeof(uint)
            || underlying == typeof(long)
            || underlying == typeof(ulong)
            || underlying == typeof(float)
            || underlying == typeof(double)
            || underlying == typeof(decimal);
    }
}