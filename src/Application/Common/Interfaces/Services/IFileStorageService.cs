using Microsoft.AspNetCore.Http;

namespace Application.Common.Interfaces.Services;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string folderName, Guid id, CancellationToken cancellationToken);
    Task<string> DeleteFileAsync(string folderName, Guid id, CancellationToken cancellationToken);
}