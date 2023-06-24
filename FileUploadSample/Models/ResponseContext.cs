namespace FileUploadSample.Models
{
    public class ResponseContext
    {
        public dynamic Data { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string ErrorMessage { get; set; }
    }
}
