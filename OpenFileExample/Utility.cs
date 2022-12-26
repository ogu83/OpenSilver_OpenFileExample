using System.Collections.Generic;

namespace OpenFileExample
{
    public static class Utility
    {
        public static long MaxFileSize = int.MaxValue;
        public static List<FileInfo2> TempFiles { get; set; } = new List<FileInfo2>();
        public static bool FileIsLoading { get; set; } = false;
    }
}
