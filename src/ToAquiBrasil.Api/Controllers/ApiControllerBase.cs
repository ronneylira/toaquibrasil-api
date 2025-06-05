using Microsoft.AspNetCore.Mvc;
using ToAquiBrasil.Api.Dtos;

namespace ToAquiBrasil.Api.Controllers;

[ApiController]
[Route("[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult<ApiResponse<T>> Success<T>(T data, string message = "Success", int statusCode = 200)
    {
        var response = ApiResponse<T>.CreateSuccess(data, message, statusCode);
        return StatusCode(statusCode, response);
    }

    protected ActionResult<ApiResponse<T>> Error<T>(string message, int statusCode, string? errorCode = null)
    {
        var response = ApiResponse<T>.CreateError(message, statusCode, errorCode);
        return StatusCode(statusCode, response);
    }
} 