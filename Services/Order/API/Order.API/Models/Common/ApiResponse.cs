namespace Order.API.Models.Common;

public record ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public string[]? Errors { get; init; }

    public static ApiResponse<T> SuccessResponse(T data, string? message = null)
        => new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> FailureResponse(string message, params string[] errors)
        => new() { Success = false, Message = message, Errors = errors };
}