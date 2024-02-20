using System;
using System.Collections;

namespace  WpfComponents.Lib.Components.Filters.Data
{
    public static class Utils
    {
        public static bool IsNumeric(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsDecimal(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsSimple(Type type)
        {
            return IsNumeric(type) ||
                IsDecimal(type) ||
                type == typeof(string) ||
                type == typeof(bool) ||
                type == typeof(TimeSpan) ||
                type == typeof(DateTime);
        }

        public static bool IsList(Type type)
        {
            return type.IsGenericType && typeof(IEnumerable).IsAssignableFrom(type);
        }
    }
}
