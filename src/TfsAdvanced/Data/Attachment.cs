
using System;

namespace TfsAdvanced.Data
{
    public class Attachment
    {
        public int Id { get; set; }
        public Guid GUID { get; set; }
        public int OriginalWorkId { get; set; }
        public string FileName { get; set; }
        public string Comment { get; set; }
        public byte[] File { get; set; }
    }

    public class AttachmentUploadResult
    {
        public string id { get; set; }
        public string url { get; set; }
    }
}
