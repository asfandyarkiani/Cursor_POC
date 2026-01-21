namespace Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static string FormatMonthYear(this DateTime date)
        {
            return date.ToString("MMM-yyyy");
        }

        public static (string startDate, string endDate) GetDefaultDateRange(this DateTime referenceDate)
        {
            // Start date: First day of the month 3 months ago
            var threeMonthsAgo = referenceDate.AddMonths(-3);
            var startDate = new DateTime(threeMonthsAgo.Year, threeMonthsAgo.Month, 1);

            // End date: Last day of previous month
            var prevMonth = referenceDate.AddMonths(-1);
            var endDate = new DateTime(prevMonth.Year, prevMonth.Month, DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month));

            return (startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));
        }

        public static DateTime? ValidateAndParseDate(this string dateString, string fieldName, List<string> errors)
        {
            if (!DateTime.TryParseExact(dateString, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var parsedValue))
            {
                errors.Add($"Invalid {fieldName} format. It must be yyyy-MM-dd");
                return null;
            }

            return parsedValue;
        }

        public static void ValidateMinimumDate(this DateTime? date, string fieldName, DateTime minimumDate, List<string> errors)
        {
            if (date.HasValue && date < minimumDate)
            {
                errors.Add($"{fieldName} cannot be earlier than {minimumDate:MMM d, yyyy}.");
            }
        }

        public static void ValidateDateRange(this DateTime? startDate, DateTime? endDate, string startFieldName, string endFieldName, List<string> errors)
        {
            if (startDate.HasValue && endDate.HasValue && endDate < startDate)
            {
                errors.Add($"{endFieldName} cannot be earlier than {startFieldName}.");
            }
        }
    }
}
