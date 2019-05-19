using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using log4net;

namespace Radar.utils
{
    class FileUtils
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(FileUtils));

        public static string readTxtFile(string path)
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
                closeQuiet(streamReader);
                closeQuiet(stream);
            }
        }

        public static void writeTxtFile(string path, string value)
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
                closeQuiet(streamWriter);
                closeQuiet(stream);
            }
        }

        public static void readFile(string path, ref byte[] fileBuffer)
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
                closeQuiet(fs);
            }
        }

        public static void writeFile(string path, byte[] fileBuffer)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                fs.Write(fileBuffer, 0, fileBuffer.Length);
                fs.Close();
                fs.Dispose();
            }
            catch (Exception)
            {
                ;
            }
        }
        public static void createDir(string dir)
        {
            if (isDirExist(dir))
            {
                return;
            }
            Directory.CreateDirectory(dir);
        }

        public static void deleteDir(string dir)
        {
            if (isDirExist(dir))
            {
                Directory.Delete(dir);
            }
        }

        public static void deleteFile(string path)
        {
            if (isFileExist(path))
            {
                File.Delete(path);
            }
        }

        public static void deleteFolder(string path)
        {
            string[] diList = Directory.GetDirectories(path);
            foreach (var item in diList)
            {
                deleteFolder(item);
            }
            string[] fileList = Directory.GetFiles(path);
            foreach (var item in fileList)
            {
                deleteFile(item);
            }
            Directory.Delete(path);
        }
        public static int getFileSize(string file)
        {
            FileInfo fileInfo = new FileInfo(file);
            return (int)fileInfo.Length;
        }
        public static void findFilesFullName(string path, ref List<string> fileList, string noContain)
        {
            if (!isDirExist(path))
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
        public static void findFiles(string path, ref List<string> fileList, bool recursive)
        {
            if (!isDirExist(path))
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
                    findFiles(item, ref fileList, recursive);
                }
            }
        }
        public static void reName(string fileName, string newFileName)
        {
            FileInfo info = new FileInfo(fileName);
            info.MoveTo(newFileName);
        }

        public static bool isDirExist(string dir)
        {
            return Directory.Exists(dir);
        }
        public static bool isFileExist(string path)
        {
            return File.Exists(path);
        }

        public static void closeQuiet(Stream stream)
        {
            try
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
            catch (Exception)
            {

            }
        }

        public static void closeQuiet(TextReader reader)
        {
            try
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
            catch (Exception)
            {

            }
        }

        public static void closeQuiet(TextWriter writer)
        {
            try
            {
                if (writer != null)
                {
                    writer.Close();
                }
            }
            catch (Exception)
            {

            }
        }

        

    }
}
