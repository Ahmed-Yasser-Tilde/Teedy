namespace Teedy.ApiClient.Models.Files
{
    public class File
    {
        public string? ID { get; set; }    //ID
        public string? FileName { get; set; }   //File name
        public string? Processing { get; set; }  //True if the file is currently processing
        public string? Size { get; set; }   //File size (in bytes)
        public string? Version { get; set; }  //Zero-based version number
        public string? MIMEtype { get; set; }   //MIME type
        public string? DocumentID { get; set; }   //Document ID
        public string? CreateDate { get; set; }    //Create date (timestamp)
    }
}
