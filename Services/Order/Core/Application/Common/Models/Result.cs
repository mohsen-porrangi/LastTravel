namespace Order.Application.Common.Models;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public List<string> Errors { get; } = new();

    protected Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
    public static Result<T> Failure(List<string> errors)
    {
        var result = new Result<T>(false, default, errors.FirstOrDefault());
        result.Errors.AddRange(errors);
        return result;
    }
}

public class Result : Result<object>
{
    protected Result(bool isSuccess, string? error) : base(isSuccess, null, error) { }

    public static Result Success() => new(true, null);
    public new static Result Failure(string error) => new(false, error);
}