using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileUploadSample.Models
{
    public class ChunkUploadData
    {
        public string index { get; set; }
        public IFormFile file { get; set; }
    }

}
