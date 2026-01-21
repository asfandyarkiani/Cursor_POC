using System.Text;

namespace Core.Extensions
{
    public static class TupleExtension
    {
        public static Tuple<string, string> Simplify(this Tuple<string, AsyncLocal<StringBuilder>> header)
        {
            if (header == null)
                return new Tuple<string, string>(string.Empty, string.Empty);

            string key = string.IsNullOrWhiteSpace(header.Item1) ? string.Empty : header.Item1;

            string value = header.Item2?.Value == null || header.Item2.Value.Length == 0 ? string.Empty : header.Item2.Value.ToString();

            return new Tuple<string, string>(key, value);
        }

        public static bool IsContentHeader(this Tuple<string, string> header)
        {
            if (header == null)
                return false;

            string key = string.IsNullOrWhiteSpace(header.Item1) ? string.Empty : header.Item1;

            return key.ToLower().StartsWith("content-") ? true : false;
        }
    }
}
