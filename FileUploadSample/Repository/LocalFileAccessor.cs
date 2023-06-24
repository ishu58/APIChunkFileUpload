using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileUploadSample.Controllers.Repository
{
    public class LocalFileAccessor : ILocalFileAccessor
    {
        private const string TmpFileDirectory = @"D:\Angular  Project Practice/Files/tmp/";
        private const string ActualFileDirectory = @"D:\Angular  Project Practice\ActualFile\";
        // Create root directories
        public LocalFileAccessor()
        {
            CreateRootDirectories();
        }
        public string CreateTmpDirectory(string fileName)
        {
            CreateRootDirectories();
            // Create directory for saving chunks
            string directoryName = GetUniqueDirectoryPath(TmpFileDirectory,
                $"{DateTime.Now:yyyyMMddHHmmssfff}_{"tempFile"}");
            DirectoryInfo info = Directory.CreateDirectory($"{TmpFileDirectory}{directoryName}");
            return directoryName;
        }
        public async Task<bool> SaveChunkAsync(string directoryName, int fileIndex, IFormFile file)
        {
            string directoryPath = $"{TmpFileDirectory}";
            if (Directory.Exists(directoryPath) == false)
            {
                return false;
            }
            string filePath = $"{directoryPath}/{fileIndex.ToString()}_{directoryName}";
            await using FileStream stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
            return true;
        }
        public async Task<bool> MergeChunksAsync(string fileName)
        {
            CreateRootDirectories();
            string tmpDirectoryPath = $"{TmpFileDirectory}";
            if (Directory.Exists(tmpDirectoryPath) == false)
            {
                return false;
            }
            // Read all chunks by byte data
            List<byte> readBytes = new List<byte>();
            using (PhysicalFileProvider provider = new PhysicalFileProvider(tmpDirectoryPath))
            {
                foreach (IFileInfo fileInfo in provider.GetDirectoryContents(string.Empty)
                    .Where(f => f.IsDirectory == false)
                    .OrderBy(f => {
                        string[] fileNames = f.Name.Split('_');
                        int.TryParse(fileNames[0], out int index);
                        return index;
                    }))
                {
                    await using Stream reader = fileInfo.CreateReadStream();
                    int fileLength = (int)fileInfo.Length;
                    byte[] newReadBytes = new byte[fileLength];
                    reader.Read(newReadBytes, 0, fileLength);
                    readBytes.AddRange(newReadBytes);
                }
            }
            // Output to one file
            await using FileStream stream = new FileStream($"{ActualFileDirectory}{fileName}", FileMode.Create);
            await stream.WriteAsync(readBytes.ToArray(), 0, readBytes.Count);

            // Delete chunks and directory
            await DeleteTmpFilesAsync(tmpDirectoryPath);
            return true;
        }
        private static void CreateRootDirectories()
        {
            if (Directory.Exists(ActualFileDirectory) == false)
            {
                DirectoryInfo info = Directory.CreateDirectory(ActualFileDirectory);
            }

            if (Directory.Exists(TmpFileDirectory) == false)
            {
                DirectoryInfo info = Directory.CreateDirectory(TmpFileDirectory);
            }
        }
        private static string GetUniqueDirectoryPath(string rootDirectory, string original)
        {
            string directoryName = original;
            if (Directory.Exists($"{rootDirectory}{directoryName}"))
            {
                int count = 0;
                while (true)
                {
                    if (Directory.Exists($"{rootDirectory}{directoryName}_{count.ToString()}"))
                    {
                        count += 1;
                        continue;
                    }
                    directoryName += count.ToString();
                    break;
                }
            }
            return directoryName;
        }
        private static async Task DeleteTmpFilesAsync(string tmpDirectoryPath)
        {
            await Task.Run(() =>
            {
                using PhysicalFileProvider provider = new PhysicalFileProvider(tmpDirectoryPath);
                foreach (IFileInfo fileInfo in provider.GetDirectoryContents(string.Empty))
                {
                    if (fileInfo.IsDirectory)
                    {
                        Directory.Delete(fileInfo.PhysicalPath);
                    }
                    else
                    {
                        File.Delete(fileInfo.PhysicalPath);
                    }
                }
                Directory.Delete(tmpDirectoryPath);
            });
        }
    }
}
