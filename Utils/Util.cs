
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
                {
                    MakeEntry(zipArchive, p, basePath);
                    var dirInfo = new DirectoryInfo(p);
                    if (!dirInfo.Exists) continue;
                    var subInfos = dirInfo.EnumerateFileSystemInfos("*", SearchOption.AllDirectories);
                    foreach (var info in subInfos) 
                    {
                        MakeEntry(zipArchive, info.FullName, basePath);
                    }
                }
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
                zipArchive.CreateEntry(Path.GetRelativePath(basePath, path) + Path.DirectorySeparatorChar);
            }
            else
            {
                throw new IOException("Path is not a file or directory");
            }
        }
    }
}