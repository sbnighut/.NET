/*
 * FileMgr.cs - Prototype of Pr#2 FileMgr
 * 
 * Platform:    Surface Pro 3, Win 8.1 pro, Visual Studio 2013
 * Application: CSE681 - SMA Helper
 * Author:      Jim Fawcett, yada, yada, yada
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSFileManager
{
    /// <summary>
    /// This class will deal with
    /// 1. Reading bunch of files from the current directory based on the patterns specified
    /// 2. Sequencing the files for read operation i.e for further processing
    /// </summary>
    public class FileManager
    {
        private List<string> patterns = new List<string>();
        private bool recurse = false;
        private string path;
        private List<string> options = new List<string>();

        /// <summary>
        /// Gets or sets the Files collection
        /// </summary>
        public List<string> files
        {
            get;
            set;
        }

        /// <summary>
        /// This method defines the constructor for FileManager class
        /// </summary>
        /// <param name="patterns"></param>
        /// <param name="path"></param>
        public FileManager(string path, List<string> patterns, List<string> options)
        {
            this.patterns = patterns;
            this.path = path;
            this.options = options;
            files = new List<string>();
            recurse = options.Any(str => str == "s");
        }

        public void findFiles(string path)
        {
            try
            {
                if (patterns.Count == 0)
                    patterns.Add("*.*");

                foreach (string pattern in patterns)
                {
                    string[] newFiles = Directory.GetFiles(path, pattern);
                    for (int i = 0; i < newFiles.Length; ++i)
                        newFiles[i] = Path.GetFullPath(newFiles[i]);
                    files.AddRange(newFiles);

                }
                if (recurse)
                {
                    string[] dirs = Directory.GetDirectories(path);
                    foreach (string dir in dirs)
                        findFiles(dir);
                }
            }
            catch (DirectoryNotFoundException de)
            {
                //Inform the user about the incorrect path he/she has mentioned
                Console.WriteLine(de.Message);
                Console.WriteLine("************Please pass the correct path.*****************");
            }
        }

    }
}
