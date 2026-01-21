using System.Text;

namespace Core.Context
{
    public static class Session
    {
        private static readonly AsyncLocal<StringBuilder?> _requestBody = new();

        public static StringBuilder RequestBody
        {
            get => _requestBody.Value ??= new StringBuilder();
            set => _requestBody.Value = value;
        }
    }
}
