using System;

namespace ProjectBase.Common.Utils
{
    public static class EnumUtils
    {
        public static T[] GetValues<T>() where T : Enum
        {
            return (T[])Enum.GetValues(typeof(T));
        }

        public static bool TryParse<T>(string value, out T result) where T : struct, Enum
        {
            return Enum.TryParse(value, true, out result);
        }

        public static T Parse<T>(string value) where T : struct, Enum
        {
            return Enum.Parse<T>(value, true);
        }

        public static int Count<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).Length;
        }
    }
}
