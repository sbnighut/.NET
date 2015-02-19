//////////////////////////////////////////////////////////////////////
// XDocument-Create-XML.cs - demonstrate creating an XML file       //
//                           using System.Xml.Linq.XDocument        //
//                                                                  //
//Author        :       Swapnil Nighut, Syracuse University,        //
//                      315-751-3149, snighut@syr.edu               //
//Source        :       Jim Fawcett                                 //
//////////////////////////////////////////////////////////////////////
/*
 * Module Operations
 * =================
 * After completion of parse 2, this file creates an XML looping through all the records
 * present in the location table inside Repository
 * 
 * Based upon whether the next entry from the location table is inside a class or after a class 
 * parent-child relationship will be set in the xml
 * 
 * GenerateXML() is the method which creates the xml file
 * 
 * CreateXMLFile() will create a physical file for the generated xml 3 levels above the current directory 
 */


using CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace XDocument_Create_XML
{
    /// <summary>
    /// This class is used to perform XML operations in order to generate an XML 
    /// file which will hold all the related info pertaining to contents inside the file such as 
    /// types defined, complexities, filename along with some attributes, if applicable
    /// </summary>
    public class XMLEditor
    {

        XDocument xml;
        public XDocument Xml
        {
            get { return xml; }
            set { xml = value; }
        }

        /// <summary>
        /// This method will generate the XML based upon the values present in the repository
        /// </summary>
        /// <returns></returns>
        public bool GenerateXML()
        {
            Repository rep = Repository.getInstance();
            Console.Write("\n======================================================\n    Generating XML file using XDocument\n======================================================\n");
            bool isInsideClass = false;
            bool isLastParentAClass = false;
            xml = new XDocument();
            xml.Declaration = new XDeclaration("1.0", "utf-8", "yes");
            /*
             *  It is a quirk of the XDocument class that the XML declaration,
             *  a valid processing instruction element, cannot be added to the
             *  XDocument's element collection.  Instead, it must be assigned
             *  to the document's Declaration property.
             */
            XComment comment = new XComment("SMA Project 2");
            xml.Add(comment);

            XElement root = new XElement("Code_Analyser");
            xml.Add(root);
            string oldFileName = string.Empty;
            rep.locations.ForEach(delegate(Elem record)
            {
                if (oldFileName != record.filePath)
                {
                    oldFileName = record.filePath;
                    XElement tempFileNameNode = new XElement("File");
                    tempFileNameNode.SetAttributeValue("Name", oldFileName.Split('\\').Last());
                    root.Add(tempFileNameNode);
                }

                XElement tempNamespaceNode = new XElement("Namespace");
                XElement tempFunctionNode = new XElement("Function");
                XElement tempClassNode = new XElement("Class");
                XElement tempInterfaceNode = new XElement("Interface");
                XElement tempStructNode = new XElement("Struct");
                XElement tempDelegateNode = new XElement("Delegate");
                XElement tempOtherTypeNode = new XElement("OtherType");

                if (record.type == "namespace")
                {
                    isInsideClass = false;
                    tempNamespaceNode.SetAttributeValue("Name", record.name);
                    root.Elements().Last().Add(tempNamespaceNode);
                }
                else
                    HandleOtherTypes(root, record, tempInterfaceNode, tempStructNode, tempDelegateNode, tempOtherTypeNode, tempClassNode, tempFunctionNode, ref isInsideClass, ref isLastParentAClass);
            });

            return true;
        }


        /// <summary>
        /// This method will insert node to the XML tree for types other name namespace
        /// </summary>
        /// <param name="root"></param>
        /// <param name="record"></param>
        /// <param name="tempInterfaceNode"></param>
        /// <param name="tempStructNode"></param>
        /// <param name="tempDelegateNode"></param>
        /// <param name="tempOtherTypeNode"></param>
        /// <param name="tempClassNode"></param>
        /// <param name="tempFunctionNode"></param>
        /// <param name="isElementInsideClass"></param>
        /// <param name="isLastParentAClass"></param>
        /// <returns></returns>
        private static bool HandleOtherTypes(XElement root, Elem record, XElement tempInterfaceNode, XElement tempStructNode, XElement tempDelegateNode, XElement tempOtherTypeNode, XElement tempClassNode, XElement tempFunctionNode, ref bool isElementInsideClass, ref bool isLastParentAClass)
        {
            if (record.type == "class")
                InsertClassNode(root, record, tempClassNode, ref isElementInsideClass, ref isLastParentAClass);
            else if (record.type == "function")
                InsertFunctionNode(root, record, ref tempFunctionNode, ref isLastParentAClass);
            else if (record.type == "interface")
            {
                tempInterfaceNode.SetAttributeValue("Name", record.name);
                if (isElementInsideClass)
                {
                    root.Elements().Last().Elements().Last().Elements().Last().Add(tempInterfaceNode);
                    isLastParentAClass = false;
                    return true;
                }
                isLastParentAClass = InsertNodeAdjacentToClassNode(root, tempInterfaceNode, isLastParentAClass);
            }
            else if (record.type == "struct")
            {
                tempStructNode.SetAttributeValue("Name", record.name);
                if (isElementInsideClass)
                {
                    root.Elements().Last().Elements().Last().Elements().Last().Add(tempStructNode);
                    return true;
                }
                isLastParentAClass = InsertNodeAdjacentToClassNode(root, tempStructNode, isLastParentAClass);
            }
            else if (record.type == "delegate")
            {
                tempDelegateNode.Value = record.name.Split(' ').Last();
                tempDelegateNode.SetAttributeValue("Return_Type", record.name.Split(' ').First());
                if (isElementInsideClass)
                {
                    root.Elements().Last().Elements().Last().Elements().Last().Add(tempDelegateNode);
                    return true;
                }
                isLastParentAClass = InsertNodeAdjacentToClassNode(root, tempDelegateNode, isLastParentAClass);
            }
            else
            {
                tempOtherTypeNode.Name = record.type;
                tempOtherTypeNode.Value = record.name;
                if (isElementInsideClass)
                {
                    root.Elements().Last().Elements().Last().Elements().Last().Add(tempOtherTypeNode);
                    return true;
                }
                isLastParentAClass = InsertNodeAdjacentToClassNode(root, tempOtherTypeNode, isLastParentAClass);
            }
            return true;
        }

        /// <summary>
        /// This method will add the a node for interface, delegates, enums and structs based on whether ii is
        /// defined outside class or inside the class
        /// </summary>
        /// <param name="root"></param>
        /// <param name="tempInterfaceNode"></param>
        /// <param name="isLastParentAClass"></param>
        /// <returns></returns>
        private static bool InsertNodeAdjacentToClassNode(XElement root, XElement tempInterfaceNode, bool isLastParentAClass)
        {
            isLastParentAClass = false;
            root.Elements().Last().Elements().Last().Add(tempInterfaceNode);
            return isLastParentAClass;
        }

        /// <summary>
        /// This method will add a XML node of Tag - Function
        /// It will also set the Attribute for function such as its complexity
        /// </summary>
        /// <param name="root"></param>
        /// <param name="record"></param>
        /// <param name="tempFunctionNode"></param>
        /// <param name="isLastParentAClass"></param>
        private static void InsertFunctionNode(XElement root, Elem record, ref XElement tempFunctionNode, ref bool isLastParentAClass)
        {
            tempFunctionNode = new XElement("Function", record.name);
            tempFunctionNode.SetAttributeValue("Complexity", record.complexity);
            root.Elements().Last().Elements().Last().Elements().Last().Add(tempFunctionNode);
            isLastParentAClass = false;
        }

        /// <summary>
        /// This method will add a class node in the xml
        /// </summary>
        /// <param name="root"></param>
        /// <param name="record"></param>
        /// <param name="tempClassNode"></param>
        /// <param name="isElementInsideClass"></param>
        /// <param name="isLastParentAClass"></param>
        private static void InsertClassNode(XElement root, Elem record, XElement tempClassNode, ref bool isElementInsideClass, ref bool isLastParentAClass)
        {
            isLastParentAClass = true;
            isElementInsideClass = true;
            tempClassNode.Name = "Class";
            tempClassNode.SetAttributeValue("Name", record.name);
            root.Elements().Last().Elements().Last().Add(tempClassNode);
            int index = 0;
            if (record.relationshipExpressions.Count > 0)
            {
                record.relationShips.ForEach(delegate(RelationShip relationShipType)
                {
                    root.Elements().Last().Elements().Last().Elements().Last().Add(new XElement("Relationship", record.relationshipExpressions[index]));
                    index++;
                });
            }
        }


        /// <summary>
        /// This method is used to save the xml is a directory
        /// filename conatains the actual path + name where it will be stored
        /// </summary>
        /// <param name="fileName"></param>
        public void CreateXMLFile(string fileName)
        {
            xml.Save(fileName);
            Console.WriteLine(xml.ToString());
        }




#if(TEST_SEMI)

        //----< test stub >--------------------------------------------------

        [STAThread]
        static void Main(string[] args)
        {
            Console.Write("\n  Testing XML Operations");
            Console.Write("\n ============================\n");
            string testCommandArguments = string.Concat("TestFiles" + " *.cs* ", " /s ");

            XMLEditor testXMLEditor = new XMLEditor();
            List<string> options = new List<string>();
            options.Add("/x");
            Repository testRepository = new Repository();

            testRepository.locations.Add(
                new Elem()
                {
                    name = "testNamespaceA",
                    type = "namespace",
                    begin = 0,
                    filePath = "testCSFile1.cs",
                    relationshipExpressions = new List<string>() { "TestClassA inherits TestClasB" }
                });

            testRepository.locations.Add(
                new Elem()
                {
                    name = "TestClassA",
                    type = "Class",
                    begin = 0,
                    filePath = "testCSFile1.cs",
                    relationshipExpressions = new List<string>() { "TestClassA inherits TestClasB" }
                });

            testRepository.locations.Add(new Elem()
            {
                name = "TestfunctionA",
                type = "function",
                begin = 0,
                filePath = "testCSFile1.cs",
                relationshipExpressions = new List<string>() { "TestClassA inherits TestClasB" }
            });

            testRepository.locations.Add(new Elem()
            {
                name = "TestNamespaceB",
                type = "namespace",
                begin = 0,
                filePath = "testCSFile2.cs",
                relationshipExpressions = new List<string>() { "TestClassA inherits TestClasB" }
            });

            testRepository.locations.Add(new Elem()
            {
                name = "TestClassB",
                type = "class",
                begin = 0,
                filePath = "testCSFile2.cs",
                relationshipExpressions = new List<string>() { "TestClassB aggregates TestClasC" }
            });

            testRepository.locations.Add(new Elem()
            {
                name = "TestFunctionA",
                type = "function",
                begin = 45,
                end = 67,
                filePath = "testCSFile2.cs",
                relationshipExpressions = new List<string>() { "TestClassB aggregates TestClasC" }
            });

            testXMLEditor.GenerateXML();
            Console.Write("\n\n{0}\n\n",testXMLEditor.Xml.ToString());
            Console.Write("\n");
        }
#endif
    }
}
