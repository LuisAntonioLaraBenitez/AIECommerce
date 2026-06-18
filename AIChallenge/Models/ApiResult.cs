namespace AIChallenge.Models;

public sealed record ApiResult<T>(bool Success, T? Data, ApiError? Error)
{
    public static ApiResult<T> Ok(T data) => new(true, data, null);

    public static ApiResult<T> Fail(string code, string message) => new(false, default, new ApiError(code, message));
}
