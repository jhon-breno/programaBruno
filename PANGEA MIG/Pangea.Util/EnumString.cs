using System;
using System.ComponentModel;
using System.Reflection;

namespace Pangea.Util
{
    public static class EnumString
    {
        public static string GetStringValue(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());

            DescriptionAttribute attribute
                    = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute))
                        as DescriptionAttribute;

            return attribute == null ? value.ToString() : attribute.Description;
        }
    }
}
