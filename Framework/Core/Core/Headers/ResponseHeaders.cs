
using System.Text;

namespace Core.Headers
{
    public static class ResponseHeaders
    {
        private const string SYS_TIME = "SYSTotalTime";
        private const string PROC_TIME = "PROCTotalTime";
        private const string DS_TIME_BREAKDOWN = "DSTimeBreakDown";
        private const string DS_AGGREGATED_TIME = "DSAggregatedTime";
        private const string CONTENT_TYPE = "Content-Type";
        private const string CONTENT_TYPE_JSON = "application/json";
        private const string IS_DOWNSTREAM_ERROR = "IsDownStreamError";
        private const string IS_DOWNLOAD_OR_PREVIEW = "ISDownloadOrPreview";

        private static readonly Tuple<string, AsyncLocal<StringBuilder>> _systotalTime = new Tuple<string, AsyncLocal<StringBuilder>>(SYS_TIME, new());
        private static readonly Tuple<string, AsyncLocal<StringBuilder>> _proctotalTime = new Tuple<string, AsyncLocal<StringBuilder>>(PROC_TIME, new());
        private static readonly Tuple<string, AsyncLocal<StringBuilder>> _dsTimeBreakDown = new Tuple<string, AsyncLocal<StringBuilder>>(DS_TIME_BREAKDOWN, new());
        private static readonly Tuple<string, AsyncLocal<StringBuilder>> _isDownStreamError = new Tuple<string, AsyncLocal<StringBuilder>>(IS_DOWNSTREAM_ERROR, new());
        private static readonly Tuple<string, AsyncLocal<StringBuilder>> _isDownloadOrPreview = new Tuple<string, AsyncLocal<StringBuilder>>(IS_DOWNLOAD_OR_PREVIEW, new());

        //for generic headers of a system layer request scope
        public static List<Tuple<string, string>> AllSysHeaders { get; set; } = new();

        //for generic headers of a process layer request scope
        public static List<Tuple<string, string>> AllProcHeaders { get; set; } = new();

        public static Tuple<string, AsyncLocal<StringBuilder>> SYSTotalTime
        {
            get { return _systotalTime; }
            set { _systotalTime.Item2.Value = value.Item2.Value!; }
        }

        public static Tuple<string, AsyncLocal<StringBuilder>> PROCTotalTime
        {
            get { return _proctotalTime; }
            set { _proctotalTime.Item2.Value = value.Item2.Value!; }
        }

        public static Tuple<string, AsyncLocal<StringBuilder>> DSTimeBreakDown
        {
            get { return _dsTimeBreakDown; }
            set { _dsTimeBreakDown.Item2.Value = value.Item2.Value!; }
        }

        public static Tuple<string, int> DSAggregatedTime
        {
            get
            {
                string? dsBreakDownTime = DSTimeBreakDown?.Item2?.Value?.ToString();
                int sum = string.IsNullOrWhiteSpace(dsBreakDownTime)
                    ? 0
                    : dsBreakDownTime
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(part => part.Split(':')[1])
                        .Sum(int.Parse);

                return new Tuple<string, int>(DS_AGGREGATED_TIME, sum);
            }
        }

        public static Tuple<string, string> ContentTypeJson
        {
            get
            {
                return new Tuple<string, string>(CONTENT_TYPE, CONTENT_TYPE_JSON);
            }
        }

        public static Tuple<string, AsyncLocal<StringBuilder>> IsDownStreamError
        {
            get { return _isDownStreamError; }
            set
            {
                _isDownStreamError.Item2.Value = value.Item2.Value!;
            }
        }

        public static Tuple<string, AsyncLocal<StringBuilder>> IsDownloadOrPreview
        {
            get { return _isDownloadOrPreview; }
            set
            {
                _isDownloadOrPreview.Item2.Value = value.Item2.Value!;
            }
        }
    }
}