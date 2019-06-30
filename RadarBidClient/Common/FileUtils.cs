using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using log4net;

namespace Radar.Common
{
    public class FileUtils
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof( FileUtils));

        public static string ReadTxtFile(string path)
        {
            if (!File.Exists(path))
            {
                return "";
            }

            FileStream stream = null;
            StreamReader streamReader = null;
            try
            {
                stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                streamReader = new StreamReader(stream);
                string value = streamReader.ReadToEnd();
                return value;
            }
            catch (Exception e)
            {
                logger.Error("readTxtFile error", e);
                throw e;
            }
            finally
            {
                CloseQuiet(streamReader);
                CloseQuiet(stream);
            }
        }

        public static void WriteTxtFile(string path, string value)
        {
            if (!File.Exists(path))
            {
                File.Create(path);
            }

            FileStream stream = null;
            StreamWriter streamWriter = null;
            try
            {
                stream = new FileStream(path, FileMode.Create, FileAccess.Write);
                streamWriter = new StreamWriter(stream);
                streamWriter.Write(value);
            }
            catch (Exception e)
            {
                logger.Error("writeTxtFile error", e);
                throw e;
            }
            finally
            {
                CloseQuiet(streamWriter);
                CloseQuiet(stream);
            }
        }

        public static void ReadFile(string path, ref byte[] fileBuffer)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                int fileSize = (int)fs.Length;
                fileBuffer = new byte[fileSize];
                fs.Read(fileBuffer, 0, fileSize);
            }
            catch (Exception e)
            {
                logger.Error("", e);
            }
            finally
            {
                CloseQuiet(fs);
            }
        }

        public static void WriteFile(string path, byte[] fileBuffer)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                fs.Write(fileBuffer, 0, fileBuffer.Length);
                fs.Close();
                fs.Dispose();
            }
            catch (Exception e)
            {
                logger.Error("WriteFile error", e);
            }
        }

        public static void CreateDir(string dir)
        {
            if (IsDirExist(dir))
            {
                return;
            }
            Directory.CreateDirectory(dir);
        }

        public static void DeleteDir(string dir)
        {
            if (IsDirExist(dir))
            {
                Directory.Delete(dir);
            }
        }

        public static void DeleteFile(string path)
        {
            if (IsFileExist(path))
            {
                File.Delete(path);
            }
        }

        public static void DeleteFolder(string path)
        {
            string[] diList = Directory.GetDirectories(path);
            foreach (var item in diList)
            {
                DeleteFolder(item);
            }
            string[] fileList = Directory.GetFiles(path);
            foreach (var item in fileList)
            {
                DeleteFile(item);
            }
            Directory.Delete(path);
        }

        public static int GetFileSize(string file)
        {
            FileInfo fileInfo = new FileInfo(file);
            return (int)fileInfo.Length;
        }

        public static void FindFilesFullName(string path, ref List<string> fileList, string noContain)
        {
            if (!IsDirExist(path))
            {
                return;
            }
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] info = dir.GetFiles();
            foreach (var item in info)
            {
                if (noContain != "")
                {
                    if (item.FullName.Contains(noContain))
                    {
                        continue;
                    }
                    item.FullName.Replace(@"\\", "/");
                    fileList.Add(item.FullName);
                }
            }
        }

        public static void FindFiles(string path, ref List<string> fileList, bool recursive)
        {
            if (!IsDirExist(path))
            {
                return;
            }
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] info = dir.GetFiles();
            foreach (var item in info)
            {
                fileList.Add(item.Name);
            }

            if (recursive)
            {
                string[] dirs = Directory.GetDirectories(path);
                foreach (var item in dirs)
                {
                    FindFiles(item, ref fileList, recursive);
                }
            }
        }

        public static void Rename(string fileName, string newFileName)
        {
            FileInfo info = new FileInfo(fileName);
            info.MoveTo(newFileName);
        }

        public static bool IsDirExist(string dir)
        {
            return Directory.Exists(dir);
        }

        public static bool IsFileExist(string path)
        {
            return File.Exists(path);
        }

        public static void CloseQuiet(Stream stream)
        {
            try
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
            catch (Exception e)
            {
                logger.Error("CloseQuiet error", e);
            }
        }

        public static void CloseQuiet(TextReader reader)
        {
            try
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
            catch (Exception e)
            {
                logger.Error("CloseQuiet error", e);
            }
        }

        public static void CloseQuiet(TextWriter writer)
        {
            try
            {
                if (writer != null)
                {
                    writer.Close();
                }
            }
            catch (Exception e)
            {
                logger.Error("CloseQuiet error", e);
            }
        }

        

    }
}
