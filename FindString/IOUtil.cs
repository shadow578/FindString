using System;
using System.Collections.Generic;
using System.IO;

namespace FindString
{
    public static class IOUtil
    {
        /// <summary>
        /// Get the parent directory
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static DirectoryInfo GetParent(this DirectoryInfo dir)
        {
            return Directory.GetParent(dir.FullName);
        }

        /// <summary>
        /// Get the directory this file is in
        /// </summary>
        /// <param name="file">the file</param>
        /// <returns></returns>
        public static DirectoryInfo GetDirectory(this FileInfo file)
        {
            return new DirectoryInfo(Path.GetDirectoryName(file.FullName));
        }

        /// <summary>
        /// Creates a file in the directory
        /// <para>if the file already exists, the file is opened instead</para>
        /// </summary>
        /// <param name="dir">the dir</param>
        /// <param name="filename">the name of the file</param>
        /// <param name="createIfNotExisting">should file.create be called if the file does not exist?</param>
        /// <returns>the file</returns>
        public static FileInfo CreateFile(this DirectoryInfo dir, string filename, bool createIfNotExisting = true)
        {
            FileInfo file = new FileInfo(dir.FullName + "\\" + filename);

            if (!file.Exists && createIfNotExisting)
            {
                file.Create();
            }

            return file;
        }

        /// <summary>
        /// Is this directory empty?
        /// </summary>
        /// <param name="dir">the dir</param>
        /// <returns>empty?</returns>
        public static bool IsEmpty(this DirectoryInfo dir)
        {
            return !dir.Exists || (dir.GetFiles().Length == 0 && dir.GetDirectories().Length == 0);
        }

        /// <summary>
        /// Returns the path of the file relative to the base directory
        /// <para>The file has to be inside the base directory</para>
        /// </summary>
        /// <param name="file">the file</param>
        /// <param name="baseDirPath">the base direcotry path</param>
        /// <returns>a path relative to the base dir</returns>
        public static string GetRelativePath(this FileInfo file, string baseDirPath)
        {
            //check if file exists
            if (!file.Exists)
            {
                throw new FileNotFoundException("Cannot create relative path: file not found");
            }

            //check if file path begins with base path
            string filePath = file.FullName;
            if (!filePath.ToLower().StartsWith(baseDirPath.ToLower()))
            {
                throw new InvalidOperationException("Cannot create relative path: The file is not inside the base directory");
            }

            return filePath.Substring(baseDirPath.Length + 1);
        }

        /// <summary>
        /// Searches a directory and all subdirectorys for files and returns a list of all files
        /// </summary>
        /// <param name="dir">the directory to start searching in</param>
        /// <param name="maxSubdirLevel">how many levels of sub dirs to search</param>
        /// <param name="level">internally used only. Leave as 0</param>
        /// <returns></returns> a list of all found files
        public static List<FileInfo> GetFilesIncludingSubdirs(this DirectoryInfo dir, int maxSubdirLevel = 15, int level = 0)
        {
            //init filepath and count up anti-stack-overflow var
            List<FileInfo> filePaths = new List<FileInfo>();
            level++;

            //check if dir exists
            if (!dir.Exists)
            {
                return filePaths;
            }

            //search subdirs for files if not at max search level
            if (level < maxSubdirLevel)
            {
                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    filePaths.AddRange(GetFilesIncludingSubdirs(subDir, maxSubdirLevel, level));
                }
            }

            //search for files
            foreach (FileInfo file in dir.GetFiles())
            {
                filePaths.Add(file);
            }

            return filePaths;
        }

        /// <summary>
        /// Gets the file size of the file
        /// </summary>
        /// <param name="file">the file</param>
        /// <returns>the filesize in bytes</returns> 
        public static long GetFileSize(this FileInfo file)
        {
            return file.Length;
        }
    }
}
