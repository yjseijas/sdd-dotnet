namespace RealtorApi.Infrastructure.Results;

public sealed record ErrorDetails(
    int StatusCode,
    string Title,
    string Detail,
    string Code);

public sealed record Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public ErrorDetails? Error { get; }

    private Result(bool isSuccess, T? value, ErrorDetails? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);

    public static Result<T> Failure(int statusCode, string title, string detail, string code = "error")
        => new(false, default, new ErrorDetails(statusCode, title, detail, code));

    public static implicit operator Result<T>(T value) => Success(value);
}
