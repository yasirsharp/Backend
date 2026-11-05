using System.Text.Json;

namespace Core.Extensions
{
    /// <summary>
    /// ExceptionMiddleware'in döndüğü hata gövdesi. Diğer API yanıtlarıyla aynı biçimdedir
    /// ({ success: false, message }), böylece istemci tek bir yolla hata mesajını okuyabilir.
    /// </summary>
    public class ErrorDetails
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public bool Success => false;
        public string Message { get; set; } = "";
        public int StatusCode { get; set; }

        // Önceden Newtonsoft.Json kullanılıyordu; paket API'nin bağımlılıklarında olmadığı için her doğrulama
        // hatası (ValidationAspect) "dosya bulunamadı" istisnasıyla 500'e dönüşüyordu.
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, GetType(), JsonOptions);
        }
    }

    public class ValidationErrorDetails : ErrorDetails
    {
        public IEnumerable<ValidationErrorItem> Errors { get; set; } = Array.Empty<ValidationErrorItem>();
    }

    public record ValidationErrorItem(string PropertyName, string ErrorMessage);
}
