/*
 * Analyzer.cs - Manages Code Analysis
 * yada, yada, yada
 */
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
    }
}
