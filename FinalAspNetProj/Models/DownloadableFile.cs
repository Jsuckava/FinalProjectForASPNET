using System;
using System.Collections.Generic;

namespace FinalAspNetProj.Models;

public partial class DownloadableFile
{
    public int FileId { get; set; }

    public string FileName { get; set; } = null!;

    public DateOnly DateCreated { get; set; }

    public string FileType { get; set; } = null!;

    public string FilePath { get; set; } = null!;
}
