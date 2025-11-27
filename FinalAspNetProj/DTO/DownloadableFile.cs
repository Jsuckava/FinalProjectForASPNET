using System;

namespace FinalAspNetProj.DTO
{
    public class DownloadableFile_ReadDTO
    {
        public int FileId { get; set; }
        public string FileName { get; set; } = null!;
        public DateTime DateCreated { get; set; }
        public string FileType { get; set; } = null!;
        public string FilePath { get; set; } = null!;
    }
}