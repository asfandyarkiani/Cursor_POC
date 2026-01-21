using System.Globalization;
using System.Text.Json;

namespace Core.Extensions
{
    public static class DictionaryExtension
    {
        public static string ToStringValue(this Dictionary<string, object> dict, string keyName)
        {
            if(dict.ContainsKey(keyName))
            {
                if (dict.TryGetValue(keyName, out var value) && value != null)
                {
                    return ((JsonElement)dict[keyName]).GetString() ?? string.Empty;
                }
            }
           
            return string.Empty;
        }

        public static string GetValueOrDefault(this Dictionary<string, string> dict, string keyName)
        {
            return dict.TryGetValue(keyName, out var value) ? value ?? string.Empty : string.Empty;
        }

        public static DateTime? ToDateTimeValue(this Dictionary<string, object> dict, string keyName)
        {
            if (!dict.ContainsKey(keyName) || dict[keyName] == null)
                return null;

            JsonElement value = (JsonElement)dict[keyName];
            string? valueStr = value.ValueKind == JsonValueKind.String? value.GetString() : value.GetRawText();
            if (string.IsNullOrEmpty(valueStr.Trim()))
            {
                return null;
            }
            else 
            {
                if (DateTime.TryParse(valueStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime result))
                {
                    return result;
                }
            }
            
            return null;
        }

        public static bool ToBoolValue(this Dictionary<string, object> dict, String keyName)
        {
            if (dict.ContainsKey(keyName))
            {
                JsonElement value = (JsonElement)dict[keyName];
                string strValue = value.GetRawText();
                return strValue.ToBool();
            }
            else
            {
                return false;
            }
        }

        public static int ToIntValue(this Dictionary<string, object> dict, String keyName)
        {
            if (dict.ContainsKey(keyName))
            {
                JsonElement value = (JsonElement)dict[keyName];
                return value.GetInt32();
            }
            else
            {
                return default(Int32);
            }
        }

        public static decimal ToDecimalValue(this Dictionary<string, object> dict, String keyName)
        {
            if (dict.ContainsKey(keyName))
            {
                JsonElement value = (JsonElement)dict[keyName];
                return value.GetDecimal();
            }
            else 
            {
                return default(decimal);
            }
        }

        public static long ToLongValue(this Dictionary<string, object> dict, string key)
        {
            if (dict.TryGetValue(key, out var value) && value != null && long.TryParse(value.ToString(), out long result))
                return result;

            return 0;
        }
    }
}
