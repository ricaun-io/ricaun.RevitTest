﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace ricaun.RevitTest.TestAdapter.Metadatas
{
    internal static class MapperKey
    {
        public static T Map<T>(T destination, IDictionary<string, string> dictionary)
        {
            if (dictionary is null)
                return destination;

            foreach (var kvp in Mapper.ProcessProperties(destination))
            {
                var sourceKey = kvp.Key;
                var sourcePropertyInfo = kvp.PropertyInfo;
                var sourceValue = sourcePropertyInfo.GetValue(kvp.Object);

                Debug.WriteLine($"{sourceKey} - {sourcePropertyInfo} - {sourceValue}");

                if (dictionary.TryGetValue(sourceKey, out string valueToConvert))
                {
                    var destinationValue = Mapper.ConvertValueToPropertyInfo(valueToConvert, sourcePropertyInfo);
                    if (!Mapper.IsValueEqual(sourceValue, destinationValue))
                    {
                        try
                        {
                            sourcePropertyInfo.SetValue(kvp.Object, destinationValue);
                            Debug.WriteLine($"\t{sourceKey}: {sourceValue} >> {sourcePropertyInfo.Name}: {destinationValue}");
                        }
                        catch { }
                    }
                }
            }

            return destination;
        }
        public static IEnumerable<string> GetNames<T>(T destination)
        {
            return Mapper.ProcessProperties(destination).Select(e => e.Key).ToList();
        }
        public static class Mapper
        {
            public static string[] CustomAttributeNames = new[] { "ElementName", "DisplayName", "Name" };
            public static Dictionary<Type, Func<string, object>> MapperConvert;
            static Mapper()
            {
                MapperConvert = new Dictionary<Type, Func<string, object>>();
                MapperConvert.Add(typeof(int), (string valueToConvert) =>
                {
                    if (int.TryParse(valueToConvert, out int value))
                        return value;
                    return valueToConvert;
                });
                MapperConvert.Add(typeof(double), (string valueToConvert) =>
                {
                    try
                    {
                        return double.Parse(valueToConvert.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch { }
                    return valueToConvert;
                });
                MapperConvert.Add(typeof(bool), (string valueToConvert) =>
                {
                    var value = valueToConvert.ToLower();
                    return value.Equals("true") || value.Equals("1");
                });
            }

            #region PropertyInformation
            public class PropertyInformation
            {
                public string Key { get; }
                public object Object { get; }
                public PropertyInfo PropertyInfo { get; }

                public PropertyInformation(string key, object obj, PropertyInfo propertyInfo)
                {
                    Key = key;
                    Object = obj;
                    PropertyInfo = propertyInfo;
                }
            }
            public static IEnumerable<PropertyInformation> ProcessProperties(object obj, string prefix = null)
            {
                if (obj == null)
                    yield break;

                if (string.IsNullOrEmpty(prefix))
                    prefix = string.Empty;

                Type objectType = obj.GetType();
                PropertyInfo[] properties = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (PropertyInfo property in properties)
                {
                    var propertyName = GetCustomAttributeName(property);
                    string propertyKey = string.IsNullOrEmpty(prefix) ? propertyName : $"{prefix}.{propertyName}";

                    if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                    {
                        object nestedPropertyValue = property.GetValue(obj);
                        if (nestedPropertyValue is null)
                        {
                            try
                            {
                                nestedPropertyValue = Activator.CreateInstance(property.PropertyType);
                                property.SetValue(obj, nestedPropertyValue);
                            }
                            catch { }
                        }
                        foreach (var nestedProperty in ProcessProperties(nestedPropertyValue, propertyKey))
                        {
                            yield return nestedProperty;
                        }
                    }
                    else
                    {
                        yield return new PropertyInformation(propertyKey, obj, property);
                    }
                }
            }
            #endregion
            public static string GetCustomAttributeName(MemberInfo memberInfo)
            {
                try
                {
                    var customPropertyNames = CustomAttributeNames;
                    foreach (var customAttribute in memberInfo.GetCustomAttributes())
                    {
                        var type = customAttribute.GetType();
                        foreach (var propertyName in customPropertyNames)
                        {
                            if (type?.GetProperty(propertyName) is PropertyInfo propertyInfo)
                            {
                                var value = propertyInfo.GetValue(customAttribute) as string;
                                if (!string.IsNullOrEmpty(value)) return value;
                            }
                        }
                    }
                }
                catch { }
                return memberInfo.Name;
            }
            public static object ConvertValueToPropertyInfo(string valueToConvert, PropertyInfo propertyInfo)
            {
                var type = propertyInfo.PropertyType;
                if (MapperConvert.TryGetValue(type, out var converter))
                {
                    return converter.Invoke(valueToConvert);
                }
                return valueToConvert;
            }
            public static bool IsValueEqual(object sourceValue, object destinationValue)
            {
                if (sourceValue is null)
                {
                    return destinationValue is null;
                }

                return sourceValue.Equals(destinationValue);
            }
        }
    }
}