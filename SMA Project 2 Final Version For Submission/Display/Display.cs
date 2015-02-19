//////////////////////////////////////////////////////////////////////
//  Display.cs - Manages the display on the console screen          //
//              based upon the options provided by the user in      // 
//              run.bat file                                        //
//                                                                  //
//  Author          :   Swapnil Nighut, Syracuse University,        //
//                      315-751-3149, snighut@syr.edu               //
//  Source          :   Jim Fawcett                                 //
//////////////////////////////////////////////////////////////////////
/*
 * Module Operations
 * =================
 * When the Analyser completes analysing of all the semi-expressions through Parser 
 * the generated table is sent to Display class for Parse 2 where, based on the identified relationships 
 * appropriate message is displayed.
 * 
 * It also contains a method named RemoveUnnecessaryAggregation() which will basically remove all the aggregated types
 * collected in Parse 1, that are not user defined. For example if the 
 * words and punctuation symbols.
 */


using CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XDocument_Create_XML;

namespace CSDisplay
{
    /// <summary>
    /// This class will contain steps to display all the types used in the files along with their complexities
    /// Apart from this if the user provides the option "/x" then the display should be printed on an xml file
    /// </summary>
    public class Display
    {
        /// <summary>
        /// This method loops through the repository and displays the declared types and funtions in the console output
        /// </summary>
        /// <param name="options"></param>
        public void DisplayContent(List<string> options)
        {
            Repository rep = Repository.getInstance();

            // Generate xml if '/x' is passed in the option parameter 
            if (options.Any(option => option.ToLower() == "x"))
            {
                try
                {
                    RemoveUnnecessaryAggregation(rep);
                    var xmlEditor = new XMLEditor();
                    string fileName = String.Format("Swapnil Nighut_SMA_{0}.{1}.{2} ", DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year);
                    string fileShortName = fileName;
                    string path = Path.GetFullPath(Directory.GetCurrentDirectory());
                    Directory.GetParent(path).GetAccessControl();

                    xmlEditor.GenerateXML();

                    fileName = Path.Combine(Directory.GetParent(path).Parent.Parent.FullName, fileName);
                    xmlEditor.CreateXMLFile(fileName);

                    Console.WriteLine("\n\n\n{0,30}{1}\n{2,30}{3}", "Generated XML File Name  : ", fileShortName, "File Location  : ", Directory.GetParent(path).Parent.FullName);
                    if (DateTime.Now.Hour > 6 && DateTime.Now.Hour < 16)
                        Console.WriteLine("\n\n............Have a nice day.............");
                    else
                        Console.WriteLine("\n\n............Have a good night.............");
                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }

            // Display relationship if '/r' is passed in the option parameter 
            if (options.Any(option => option.ToLower() == "r"))
            {
                Console.Write("\n\n************************  FINAL ANALYSIS   ************************\n");
                HandleUsingRelationType(rep);
                HandleDisplayBasedOnRelationships(rep);
            }

            //display analysis of sudirectories
            else if (options.Any(option => option.ToLower() == "s"))
            {
                Console.Write("\n\n************************  FINAL ANALYSIS   ************************\n");
                DisplayWithoutShowingRelationship(rep);
            }
        }

        /// <summary>
        /// This function will display types, functions along with their complexities
        /// </summary>
        /// <param name="rep"></param>
        private void DisplayWithoutShowingRelationship(Repository rep)
        {
            string oldFileName = string.Empty;
            bool isInsideClass = false;
            if (rep.locations.Count > 0)
                foreach (var entry in rep.locations)
                {
                    if (oldFileName != entry.filePath)
                    {
                        oldFileName = entry.filePath;
                        Console.WriteLine("\n\n\n**************************************************************\nDisplaying analysed output for file {0}..............\n**************************************************************", entry.filePath);
                    }

                    if (entry.type == "function")
                        Console.WriteLine("\n \t\tFunction {0,-20} : Complexity = {1,3}   Size = {2}", entry.name, entry.complexity, entry.end - entry.begin);

                    else if (entry.type == "namespace")
                    {
                        isInsideClass = false;
                        Console.WriteLine("\n Namespace {0}", entry.name);
                    }

                    else if (entry.type == "class")
                    {
                        isInsideClass = true;
                        Console.WriteLine("\n\t Class {0}", entry.name);
                    }

                    else if (isInsideClass == true)
                    {
                        DisplayDelegateAndInterfaceName(entry);
                    }
                    else
                    {
                        DisplayNameWithPaddingNextToNamespace(entry);
                    }
                }
        }

        /// <summary>
        /// This method will display names from the entry in appropriate spacing
        /// </summary>
        /// <param name="entry"></param>
        private static void DisplayNameWithPaddingNextToNamespace(Elem entry)
        {
            if (entry.type == "delegate")
                Console.WriteLine("\n\t Delegate {0,-20}", entry.name);

            else if (entry.type == "interface")
                Console.WriteLine("\n\t Interface {0,-20}", entry.name);

            else if (entry.type == "struct")
                Console.WriteLine("\n\t Struct {0,-20}", entry.name);

            else if (entry.type == "enum")
                Console.WriteLine("\n\t Enum {0,-20}", entry.name);
        }

        /// <summary>
        /// This method will display the declarations of delegate, interface, struct and Enum
        /// </summary>
        /// <param name="entry"></param>
        private static void DisplayDelegateAndInterfaceName(Elem entry)
        {
            if (entry.type == "delegate")
                Console.WriteLine("\n\t\t Delegate {0,-20}", entry.name);

            else if (entry.type == "interface")
                Console.WriteLine("\n\t\t Interface {0,-20}", entry.name);

            else if (entry.type == "struct")
                Console.WriteLine("\n\t\t Struct {0,-20}", entry.name);

            else if (entry.type == "enum")
                Console.WriteLine("\n\t\t Enum {0,-20}", entry.name);
        }

        /// <summary>
        /// This method will identify the user defined class types from the semiexpression list we have in the repository 
        /// and delete those entries which are not of user defined types
        /// </summary>
        private void RemoveUnnecessaryAggregation(Repository rep)
        {
            rep.locations.Where(entry => entry.type == "class" && entry.relationShips.Count > 0).ToList().ForEach(delegate(Elem ele)
            {
                int index = -1;
                List<int> indexesTobeDeleted = new List<int>();
                ele.relationShips.RemoveAll(relationShip =>
                    {
                        index++;
                        if (relationShip == RelationShip.Aggregation && !rep.ValueTypeCollection.ContainsKey(ele.relationshipExpressions[index].Split(' ').Last()))
                        {
                            indexesTobeDeleted.Add(index);
                            return true;
                        }
                        return false;
                    });

                if (indexesTobeDeleted.Count > 0)
                {
                    index = 0;
                    indexesTobeDeleted.ForEach(indexedItem =>
                        {
                            ele.relationshipExpressions.RemoveAt(indexedItem - index);
                            index++;
                        });
                }
            });
        }

        /// <summary>
        /// In case there is a "Using" relationship this method will check if the new InstanceType is user defined reference type or system defined reference type 
        /// </summary>
        /// <param name="rep"></param>
        private static void HandleUsingRelationType(Repository rep)
        {
            bool isValueOrReferenceType;
            bool isReferenceType;
            bool isValueType;
            string oldFileName = string.Empty;

            if (rep.locations.Count > 0)
                foreach (var entry in rep.locations)
                {
                    int index = 0;

                    if ((entry.type == "class") && !string.IsNullOrEmpty(entry.listOfParameters))
                    {
                        entry.listOfParameters.Split(' ').ToList<string>().ForEach(delegate(string parameter)
                        {
                            if (rep.ValueTypeCollection.ContainsKey(parameter))
                            {
                                if (!rep.ValueTypeCollection[parameter])
                                {
                                    entry.relationShips.Add(RelationShip.Using);
                                    entry.relationshipExpressions.Add(string.Format("{0} uses {1}", entry.name, parameter));
                                }
                            }
                        });

                        Console.Write("\n Class {0} :\n ", entry.name);
                        entry.relationShips.ForEach(delegate(RelationShip relationshipEntry)
                        {
                            isValueOrReferenceType = isReferenceType = isValueType = false;
                            var correspondingSemiexpression = entry.relationshipExpressions[index];
                            var correspondingRelationshipParent = correspondingSemiexpression.Split(' ').First();
                            var correspondingRelationshipChild = entry.relationshipExpressions[index].Split(' ').Last();
                            string composingType = correspondingSemiexpression.Split(' ')[2];
                            isValueOrReferenceType = rep.ValueTypeCollection.ContainsKey(correspondingRelationshipChild);

                            if (isValueOrReferenceType)
                            {
                                isReferenceType = !rep.ValueTypeCollection[correspondingRelationshipChild];
                                isValueType = !isReferenceType;
                            }

                            DisplayBasedOnTheRelationship(composingType, isReferenceType, relationshipEntry, correspondingRelationshipParent, correspondingRelationshipChild, false);

                            index++;
                        });
                    }
                }
        }

        /// <summary>
        /// This method will display appropriate expression on the console screen screen based on the relationship identified in the repository
        /// </summary>
        /// <param name="rep"></param>
        private static void HandleDisplayBasedOnRelationships(Repository rep)
        {
            string oldFileName = string.Empty;
            if (rep.locations.Count > 0)
                foreach (var entry in rep.locations)
                {

                    if (oldFileName != entry.filePath)
                    {
                        oldFileName = entry.filePath;
                        Console.WriteLine("\n\n\n**************************************************************\nDisplaying analysed output for file {0}..............\n**************************************************************", entry.filePath);
                    }
                    int index = 0;

                    if (entry.relationShips.Count > 0 && (entry.type == "class"))
                    {
                        index = ExtractParentChildWithRelationship(rep, entry, index);
                    }
                    if (entry.relationShips.Count == 0 && (entry.type == "class"))
                    {
                        Console.WriteLine("\n----------------------------------------------------------------------\n");
                        Console.Write("{0,10} {1,-10} {2,-20}\n ", "Class", entry.name, ": NO RELATIONSHIPS IDENTIFIED");
                    }
                    else
                        HandleDisplayForOtherTypes(entry);
                }
        }

        /// <summary>
        /// This method will check the stored semi expression and display the relationship
        /// based on the Relationship enum value 
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="entry"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static int ExtractParentChildWithRelationship(Repository rep, Elem entry, int index)
        {
            bool isValueOrReferenceType;
            bool isReferenceType;
            bool isValueType;
            Console.WriteLine("\n----------------------------------------------------------------------\n");
            Console.Write("{0,10} {1,-10} {2,-20}\n ", "Class", entry.name, ": RELATIONSHIPS IDENTIFIED");
            entry.relationShips.ForEach(delegate(RelationShip relationshipEntry)
            {
                isValueOrReferenceType = isReferenceType = isValueType = false;
                var correspondingSemiexpression = entry.relationshipExpressions[index];
                var correspondingRelationshipParent = correspondingSemiexpression.Split(' ').First();
                var correspondingRelationshipChild = entry.relationshipExpressions[index].Split(' ').Last();
                isValueOrReferenceType = rep.ValueTypeCollection.ContainsKey(correspondingRelationshipChild);
                string composingType = entry.relationshipExpressions[index].Split(' ')[2];
                if (isValueOrReferenceType)
                {
                    isReferenceType = !rep.ValueTypeCollection[correspondingRelationshipChild];
                    isValueType = !isReferenceType;
                }
                bool isDelegateType = rep.DelegateInstanceCollection.Any(delegateEntry => correspondingRelationshipChild == delegateEntry.name);
                DisplayBasedOnTheRelationship(composingType, isReferenceType, relationshipEntry, correspondingRelationshipParent, correspondingRelationshipChild, isDelegateType);

                index++;
            });
            return index;
        }

        /// <summary>
        /// This method will display types on the console screen for the remaining types 
        /// </summary>
        /// <param name="entry"></param>
        private static void HandleDisplayForOtherTypes(Elem entry)
        {
            if (entry.type == "function")
                Console.WriteLine("\n \tFunction {0,-20} : Complexity = {1,3}   Size = {2}", entry.name, entry.complexity, entry.end - entry.begin);

            else if (entry.type == "namespace")
                Console.WriteLine("\n Namespace {0}", entry.name);

            else if (entry.type == "delegate")
                Console.WriteLine("\n Delegate {0}", entry.name);

            else if (entry.type == "interface")
                Console.WriteLine("\n Interface {0}", entry.name);

            else if (entry.type == "struct")
                Console.WriteLine("\n\t Structure {0}", entry.name);

            else if (entry.type == "enum")
                Console.WriteLine("\n\t Enum {0}", entry.name);
        }

        /// <summary>
        /// This method will display relationships obtained in appropriate format on the console screen
        /// </summary>
        /// <param name="composingStatement"></param>
        /// <param name="isReferenceType"></param>
        /// <param name="relationshipEntry"></param>
        /// <param name="correspondingRelationshipParent"></param>
        /// <param name="correspondingRelationshipChild"></param>
        private static void DisplayBasedOnTheRelationship(string composingStatement, bool isReferenceType, RelationShip relationshipEntry, string correspondingRelationshipParent, string correspondingRelationshipChild, bool isDelegateType)
        {
            switch (relationshipEntry)
            {
                case RelationShip.Aggregation:
                    {
                        if (isDelegateType)
                            Console.WriteLine("\t\t\tClass {0} is a subscriber to Delegate instance named {1}", correspondingRelationshipParent, correspondingRelationshipChild);
                        if (isReferenceType)
                            Console.WriteLine("\t\t\t{0} aggregates {1}", correspondingRelationshipParent, correspondingRelationshipChild);
                        break;
                    }

                case RelationShip.Composition:
                    Console.WriteLine("\t\t\t{0} composes {1} {2}", correspondingRelationshipParent, composingStatement, correspondingRelationshipChild);
                    break;

                case RelationShip.Inheritence:
                    Console.WriteLine("\t\t\t{0} inherits from {1}", correspondingRelationshipParent, correspondingRelationshipChild);
                    break;

                case RelationShip.Using:
                    Console.WriteLine("\t\t\t{0} uses {1}", correspondingRelationshipParent, correspondingRelationshipChild);
                    break;
            }
        }


#if(TEST_DISPLAY)
        //----< test stub >--------------------------------------------------

        [STAThread]
        static void Main(string[] args)
        {
            Console.Write("\n  Testing Display Operations");
            Console.Write("\n ============================\n");
            string testCommandArguments = string.Concat("TestFiles" + " *.cs* ", " /s ");

            Display testDisplay = new Display();
            List<string> options = new List<string>();
            options.Add("/s");
            options.Add("/r");
            options.Add("/x");
            Repository testRepository = new Repository();
            testRepository.locations.Add(
                new Elem(){
                    name = "TestClassA", 
                    type = "Class", 
                    begin = 0, 
                    filePath = "testCSFile.cs",
                    relationshipExpressions = new List<string>(){"TestClassA inherits TestClasB"}
                });

                testRepository.locations.Add(new Elem(){
                    name = "TestClassB", 
                    type = "class", 
                    begin = 0, 
                    filePath = "testCSFile.cs",
                    relationshipExpressions = new List<string>() { "TestClassB aggregates TestClasC" }
                });

                testRepository.locations.Add(new Elem()
                {
                    name = "TestFunctionA",
                    type = "function",
                    begin = 45,
                    end = 67,
                    filePath = "testCSFile.cs",
                    relationshipExpressions = new List<string>() { "TestClassB aggregates TestClasC" }
                });

            testDisplay.DisplayContent(options);

            Console.Write("\n");
        }
#endif

    }
}
