using Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Extensions
{
    public static class EnumExtensions
    {
        public static string GetStringValue(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            if (field == null)
                return value.ToString();
            var attr = Attribute.GetCustomAttribute(field, typeof(StringValueAttribute)) as StringValueAttribute;
            return attr != null ? attr.StringValue : value.ToString();
        }
    }

}
