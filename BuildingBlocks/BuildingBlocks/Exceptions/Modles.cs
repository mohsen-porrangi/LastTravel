namespace BuildingBlocks.Exceptions
{
    public class ApiError
    {
        public ApiError(string title, string detail, int statusCode)
        {
            Title = title;
            Detail = detail;
            StatusCode = statusCode;
        }

        public string Title { get; }
        public string Detail { get; }
        public int StatusCode { get; }
        public IDictionary<string, object> Extensions { get; } = new Dictionary<string, object>();
    }

    // مدل برای خطاهای اعتبارسنجی
    public class ValidationApiError : ApiError
    {
        public ValidationApiError(string title, string detail, int statusCode)
            : base(title, detail, statusCode)
        {
            Errors = new Dictionary<string, string[]>();
        }

        public IDictionary<string, string[]> Errors { get; }
    }
}
