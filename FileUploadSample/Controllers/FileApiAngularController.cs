using FileUploadSample.Controllers.Repository;
using FileUploadSample.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;


namespace FileUploadSample.Controllers
{
    public class FileApiAngularController: ControllerBase
    {
        private readonly ILocalFileAccessor _localFileAccessor;
        public FileApiAngularController(ILocalFileAccessor localFileAccessor)
        {
            _localFileAccessor = localFileAccessor;
        }

        [HttpPost]
        [Route("/files/start")]
        [Produces("application/json")]
        public UploadFileResult StartUploading([FromForm] string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return new UploadFileResult
                {
                    Result = GetFailedResult("invalid file name"),
                };
            }
            //string directoryName = _localFileAccessor.CreateTmpDirectory(fileName);
            string directoryName = "geio";
            if (string.IsNullOrEmpty(directoryName))
            {
                return new UploadFileResult
                {
                    Result = GetFailedResult("failed create directory"),
                };
            }
            return new UploadFileResult
            {
                Result = GetSuccessResult(),
                TmpDirectoryName = directoryName,
            };
        }
        
        [HttpPost]
        [Route("/files/chunk")]
        [Produces("application/json")]
        public async Task<UploadResult> UploadChunk([FromForm] ChunkUploadData uploadData)
        {
            //string directoryName = uploadData.tmpDirectory;           


            if (uploadData.file == null)
            {
                return GetFailedResult("no file data");
            }
            //if (string.IsNullOrEmpty(directoryName))
            //{
            //    return GetFailedResult("no directory name");
            //}
            if (int.TryParse(uploadData.index, out int fileIndex) == false)
            {
                return GetFailedResult("no file index");
            }


            bool result = await _localFileAccessor.SaveChunkAsync("tempFile", fileIndex, uploadData.file);
            return (result) ? GetSuccessResult() : GetFailedResult("failed saving chunk data");
        }
        
        [HttpPost]
        [Route("/files/end")]
        [Produces("application/json")]
        public async Task<UploadResult> EndUploading([FromForm] string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return GetFailedResult("invalid file name");
            }
            //if (string.IsNullOrEmpty(tmpDirectory))
            //{
            //    return GetFailedResult("invalid directory");
            //}
            bool result = await _localFileAccessor.MergeChunksAsync(fileName);
            return (result) ? GetSuccessResult() : GetFailedResult("failed saving file");
        }
        private static UploadResult GetSuccessResult()
        {
            return new UploadResult
            {
                Succeeded = true,
            };
        }
        private static UploadResult GetFailedResult(string reason)
        {
            return new UploadResult
            {
                Succeeded = false,
                ErrorMessage = reason,
            };
        }
    }
}
