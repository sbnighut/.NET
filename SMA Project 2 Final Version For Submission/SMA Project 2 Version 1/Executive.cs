//////////////////////////////////////////////////////////////////////////
// Executive.cs  -  Executive                                           //
//              Main controller of the system. Calls appropriate        //
//              functions by creating module objects and                // 
//              based upon the design flow                              //
// ver 2.5                                                              //
// Language:    C#, Visual Studio 13.0, .Net Framework 4.5              //
// Platform:    Lenovo G580 , Win 8.1, SP 1                      //
// Application: Pr#2 Help, CSE681, Fall 2011                            //
// Author:      Swapnil Nighut, Syracuse University,                    //
//                      315-751-3149, snighut@syr.edu                   //
// Source:      Jim Fawcett, CST 2-187, Syracuse University             //
//              (315) 443-3948, jfawcett@twcny.rr.com                   //
//////////////////////////////////////////////////////////////////////////
/*
 * Module Operations
 * =================
 * Executive creates an instance of commandLineParse class which is used to seggregate comaand line
 * arguments into separate variables such as path, options and patterns for processing
 * 
 * It then creates an instance of FileManager and passes the path, patterns related data in the constructor
 * so that it will fetch the path names of all the files based on the passed pattern value
 * 
 * Once it has all the file names, the Executive will create an instancce of Analyzer in order to proceed with parsing i.e.
 * each file from the list of files will be looped through in order to generate the type analysis.
 * 
 * The repository will be generated in Parse 1, which might contain incorrect relationships such as aggregation, or inheritance.
 * 
 * In final Display() phase which is also the Parse 2 step, in my application, for each record in the location table all the obtained relationships will be scanned and
 * the type names will be checked in the existing Repository.ValueTypeCollection dictionary(hold a list of all the classes declared in the application)
 * to check whether the type name is of reference type or value type.
 * 
 * Display step will check for the options specied by the user in the command line.
 * Based on the provided options Display will either display or skip relationship display part
 * 
 */

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
            try
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
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
                Console.ReadKey();
            }
        }


#if(TEST_Executive)

        //----< test stub >--------------------------------------------------

        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                args = new string[] { "TestFiles", "RulesAndActions.cs", "/s" };
                CommandLineParser _testCommandParser;
                FileManager _testFileMgr;
                Analyzer _testAnalyzer;
                Display _testDisplay;

                //Step 1: Read the input command line parameter from the console and extract patterns and parameters
                if (args.Length > 0)
                    _testCommandParser = new CommandLineParser(string.Join(" ", args));
                else
                    _testCommandParser = new CommandLineParser(Console.ReadLine());

                _testFileMgr = new FileManager(_testCommandParser.Path, _testCommandParser.patterns, _testCommandParser.options);

                if (_testCommandParser.Path.Length > 0)
                    _testFileMgr.findFiles(_testCommandParser.Path);
                else
                    _testFileMgr.findFiles("../../");

                _testAnalyzer = new Analyzer();
                _testAnalyzer.DoAnalysis(_testFileMgr.files);

                _testDisplay = new Display();
                _testDisplay.DisplayContent(_testCommandParser.options);
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
                Console.ReadKey();
            }
        }
#endif

    }
}
