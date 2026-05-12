using Microsoft.AspNetCore.Http;
using SiyahaJobs.Web.Helpers;

namespace SiyahaJobs.Web.Services.Interfaces;

public enum UploadKind
{
    Avatar,
    Logo,
    Cv
}

public interface IFileUploadService
{
    Task<OperationResult<string>> UploadAsync(IFormFile file, UploadKind kind);
    void DeleteIfExists(string? relativePath);
}
