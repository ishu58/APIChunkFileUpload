using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace FileUploadSample.Controllers.Repository
{
    public interface ILocalFileAccessor
    {
        string CreateTmpDirectory(string fileName);
        Task<bool> SaveChunkAsync(string directoryName, int fileIndex, IFormFile file);
        Task<bool> MergeChunksAsync( string fileName);
    }
}
