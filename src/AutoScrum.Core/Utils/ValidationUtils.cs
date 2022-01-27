using System;

namespace AutoScrum.Core.Utils;

public static class ValidationUtils
{
    public static void EnsurePropertyIsValid<T>(this T obj, Func<T, string?> property, string paramName)
    {
        if (string.IsNullOrWhiteSpace(property.Invoke(obj)))
        {
            throw new ArgumentNullException(paramName, $"{paramName} cannot be null or whitespace");
        }
    }
}