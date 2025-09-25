using Microsoft.AspNetCore.Mvc;

namespace Labverse.API.Helpers
{
    public static class ApiErrorHelper
    {
        public class ErrorResponse
        {
            public string Code { get; set; }
            public string Message { get; set; }
            public ErrorResponse(string code, string message)
            {
                Code = code;
                Message = message;
            }
        }

        public static IActionResult Error(string code, string message, int statusCode = 400)
        {
            return new ObjectResult(new { error = new ErrorResponse(code, message) })
            {
                StatusCode = statusCode
            };
        }
    }
}
