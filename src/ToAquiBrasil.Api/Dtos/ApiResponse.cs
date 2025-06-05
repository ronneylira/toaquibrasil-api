namespace ToAquiBrasil.Api.Dtos;

public record ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; set; }
    public int StatusCode { get; init; }
    public string? ErrorCode { get; init; }
    
    public static ApiResponse<T> CreateSuccess(T data, string message = "Success", int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = statusCode
        };
    }
    
    public static ApiResponse<T> CreateError(string message, int statusCode, string? errorCode = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = statusCode,
            ErrorCode = errorCode
        };
    }
} 