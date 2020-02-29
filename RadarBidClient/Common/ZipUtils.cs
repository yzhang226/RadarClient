using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Radar.Common
{
    public class ZipUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="resultGZipFilePath"></param>
        /// <param name="fileToCompress"></param>
        public static void Compress(string resultGZipFilePath, string filePathToCompress)
        {
            FileInfo fileToCompress = new FileInfo(filePathToCompress);
            // fileToCompress.OpenRead()
            using (FileStream originalFileStream = new FileStream(filePathToCompress, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if ((File.GetAttributes(fileToCompress.FullName) & FileAttributes.Hidden) != FileAttributes.Hidden
                    & fileToCompress.Extension != ".gz")
                {
                    using (FileStream compressedFileStream = File.Create(resultGZipFilePath))
                    {
                        using (GZipStream compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
                        {
                            originalFileStream.CopyTo(compressionStream);
                        }
                    }
                }
            }
        }


    }
}
