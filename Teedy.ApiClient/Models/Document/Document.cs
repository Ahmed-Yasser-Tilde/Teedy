namespace Teedy.ApiClient.Models.Document
{
    public class Document
    {
        public string ID { get; set; }   //Document ID
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? Subject { get; set; }
        public string? Identifier { get; set; }
        public string? Publisher { get; set; }
        public string? Format { get; set; }
        public string? Source { get; set; }
        public string? Type { get; set; }
        public string? Coverage { get; set; }
        public string? Rights { get; set; }
        public List<string>? Tags { get; set; }    //List of tags ID
        public List<string>? Relations { get; set; }  //List of related documents ID
        public List<string>? MetaDataIds { get; set; }   //List of metadata ID
        public List<string>? MetadataValues { get; set; }   //List of metadata values
        public string Language { get; set; }
        public long? CreateDate { get; set; } // Timestamp stored as long


    }
}
