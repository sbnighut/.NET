using CSAnalyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSDisplay;
using CSFileManager;

namespace SMA_Project_2_Version_1
{
    /// <summary>
    /// This is the main controller class which controls the flow of the 
    /// </summary>
    class Executive
    {
        static void Main(string[] args)
        {
            CommandLineParser _commandParser;
            FileManager _fileMgr;
            Analyzer _analyzer;
            Display _display;

            //Step 1: Read the input command line parameter from the console and extract patterns and parameters
            if (args.Length > 0)
                _commandParser = new CommandLineParser(string.Join(" ", args));
            else
                _commandParser = new CommandLineParser(Console.ReadLine());


            //Step 2: Prepare file manager to read from the current project directory all the files
            //based on the selected patterns and running parameters
            _fileMgr = new FileManager(_commandParser.Path, _commandParser.patterns, _commandParser.options);

            //this statement will set the files collection inside FileManager class
            if (_commandParser.Path.Length > 0)
                _fileMgr.findFiles(_commandParser.Path);
            else
                _fileMgr.findFiles("../../");

            //Step 3: Initialize the analyzer in order to proceed with the analysis phase
            //which involves identifying various functions used in the file along with generation of repository
            _analyzer = new Analyzer();
            _analyzer.DoAnalysis(_fileMgr.files);

            //Step 4: Display the output based on the patterns
            // s - recursive searching through subdirectories
            // x - generate xml
            // r - to display relationships
            _display = new Display();
            _display.DisplayContent(_commandParser.options);
        }
    }
}
