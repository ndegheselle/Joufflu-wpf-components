using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;

namespace Usuel.Shared.Schema
{
    public class GenericFactoryException : Exception
    {
        public GenericFactoryException(string message) : base(message)
        { }
    }
    
    public static class GenericExtensions
    {
        /// <summary>
        /// Convert type to a <see cref="EnumDataType"/> if the type is compatible.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsValue(this Type type, out EnumDataType datatype)
        {
            datatype = type switch
            {
                _ when type == typeof(string) => EnumDataType.String,
                _ when type == typeof(int) => EnumDataType.Decimal,
                _ when type == typeof(float) => EnumDataType.Decimal,
                _ when type == typeof(double) => EnumDataType.Decimal,
                _ when type == typeof(decimal) => EnumDataType.Decimal,
                _ when type == typeof(bool) => EnumDataType.Boolean,
                _ when type == typeof(DateTime) => EnumDataType.DateTime,
                _ when type == typeof(TimeSpan) => EnumDataType.TimeSpan,
                _ => EnumDataType.Object
            };
            return datatype != EnumDataType.Object;
        }

        /// <summary>
        /// If the type is an IEnumerable<> get the generic type, null otherwise.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsEnumerable(this Type type, out Type? enumerableType)
        {
            var enumerableInterface = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            enumerableType = enumerableInterface?.GetGenericArguments()[0];
            return enumerableType != null;
        }

        public static bool IsIgnorable(this PropertyInfo property)
        {
            IEnumerable<IgnoreDataMemberAttribute> ignoreAttribute = property.GetCustomAttributes(false).OfType<IgnoreDataMemberAttribute>();
            return ignoreAttribute.Any();
        }
    }

    public static class GenericFactory
    {

        public static IGenericElement Convert(Type type, object? data = null)
        {
            // Simple value
            if (type.IsValue(out EnumDataType dataType))
            {
                return new GenericValue(dataType, data);
            }
            else if (type.IsEnumerable(out Type? enumerableType) && enumerableType != null)
            {
                // XXX : generic array don't take 
                return new GenericArray(
                    Convert(enumerableType, null),
                    (data as IEnumerable)?.Cast<object>().Select(val => Convert(val.GetType(), val)).ToList());
            }

            return ConvertObject(type, data);
        }

        public static IGenericElement Convert(object data)
        {
            return Convert(data.GetType(), data);
        }

        public static GenericObject ConvertObject(Type type, object? data)
        {
            GenericObject @object = new GenericObject();
            IEnumerable<PropertyInfo> typeProps = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(prop => prop.IsIgnorable() == false);

            foreach (var property in typeProps)
            {
                @object.AddProperty(property.Name, Convert(property.PropertyType, property.GetValue(data, null)));
            }
            return @object;
        }

        public static GenericObject ConvertObject(object data)
        {
            return ConvertObject(data.GetType(), data);
        }

    }
}