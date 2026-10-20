using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Core.Extensions
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception e)
            {
                await HandleExceptionAsync(httpContext, e);
            }
        }

        private Task HandleExceptionAsync(HttpContext httpContext, Exception e)
        {
            httpContext.Response.ContentType = "application/json";

            if (e is ValidationException validationException)
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return httpContext.Response.WriteAsync(new ValidationErrorDetails
                {
                    StatusCode = httpContext.Response.StatusCode,
                    // İlk hata mesajı doğrudan gösterilebilsin diye Message'a da yazılır
                    Message = validationException.Errors.FirstOrDefault()?.ErrorMessage ?? "Doğrulama hatası.",
                    // Sadece alan adı ve mesaj döner; gönderilen değer (ör. parola) yanıta yansıtılmaz
                    Errors = validationException.Errors
                        .Select(f => new ValidationErrorItem(f.PropertyName, f.ErrorMessage))
                        .ToList()
                }.ToString());
            }

            if (e is UnauthorizedAccessException)
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return httpContext.Response.WriteAsync(new ErrorDetails
                {
                    StatusCode = httpContext.Response.StatusCode,
                    Message = e.Message
                }.ToString());
            }

            // Beklenmeyen hatanın ayrıntısı sadece loglanır; istemciye iç bilgi (SQL, stack trace) gönderilmez.
            _logger.LogError(e, "İşlenmeyen hata: {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return httpContext.Response.WriteAsync(new ErrorDetails
            {
                StatusCode = httpContext.Response.StatusCode,
                Message = "Sunucuda beklenmeyen bir hata oluştu."
            }.ToString());
        }
    }
}
