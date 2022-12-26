using System;
using System.IO;

namespace OpenFileExample
{
    public class FileInfo2
    {
        Stream _file;

        public string Name { get; }
        public long Length { get; }
        public FileInfo2(object o, Stream file)
        {
            Type fileType = o.GetType();
            _file = file;
            Name = fileType.GetProperty("Name").GetValue(o).ToString();
            Length = (long)fileType.GetProperty("Size").GetValue(o);
        }

        public Stream OpenRead()
        {
            return _file;
        }
    }
}
