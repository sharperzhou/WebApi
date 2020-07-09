
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace WebApi.Utils
{
    public class Util
    {
        public static Stream Compress(string basePath, params string[] path)
        {
            var stream = new MemoryStream();
            using (var zipArchive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                foreach (var p in path)
                    MakeEntry(zipArchive, Path.GetFullPath(p), Path.GetFullPath(basePath));
            }
            stream.Seek(0, SeekOrigin.Begin);
            stream.Flush();
            return stream;
        }

        static void MakeEntry(ZipArchive zipArchive, string path, string basePath)
        {
            if (File.Exists(path))
            {
                zipArchive.CreateEntryFromFile(path, Path.GetRelativePath(basePath, path));
            }
            else if (Directory.Exists(path))
            {
                var dirInfo = new DirectoryInfo(path);
                foreach (var fileInfo in dirInfo.GetFiles())
                    zipArchive.CreateEntryFromFile(fileInfo.FullName, Path.GetRelativePath(basePath, fileInfo.FullName));

                foreach (var d in dirInfo.GetDirectories())
                    MakeEntry(zipArchive, d.FullName, basePath);
            }
            else
                throw new IOException("Path is not a file or directory");
        }
    }
}