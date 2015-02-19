//////////////////////////////////////////////////////////////////////
//  CommandLineParser.cs - demonstrates the processing of command   //
//              line arguments and storing it in appropriate        // 
//              properties                                          //
//                                                                  //
//  Author          :   Swapnil Nighut, Syracuse University,        //
//                      315-751-3149, snighut@syr.edu               //
//////////////////////////////////////////////////////////////////////
/*
 * Module Operations
 * =================
 * The sole purpose of CommandLineParser is to seggregate command line arguments and save it in different variable
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMA_Project_2_Version_1
{
    /// <summary>
    /// This class defines methods to read command line arguments and seggregates it and save it in different properties like Path, Patterns, Options
    /// </summary>
    class CommandLineParser
    {
        //private string path;
        //List<string> patterns = new List<string>();
        //List<string> options = new List<string>();

        /// <summary>
        /// Returns a list of patterns used in the analyser
        /// </summary>
        public List<string> patterns
        {
            get;
            set;
        }

        /// <summary>
        /// Returns a list of options passed for processing files
        /// </summary>
        public List<string> options
        {
            get;
            set;
        }

        private string path = string.Empty;

        /// <summary>
        /// Gets or sets the path in which the type analysis should begin
        /// </summary>
        public string Path
        {
            get;
            set;
        }

        /// <summary>
        /// This method defines the constructor of the command line parser
        /// </summary>
        /// <param name="consoleParameters"></param>
        public CommandLineParser(string consoleParameters)
        {
            path = string.Empty;
            options = new List<string>();
            patterns = new List<string>();
            patterns.Clear();
            options.Clear();
            RetrievePathPatternsOptions(consoleParameters);
        }

        /// <summary>
        /// seggregate path, patterns and options into separate string variables
        /// </summary>
        /// <param name="commandLine"></param>
        private void RetrievePathPatternsOptions(string commandLine)
        {
            //Retrieve path
            path = commandLine.Split(' ').FirstOrDefault(str => str[0] == '"');

            if (path == null)
            {
                Path = commandLine.Split(' ').First();
            }

            if ((path != null) && (Path.First() == '"'))
            {
                path = path.Remove(0, 1);
                path = path.Remove(Path.Length - 1, 1);
            }

            //Add all the patterns such as *.cs, *.txt to patterns list
            patterns.AddRange(commandLine.Split(' ').Where(str => str.Contains(".") && (!str.Contains(".exe") && (!str.Contains("/")))));

            //Add all the running options such as /r, /h to options list
            options.AddRange(commandLine.Split(' ').Where(str => str.StartsWith("/")));
            //Remove '/' character from each element in the options list
            options = options.Select(s => s.Remove(0, 1)).ToList<string>();
        }

#if(TEST_CMDPARSER)

        //----< test stub >--------------------------------------------------

        [STAThread]
        static void Main(string[] args)
        {
            Console.Write("\n  Testing Command Line Parser Operations");
            Console.Write("\n ============================\n");
            string testCommandArguments = string.Concat("TestFiles" + " *.cs* ", " /s ");

            CommandLineParser test = new CommandLineParser(testCommandArguments);
            test.RetrievePathPatternsOptions(testCommandArguments);
            Console.WriteLine("Test options: {0}", test.options);
            Console.WriteLine("Test Patterns: {0}", test.patterns);
            Console.WriteLine("Test Path {0}", test.Path);
            
            Console.Write("\n");
        }
#endif

    }
}