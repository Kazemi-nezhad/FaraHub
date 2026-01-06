using Microsoft.AspNetCore.Http;

namespace FaraHub.Web.Services
{
    public static class FileValidationService
    {
        private static readonly string[] AllowedExtensions =
        {
            ".jpg", ".jpeg", ".png", ".pdf", ".docx", ".xlsx", ".zip"
        };

        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        public static void Validate(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            {
                throw new Exception("نوع فایل مجاز نیست.");
            }

            if (file.Length <= 0 || file.Length > MaxFileSize)
            {
                throw new Exception("حجم فایل بیشتر از حد مجاز است.");
            }
        }
    }
}
