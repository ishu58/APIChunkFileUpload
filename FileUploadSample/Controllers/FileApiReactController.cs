using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileUploadSample.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FileUploadSample.Controllers
{


    [ApiController]
    [Route("[controller]")]
    public class FileApiReactController : ControllerBase
    {
        private IConfiguration configuration;

        private readonly ILogger<FileApiReactController> _logger;
        private readonly ResponseContext _responseData;
        public int chunkSize;
        private string tempFolder;

        public FileApiReactController(IConfiguration configuration, ILogger<FileApiReactController> logger)
        {
            this.configuration = configuration;
            _logger = logger;
            chunkSize = 1048576 * Convert.ToInt32(configuration["ChunkSize"]);
            tempFolder = configuration["TargetFolder"];
            _responseData = new ResponseContext();
        }

       

        [HttpPost("UploadChunks")]
        public async Task<IActionResult> UploadChunks(string id, string fileName)
        {
            try
            {
                var chunkNumber = id;
                string folderPath = Path.Combine(tempFolder, "Temp");
                string newpath = Path.Combine(folderPath, fileName + chunkNumber);

                // Create the directory if it doesn't exist
                Directory.CreateDirectory(folderPath);

                using (FileStream fs = System.IO.File.Create(newpath))
                {
                    byte[] bytes = new byte[chunkSize];
                    int bytesRead = 0;
                    while ((bytesRead = await Request.Body.ReadAsync(bytes, 0, bytes.Length)) > 0)
                    {
                        fs.Write(bytes, 0, bytesRead);
                    }
                }

                _responseData.IsSuccess = true;

            }
            catch (Exception ex)
            {
                _responseData.ErrorMessage = ex.Message;
                _responseData.IsSuccess = false;
            }
            return Ok(_responseData);
        }


        [HttpPost("UploadComplete")]
        public IActionResult UploadComplete(string fileName)
        {
            try
            {
                string tempPath = tempFolder + "/Temp";
                string newPath = Path.Combine(tempPath, fileName);
                string[] filePaths = Directory.GetFiles(tempPath).Where(p => p.Contains(fileName)).OrderBy(p => Int32.Parse(p.Replace(fileName, "$").Split('$')[1])).ToArray();
                foreach (string filePath in filePaths)
                {
                    MergeChunks(newPath, filePath);
                }
                System.IO.File.Move(Path.Combine(tempPath, fileName), Path.Combine(tempFolder, fileName));
            }
            catch (Exception ex)
            {
                _responseData.ErrorMessage = ex.Message;
                _responseData.IsSuccess = false;
            }
            return Ok(_responseData);
        }

        private static void MergeChunks(string chunk1, string chunk2)
        {
            FileStream fs1 = null;
            FileStream fs2 = null;
            try
            {
                fs1 = System.IO.File.Open(chunk1, FileMode.Append);
                fs2 = System.IO.File.Open(chunk2, FileMode.Open);
                byte[] fs2Content = new byte[fs2.Length];
                fs2.Read(fs2Content, 0, (int)fs2.Length);
                fs1.Write(fs2Content, 0, (int)fs2.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " : " + ex.StackTrace);
            }
            finally
            {
                if (fs1 != null) fs1.Close();
                if (fs2 != null) fs2.Close();
                System.IO.File.Delete(chunk2);
            }
        }
    }
}