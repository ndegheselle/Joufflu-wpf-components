using System.Reflection;

namespace Joufflu.Proto.Data.Builders
{
    public class BuilderToObject
    {
        public static TObject Convert<TObject>(GenericElement element, GenericObject? context = null)
        {
            return (TObject)Convert(element, typeof(TObject), context);
        }

        private static object Convert(GenericElement element, Type type, GenericObject? context = null)
        {
            if (context != null)
                element.ApplyContext(context);

            return element switch
            {
                GenericEnum genericEnum => ConvertEnum(genericEnum, type),
                GenericValue genericValue => ConvertValue(genericValue, type),
                GenericArray genericArray => ConvertArray(genericArray, type),
                GenericObject genericObject => ConvertObject(genericObject, type),
                _ => throw new ArgumentException($"Unsupported IGenericElement type: {element.GetType()}")
            };
        }

        private static object ConvertEnum(GenericEnum genericEnum, Type type)
        {
            return Enum.ToObject(type, genericEnum.Value);
        }

        private static object ConvertValue(GenericValue genericValue, Type type)
        {
            return genericValue.Value;
        }

        private static object ConvertArray(GenericArray genericArray, Type type)
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
                addMethod.Invoke(instance, [Convert(value, genericType)]);
            }

            return instance;
        }

        private static object ConvertObject(GenericObject genericObject, Type type)
        {
            object instance = Activator.CreateInstance(type) 
                ?? throw new Exception($"Cannot create type {type}.");

            foreach (var property in genericObject.Properties)
            {
                PropertyInfo propInfo = type.GetProperty(property.Key)
                    ?? throw new ArgumentException($"Property {property.Key} not found in type {type}.");

                object? value = Convert(property.Value, propInfo.PropertyType);
                propInfo.SetValue(instance, value);
            }

            return instance;
        }
    }
}