//////////////////////////////////////////////////////////////////////
// Analyzer.cs - Manages Code Analysis                              //
//              using System.Xml.Linq.XDocument                     //
//                                                                  //
//  Author          :   Swapnil Nighut, Syracuse University,        //
//                      315-751-3149, snighut@syr.edu               //
//  Source          :   Jim Fawcett                                 //
//////////////////////////////////////////////////////////////////////

using CodeAnalysis;
using CSsemi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSAnalyzer
{
    /// <summary>
    /// This class is used to iterate through a number of files and scan all the class type declarations along with the functions and other declarations
    /// </summary>
    public class Analyzer
    {
        public void DoAnalysis(List<string> files)
        {
            CSemiExp semi = new CSemiExp();
            BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
            Parser parser = builder.build();
            Repository rep = Repository.getInstance();
            InsertValueTypes(rep);

            Console.Write("\n===================================================================");
            Console.Write("\n               Type and Function Analysis started");
            Console.Write("\n===================================================================");
            
            foreach (object file in files)
            {
                string fileName = file as string;
                fileName = fileName.Split('\\').Last();
                rep.CurrentFile = fileName.ToString().Split('\\').Last();
                Console.Write("\nProcessing file {0}...............................\n", fileName);
                semi.displayNewLines = false;
                if (!semi.open(file as string))
                {
                    Console.Write("\n  Can't open {0}\n\n", file);
                    return;
                }
                Console.WriteLine("\nAnalyzing file {0}................................", fileName);

                try
                {
                    while (semi.getSemi())
                        parser.parse(semi);
                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  {0}\n", ex.Message);
                }
                Console.WriteLine("\nAnalysis of file {0} is completed", fileName);
                semi.close();
                Console.Write("\n----------------------------------------------------------------------------");
            }
        }

        /// <summary>
        /// Set the dictionary of all value types for the first time
        /// </summary>
        /// <param name="rep"></param>
        private void InsertValueTypes(Repository rep)
        {
            rep.DelegateInstanceCollection = new List<Elem>();
            rep.ValueTypeCollection = new Dictionary<string, bool>();
            rep.ValueTypeCollection.Add("int", true);
            rep.ValueTypeCollection.Add("bool", true);
            rep.ValueTypeCollection.Add("byte", true);
            rep.ValueTypeCollection.Add("char", true);
            rep.ValueTypeCollection.Add("double", true);
            rep.ValueTypeCollection.Add("decimal", true);
            rep.ValueTypeCollection.Add("enum", true);
            rep.ValueTypeCollection.Add("float", true);
            rep.ValueTypeCollection.Add("long", true);
            rep.ValueTypeCollection.Add("sbyte", true);
            rep.ValueTypeCollection.Add("short", true);
            rep.ValueTypeCollection.Add("struct", true);
            rep.ValueTypeCollection.Add("uint", true);
            rep.ValueTypeCollection.Add("ulong", true);
            rep.ValueTypeCollection.Add("ushort", true);
        }



#if(TEST_SEMI)

    //----< test stub >--------------------------------------------------

    [STAThread]
    static void Main(string[] args)
    {
      Console.Write("\n  Testing semiExp Operations");
      Console.Write("\n ============================\n");

      CSemiExp test = new CSemiExp();
      test.returnNewLines = true;
      test.displayNewLines = true;

      string testFile = "../../testSemi.txt";
      if(!test.open(testFile))
        Console.Write("\n  Can't open file {0}",testFile);
      while(test.getSemi())
        test.display();
      
      test.initialize();
      test.insert(0,"this");
      test.insert(1,"is");
      test.insert(2,"a");
      test.insert(3,"test");
      test.display();

      Console.Write("\n  2nd token = \"{0}\"\n",test[1]);

      Console.Write("\n  removing first token:");
      test.remove(0);
      test.display();
      Console.Write("\n");

      Console.Write("\n  removing token \"test\":");
      test.remove("test");
      test.display();
      Console.Write("\n");

      Console.Write("\n  making copy of semiExpression:");
      CSemiExp copy = test.clone();
      copy.display();
      Console.Write("\n");

      if(args.Length == 0)
      {
        Console.Write("\n  Please enter name of file to analyze\n\n");
        return;
      }
      CSemiExp semi = new CSemiExp();
      semi.returnNewLines = true;
      if(!semi.open(args[0]))
      {
        Console.Write("\n  can't open file {0}\n\n",args[0]);
        return;
      }

      Console.Write("\n  Analyzing file {0}",args[0]);
      Console.Write("\n ----------------------------------\n");

      while(semi.getSemi())
        semi.display();
      semi.close();
    }
#endif
    }
}
