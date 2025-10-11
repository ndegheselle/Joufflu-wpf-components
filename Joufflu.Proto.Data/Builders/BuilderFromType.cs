using System.Collections;
using System.Reflection;

namespace Joufflu.Proto.Data.Builders
{
    /// <summary>
    /// Convert a type to a GenericElement
    /// </summary>
    public class BuilderFromType
    {
        public static GenericElement Convert(Type type, object? data = null)
        {
            if (type.IsEnum)
            {
                return ConvertEnum(type, data);
            }
            else if (type.IsValue(out EnumDataType dataType))
            {
                return new GenericValue(dataType, data);
            }

            // Get data from parameter less constructor if exist
            if (data == null)
            {
                var constructor = type.GetConstructor(Type.EmptyTypes);
                data = constructor?.Invoke(null);
            }

            if (type.IsEnumerable())
            {
                return ConvertArray(type, data);
            }

            return ConvertObject(type, data);
        }

        public static GenericElement Convert(object data)
        {
            return Convert(data.GetType(), data);
        }

        public static GenericEnum ConvertEnum(Type type, object? data)
        {
            return new GenericEnum(
                Enum.GetValues(type).Cast<Enum>().Select((x, i) => new GenericEnumValue(i, x.ToString())),
                data as int? ?? 0
                );
        }

        public static GenericArray ConvertArray(Type type, object? data)
        {
            var enumerableType = type.GetEnumerableType() ?? throw new Exception($"Can't get the generic type of '{type}'");
            return new GenericArray(
                    Convert(enumerableType, null),
                    (data as IEnumerable)?.Cast<object>().Select(val => Convert(val.GetType(), val)).ToList());
        }

        public static GenericObject ConvertObject(Type type, object? data)
        {
            GenericObject @object = new GenericObject([]);
            IEnumerable<PropertyInfo> typeProps = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(prop => prop.IsIgnorable() == false);

            foreach (var property in typeProps)
            {
                @object.AddProperty(property.Name, Convert(property.PropertyType, data == null ? null : property.GetValue(data, null)));
            }
            return @object;
        }

        public static GenericObject ConvertObject(object data)
        {
            return ConvertObject(data.GetType(), data);
        }
    }
}
