using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public class CreateMessageWithAttachmentsDto
{
    public string? Content { get; set; }

    // چند فایل همزمان
    public List<IFormFile>? Files { get; set; }
}
