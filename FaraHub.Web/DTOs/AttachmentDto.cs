namespace FaraHub.Web.DTOs
{
    public class AttachmentDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long Size { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty; // لینک دانلود ایمن
    }
}
