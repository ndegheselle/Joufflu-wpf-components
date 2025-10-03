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
                _ when type == typeof(int) => EnumDataType.Integer,
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
        /// If the type is an IEnumerable<>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsEnumerable(this Type type)
        {
            return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }

        /// <summary>
        /// Get the generic type of an an IEnumerable<>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type? GetEnumerableType(this Type type)
        {
            var enumerableInterface = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            return enumerableInterface?.GetGenericArguments()[0];
        }

        public static bool IsIgnorable(this PropertyInfo property)
        {
            IEnumerable<IgnoreDataMemberAttribute> ignoreAttribute = property.GetCustomAttributes(false).OfType<IgnoreDataMemberAttribute>();
            return ignoreAttribute.Any();
        }
    }

    /// <summary>
    /// Convert a type to a GenericElement
    /// </summary>
    public static class GenericFactory
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
            return new GenericEnum(Enum.GetValues(type).Cast<Enum>().Select((x, i) => new GenericEnum.EnumValue(i, x.ToString())));
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

    public class ObjectFactory
    {
        public static TObject Convert<TObject>(GenericElement element, GenericObject? context)
        {
            return (TObject)Convert(element, typeof(TObject), context);
        }

        private static object Convert(GenericElement element, Type type,  GenericObject? context)
        {
            return element switch
            {
                GenericEnum genericEnum => ConvertEnum(genericEnum, type, context),
                GenericValue genericValue => ConvertValue(genericValue, type, context),
                GenericArray genericArray => ConvertArray(genericArray, type, context),
                GenericObject genericObject => ConvertObject(genericObject, type, context),
                _ => throw new ArgumentException($"Unsupported IGenericElement type: {element.GetType()}")
            };
        }

        private static object ConvertEnum(GenericEnum genericEnum, Type type, GenericObject? context)
        {
            object? contextValue = ConvertContext(genericEnum.ContextReference, type, context);
            return contextValue ?? Enum.ToObject(type, genericEnum.Value);
        }

        private static object ConvertValue(GenericValue genericValue, Type type, GenericObject? context)
        {
            object? contextValue = ConvertContext(genericValue.ContextReference, type, context);
            return contextValue ?? genericValue.Value;
        }

        private static object ConvertArray(GenericArray genericArray, Type type, GenericObject? context)
        {
            if (!type.IsEnumerable())
                throw new ArgumentException($"Target type {type} is not an enumerable type.");

            object instance = Activator.CreateInstance(type)
                ?? throw new Exception($"Cannot create type {type}.");


            MethodInfo addMethod = type.GetMethod("Add", [type])
                ?? throw new InvalidOperationException($"{type.Name} does not support adding items.");

            var genericType = type.GetGenericArguments()[0];
            foreach (var value in genericArray.Values)
            {
                addMethod.Invoke(instance, [Convert(value, genericType, context)]);
            }

            return instance;
        }

        private static object ConvertObject(GenericObject genericObject, Type type, GenericObject? context)
        {
            object instance = Activator.CreateInstance(type) 
                ?? throw new Exception($"Cannot create type {type}.");

            foreach (var property in genericObject.Properties)
            {
                PropertyInfo propInfo = type.GetProperty(property.Key)
                    ?? throw new ArgumentException($"Property {property.Key} not found in type {type}.");

                object? value = Convert(property.Value, propInfo.PropertyType, context);
                propInfo.SetValue(instance, value);
            }

            return instance;
        }

        // TODO : move to GenericElement
        [Obsolete]
        private static GenericElement? ResolveContext(string contextReference, GenericObject? context)
        {
            if (context == null)
                return null;
            if (string.IsNullOrEmpty(contextReference))
                return null;

            string[] referenceTree = contextReference.Split(".");

            for(int i = 0; i < referenceTree.Length - 1; i++)
            {
                string reference = referenceTree[i];
                if (!context.Properties.TryGetValue(reference, out var nextElement))
                    throw new KeyNotFoundException($"Could not find property '{reference}' in '{contextReference}'.");

                if (nextElement is not GenericObject objectElement)
                    throw new InvalidOperationException($"Property '{reference}' is not an object in '{contextReference}'.");

                context = objectElement;
            }

            return context.Properties[referenceTree[^1]];
        }

        private static object? ConvertContext(string contextReference, Type type, GenericObject? context)
        {
            GenericElement? element = ResolveContext(contextReference, context);
            if (element == null)
                return null;
            return Convert(element, type, context);
        }
    }
}