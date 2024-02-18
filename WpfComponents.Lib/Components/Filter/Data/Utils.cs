using System;
using System.Collections;

namespace UltraFiltre.Lib
{
    public static class Utils
    {
        public static bool EstEntier(Type pType)
        {
            switch (Type.GetTypeCode(pType))
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

        public static bool EstDecimal(Type pType)
        {
            switch (Type.GetTypeCode(pType))
            {
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static bool EstSimple(Type pType)
        {
            return EstEntier(pType) ||
                EstDecimal(pType) ||
                pType == typeof(string) ||
                pType == typeof(bool) ||
                pType == typeof(TimeSpan) ||
                pType == typeof(DateTime);
        }

        public static bool EstListe(Type pType)
        {
            return pType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(pType);
        }
    }
}
