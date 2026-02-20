namespace UserAccessManagement.Application.Common
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; }
        public T Value { get; }
        public string Message { get; }
        public List<string> Errors { get; }

        private ApiResponse(bool success, T value, string message, List<string> errors)
        {
            IsSuccess = success;
            Value = value;
            Message = message;
            Errors = errors;
        }

        public static ApiResponse<T> Success(T value, string message = "") => new(true, value, message, new());
        public static ApiResponse<T> Failure(string message, List<string> errors = null) => new(false, default!, message, errors ?? new());
    }
}
