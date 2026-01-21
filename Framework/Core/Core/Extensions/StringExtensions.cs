
using System.Text.RegularExpressions;

namespace Core.Extensions
{
    public static class StringExtensions
    {
        public static bool ToBool(this String value)
        {
            if (value == null) return false;

            var str = value.ToString().Trim().ToLowerInvariant();
            if (str == "true" || str == "yes" || str == "1")
                return true;
            if (str == "false" || str == "no" || str == "0")
                return false;

            if (int.TryParse(str, out int intVal))
                return intVal != 0;

            return false;
        }

        public static bool IsMMM_YYYY(this string monthAndYear)
        {
            var monthPattern = @"^(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)-\d{4}$";
            if (!Regex.IsMatch(monthAndYear, monthPattern, RegexOptions.IgnoreCase))
            {
                return false;
            }

            return true;
        }

        public static bool IsValidEmail(this string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            // Basic email regex pattern (covers most valid cases)
            string pattern = @"^(?=.{1,254}$)(?=.{1,64}@)[a-zA-Z0-9]+(?:[._-][a-zA-Z0-9]+)*@(?!-)[a-zA-Z0-9-]+(?<!-)(?:\.[a-zA-Z0-9-]+)*(?<!-)\.(?!-)[a-zA-Z]{2,}(?<!-)$";
            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }

        public static bool IsValidPhoneNumber(this string number)
        {
            if (string.IsNullOrWhiteSpace(number))
                return false;

            // E.164 format: + followed by 8–15 digits
            Regex regex = new Regex(@"^\+[1-9]\d{7,14}$");
            return regex.IsMatch(number);
        }

        public static bool IsAlphaNumeric(this string employeeNumber)
        {
            if (string.IsNullOrWhiteSpace(employeeNumber))
            {
                return false;
            }

            string pattern = "^[a-zA-Z0-9]+$";
            return Regex.IsMatch(employeeNumber, pattern, RegexOptions.IgnoreCase);
        }

        public static bool IsNumericValue(this string employeeNumber)
        {
            if (string.IsNullOrWhiteSpace(employeeNumber))
            {
                return false;
            }

            string pattern = "^[0-9]+$";
            return Regex.IsMatch(employeeNumber, pattern);
        }

        public static string RemoveAllWhitespace(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var result = new char[input.Length];
            int index = 0;

            foreach (char c in input)
            {
                if (!char.IsWhiteSpace(c))
                {
                    result[index++] = c;
                }
            }

            return new string(result, 0, index);
        }

        public static bool LooksLikeHtml(this string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return false;

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(content);

            // if it has at least one element node, it's HTML
            return doc.DocumentNode.Descendants().Any(n => n.NodeType == HtmlAgilityPack.HtmlNodeType.Element);
        }

        public static bool HasExtension(this string fileName)
        {
            string extension = Path.GetExtension(fileName);
            return !string.IsNullOrEmpty(extension) && extension.Length > 1;
        }

        public static bool HasValidExtension(this string fileName)
        {
            string[] _allowedExtensions = { ".pdf", ".jpeg", ".png", ".doc", ".docx", ".jpg" };

            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            return _allowedExtensions.Contains(extension);
        }

        public static bool IsContainsAlphabetsOrSpecialChars(this string value)
        {
            return value.Any(ch => !char.IsDigit(ch));
        }

        public static bool ValidateOrderBy(this string value, List<string> errors)
        {
            var parts = value.Split(':', StringSplitOptions.None);

            if (parts.Length > 2)
            {
                errors.Add("OrderBy format is invalid. Use 'FieldName' or 'FieldName:asc|desc'.");
                return false;
            }
            else
            {
                string field = parts[0]?.Trim();
                string? direction = parts.Length == 2 ? parts[1]?.Trim() : null;

                //  Field must not be empty
                if (string.IsNullOrWhiteSpace(field))
                {
                    errors.Add("OrderBy field name cannot be empty.");
                    return false;
                }

                //  Field must not equal asc/desc (catch 'asc', 'desc' alone)
                if (string.Equals(field, "asc", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(field, "desc", StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add("OrderBy field name cannot be 'asc' or 'desc'.");
                    return false;
                }

                // Direction must be only asc/desc if provided
                if (!string.IsNullOrWhiteSpace(direction) &&
                    !string.Equals(direction, "asc", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add("OrderBy direction must be 'asc' or 'desc'.");
                    return false;
                }
                return true;
            }
        }
    }
}
