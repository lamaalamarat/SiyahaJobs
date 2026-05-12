using Microsoft.AspNetCore.Http;
using SiyahaJobs.Web.Helpers;
using SiyahaJobs.Web.Services.Interfaces;

namespace SiyahaJobs.Web.Services;

public class FileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;

    public FileUploadService(IWebHostEnvironment env, IConfiguration config)
    {
        _env = env;
        _config = config;
    }

    public async Task<OperationResult<string>> UploadAsync(IFormFile file, UploadKind kind)
    {
        if (file == null || file.Length == 0)
            return OperationResult<string>.Fail("No file provided.");

        var (folder, maxMb, allowedExt) = ResolveKind(kind);
        var maxBytes = maxMb * 1024 * 1024;
        if (file.Length > maxBytes)
            return OperationResult<string>.Fail($"File exceeds the {maxMb} MB limit.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExt.Contains(ext))
            return OperationResult<string>.Fail($"File type not allowed. Allowed: {string.Join(", ", allowedExt)}");

        var physicalFolder = Path.Combine(_env.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(physicalFolder);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(physicalFolder, fileName);

        await using (var fs = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(fs);
        }

        var relativePath = $"/uploads/{folder}/{fileName}".Replace("\\", "/");
        return OperationResult<string>.Ok(relativePath, "File uploaded.");
    }

    public void DeleteIfExists(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return;
        var trimmed = relativePath.TrimStart('/');
        var fullPath = Path.Combine(_env.WebRootPath, trimmed);
        if (File.Exists(fullPath))
        {
            try { File.Delete(fullPath); } catch { /* swallow */ }
        }
    }

    private (string Folder, int MaxMb, string[] Allowed) ResolveKind(UploadKind kind)
    {
        var imageExt = _config.GetSection("FileUpload:AllowedImageExtensions").Get<string[]>()
                       ?? new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var cvExt = _config.GetSection("FileUpload:AllowedCvExtensions").Get<string[]>()
                    ?? new[] { ".pdf", ".doc", ".docx" };

        return kind switch
        {
            UploadKind.Avatar => ("avatars", _config.GetValue<int?>("FileUpload:MaxAvatarSizeMB") ?? 2, imageExt),
            UploadKind.Logo   => ("logos",   _config.GetValue<int?>("FileUpload:MaxLogoSizeMB")   ?? 3, imageExt),
            UploadKind.Cv     => ("cvs",     _config.GetValue<int?>("FileUpload:MaxCvSizeMB")     ?? 5, cvExt),
            _ => ("misc", 2, imageExt)
        };
    }
}
