
using System.Text;

namespace Core.Headers
{
    public static class RequestHeaders
    {
        private const string TEST_RUN_ID = "Test-Run-Id";
        private const string REQ_ID = "x-apim-correlation-id";

        private static readonly Tuple<string, AsyncLocal<StringBuilder>> _testRunId = new Tuple<string, AsyncLocal<StringBuilder>>(TEST_RUN_ID, new());
        private static readonly Tuple<string, AsyncLocal<StringBuilder>> _reqId = new Tuple<string, AsyncLocal<StringBuilder>>(REQ_ID, new());

        public static Tuple<string, AsyncLocal<StringBuilder>> TestRunId
        {
            get { return _testRunId; }
            set { _testRunId.Item2.Value = value.Item2.Value!; }
        }

        public static Tuple<string, AsyncLocal<StringBuilder>> RequestId
        {
            get { return _reqId; }
            set { _reqId.Item2.Value = value.Item2.Value!; }
        }
    }
}
