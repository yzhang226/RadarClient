using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Radar.Common.Utils
{
    public class EnumHelper
    {

        public static string ToText<T>(T t)
        {
            return Enum.GetName(typeof(T), t);
        }


        public static int ToValue<T>(T t)
        {
            return t.GetHashCode();
        }


        public static T ToEnum<T>(string name)
        {
            return (T) Enum.Parse(typeof(T), name);
        }


        public static int NameToValue<T>(string name)
        {
            return (int) Enum.Parse(typeof(T), name);
        }

        public static T ToEnum<T>(int value)
        {
            return (T) Enum.ToObject(typeof(T), value);
        }

        public static string Value2Name<T>(int value)
        {
            return Enum.GetName(typeof(T), value);
        }


        public static List<object> GetItems(Type enumType)
        {
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("Type must be an enum type.");
            }

            int[] items = (int[])Enum.GetValues(enumType);
            List<object> output = new List<object>();
            foreach (int val in items)
            {
                output.Add(val);
            }

            return output;
        }

    }


    public class EnumWrap
    {

        public string Name { get; set; }

        public int Value { get; set; }

        public int Sort { get; set; }

    }


}
