﻿///////////////////////////////////////////////////////////////////////
// RulesAndActions.cs - Parser rules specific to an application      //
// ver 2.1                                                           //
// Language:    C#, 2008, .Net Framework 4.0                         //
// Platform:    Dell Precision T7400, Win7, SP1                      //
// Application: Demonstration for CSE681, Project #2, Fall 2011      //
// Author:      Jim Fawcett, CST 4-187, Syracuse University          //
//              (315) 443-3948, jfawcett@twcny.rr.com                //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * RulesAndActions package contains all of the Application specific
 * code required for most analysis tools.
 *
 * It defines the following Four rules which each have a
 * grammar construct detector and also a collection of IActions:
 *   - DetectNameSpace rule
 *   - DetectClass rule
 *   - DetectFunction rule
 *   - DetectScopeChange
 *   
 *   Three actions - some are specific to a parent rule:
 *   - Print
 *   - PrintFunction
 *   - PrintScope
 * 
 * The package also defines a Repository class for passing data between
 * actions and uses the services of a ScopeStack, defined in a package
 * of that name.
 *
 * Note:
 * This package does not have a test stub since it cannot execute
 * without requests from Parser.
 *  
 */
/* Required Files:
 *   IRuleAndAction.cs, RulesAndActions.cs, Parser.cs, ScopeStack.cs,
 *   Semi.cs, Toker.cs
 *   
 * Build command:
 *   csc /D:TEST_PARSER Parser.cs IRuleAndAction.cs RulesAndActions.cs \
 *                      ScopeStack.cs Semi.cs Toker.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 2.2 : 24 Sep 2011
 * - modified Semi package to extract compile directives (statements with #)
 *   as semiExpressions
 * - strengthened and simplified DetectFunction
 * - the previous changes fixed a bug, reported by Yu-Chi Jen, resulting in
 * - failure to properly handle a couple of special cases in DetectFunction
 * - fixed bug in PopStack, reported by Weimin Huang, that resulted in
 *   overloaded functions all being reported as ending on the same line
 * - fixed bug in isSpecialToken, in the DetectFunction class, found and
 *   solved by Zuowei Yuan, by adding "using" to the special tokens list.
 * - There is a remaining bug in Toker caused by using the @ just before
 *   quotes to allow using \ as characters so they are not interpreted as
 *   escape sequences.  You will have to avoid using this construct, e.g.,
 *   use "\\xyz" instead of @"\xyz".  Too many changes and subsequent testing
 *   are required to fix this immediately.
 * ver 2.1 : 13 Sep 2011
 * - made BuildCodeAnalyzer a public class
 * ver 2.0 : 05 Sep 2011
 * - removed old stack and added scope stack
 * - added Repository class that allows actions to save and 
 *   retrieve application specific data
 * - added rules and actions specific to Project #2, Fall 2010
 * ver 1.1 : 05 Sep 11
 * - added Repository and references to ScopeStack
 * - revised actions
 * - thought about added folding rules
 * ver 1.0 : 28 Aug 2011
 * - first release
 *
 * Planned Modifications (not needed for Project #2):
 * --------------------------------------------------
 * - add folding rules:
 *   - CSemiExp returns for(int i=0; i<len; ++i) { as three semi-expressions, e.g.:
 *       for(int i=0;
 *       i<len;
 *       ++i) {
 *     The first folding rule folds these three semi-expression into one,
 *     passed to parser. 
 *   - CToker returns operator[]( as four distinct tokens, e.g.: operator, [, ], (.
 *     The second folding rule coalesces the first three into one token so we get:
 *     operator[], ( 
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CSsemi;
using System.Linq;

namespace CodeAnalysis
{
    // holds scope information
    public class Elem
    {
        public string type { get; set; }
        public string name { get; set; }
        public int begin { get; set; }
        public int end { get; set; }
        public int complexity { get; set; }
        public List<string> relationshipExpressions { get; set; }
        public List<RelationShip> relationShips { get; set; }
        public string filePath { get; set; }
        public string listOfParameters { get; set; }
        public Elem()
        {
            relationShips = new List<RelationShip>();
            relationshipExpressions = new List<string>();

        }
        public override string ToString()
        {
            StringBuilder temp = new StringBuilder();
            temp.Append("{");
            temp.Append(String.Format("{0,-10}", type)).Append(" : ");
            temp.Append(String.Format("{0,-10}", name)).Append(" : ");
            temp.Append(String.Format("{0,-5}", begin.ToString()));             // line of scope start
            temp.Append(String.Format("{0,-5}", end.ToString()));               // line of scope end
            temp.Append(String.Format("{0,-5}", (end - begin).ToString()));     // size of the function
            temp.Append(String.Format("{0,-5}", complexity.ToString()));        // complexity
            temp.Append("}");
            return temp.ToString();
        }
    }

    public class Repository
    {
        string currentFile = string.Empty;

        ScopeStack<Elem> stack_ = new ScopeStack<Elem>();
        List<Elem> locations_ = new List<Elem>();
        static Repository instance;

        public List<Elem> DelegateInstanceCollection
        {
            get;
            set;
        }
        public string CurrentFile
        {
            get { return currentFile; }
            set { currentFile = value; }
        }

        public Dictionary<string, bool> ValueTypeCollection
        {
            get;
            set;
        }

        public Repository()
        {
            instance = this;
        }

        public static Repository getInstance()
        {
            return instance;
        }


        // provides all actions access to current semiExp
        public CSemiExp semi
        {
            get;
            set;
        }

        // semi gets line count from toker who counts lines
        // while reading from its source
        public int lineCount  // saved by newline rule's action
        {
            get { return semi.lineCount; }
        }

        public int prevLineCount  // not used in this demo
        {
            get;
            set;
        }
        // enables recursively tracking entry and exit from scopes

        public ScopeStack<Elem> stack  // pushed and popped by scope rule's action
        {
            get { return stack_; }
        }
        // the locations table is the result returned by parser's actions
        // in this demo

        public List<Elem> locations
        {
            get { return locations_; }
        }
    }

    /////////////////////////////////////////////////////////
    // pushes scope info on stack when entering new scope
    public class PushStack : AAction
    {
        public Repository repo_;

        public PushStack(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(CSemiExp semi)
        {
            Elem elem = new Elem();
            elem.type = semi[0];  // expects type
            elem.name = semi[1];  // expects name
            elem.begin = repo_.semi.lineCount - 1;
            elem.end = 0;
            elem.filePath = repo_.CurrentFile;

            if (!IsCompositionDetected(repo_, elem))
            {
                if (!IsAggregationDetected(repo_, elem, semi.count == 3 ? semi[2] : string.Empty))
                {
                    if (elem.type != "BracesAbsent" && elem.type != "delegate")
                        repo_.stack.push(elem);

                    if (elem.type == "delegate")
                    {
                        if (!repo_.DelegateInstanceCollection.Any(entry => entry.name == semi[2]))
                        {
                            repo_.DelegateInstanceCollection.Add(new Elem() { type = elem.type, name = semi[2] });
                        }
                    }

                    //identify the complexity of the function by checking with the existing elements in the stack with type as control & name as anonymous
                    if (elem.type == "control" || elem.type == "BracesAbsent")
                    {
                        SetComplexityOfFunction();
                        return;
                    }

                    if (elem.type == "function")
                    {
                        var lastClassInserted = repo_.locations.Last(record => record.type == "class");
                        lastClassInserted.listOfParameters = string.Concat(lastClassInserted.listOfParameters, semi.FunctionArguments);
                    }

                    if (elem.type == "delegate")
                    {
                        elem.name = string.Format("{0} {1}", elem.name, semi[2]);
                    }
                    repo_.locations.Add(elem);
                    CheckAndAddNewReferenceTypes(elem);
                    DetectInheritence(semi);
                }
            }
        }

        /// <summary>
        /// This method will check whether the derived class or normal class is already there in the dictionary or not.
        /// If its not there then adds it into the dictionary of Type <Typename,Boolean>
        /// 
        /// Note: This will ensure that the dictionary always contains unique names for classes
        /// </summary>
        /// <param name="elem"></param>
        private void CheckAndAddNewReferenceTypes(Elem elem)
        {
            // if new reference type i.e any class is deteected add it in the dictionary
            if (!repo_.ValueTypeCollection.ContainsKey(elem.name) && elem.type == "class")
            {
                repo_.ValueTypeCollection.Add(elem.name, false);
            }
        }

        /// <summary>
        /// This method sets the complexity of the function as we encounter during the parsing
        /// </summary>
        private void SetComplexityOfFunction()
        {
            for (int i = repo_.locations.Count - 1; i >= 0; --i)
            {
                Elem temp = repo_.locations[i];
                if (temp.type == "function")
                {
                    (repo_.locations[i]).complexity++;
                    break;
                }
            }
        }

        /// <summary>
        /// This method will check for the aggregation relationship. It will loop the aggregated type through
        /// the list of classes we have obtained in Parse 1. If the type is present in the defined class's list 
        /// then aggregation relationship will be added in the record inside the repository corresponding to that class
        /// </summary>
        /// <param name="repo_"></param>
        /// <param name="elem"></param>
        /// <returns></returns>
        private bool IsAggregationDetected(Repository repo_, Elem elem, string delegateName)
        {
            if (elem.type == "aggregation")
            {
                var entry = repo_.locations.Last(recordEntry => recordEntry.type == "class");
                entry.relationShips.Add(RelationShip.Aggregation);
                entry.relationshipExpressions.Add(string.Format("{0} aggregates {1}", entry.name, elem.name));
                return true;
            }

            return false;
        }

        /// <summary>
        /// This method will check whether there is any word in the semi-expression which is of value type like struct, enum , int etc
        /// 
        /// Note: We are storing a dictionary of all the 16 value types in the repository
        /// </summary>
        /// <param name="repo_"></param>
        /// <param name="elem"></param>
        /// <returns></returns>
        private bool IsCompositionDetected(Repository repo_, Elem elem)
        {
            //Check for composition type
            if (repo_.ValueTypeCollection.ContainsKey(elem.type))
            {
                if (repo_.ValueTypeCollection[elem.type])
                {
                    if (repo_.stack.count > 0 && repo_.stack[repo_.stack.count - 1].type == "class")
                    {
                        var entry = repo_.locations.Last(recordEntry => recordEntry.type == "class");
                        entry.relationShips.Add(RelationShip.Composition);
                        entry.relationshipExpressions.Add(string.Format("{0} composes {1} {2}", entry.name, elem.type, elem.name));
                    }
                    else if (elem.type == "enum" && repo_.stack[repo_.stack.count - 1].type != "function" && repo_.stack[repo_.stack.count - 1].type != "delegate")
                    {
                        repo_.stack.push(elem);
                        //In case of enum or any other value type declared inside namespace but outside class table will be updated
                        repo_.locations.Add(elem);
                    }
                    if (elem.type == "struct")
                    {
                        repo_.stack.push(elem);
                        //In case of enum or any other value type declared inside namespace but outside class table will be updated
                        repo_.locations.Add(elem);
                    }
                    return true;
                }

            }
            return false;
        }

        /// <summary>
        /// This method will check for the semicolon in the class expression and in case if its detected it will
        /// insert a new relationship in the Class record from the repository table
        /// </summary>
        /// <param name="semi"></param>
        private void DetectInheritence(CSemiExp semi)
        {
            //Inheritence detection
            int indexCL = semi.CompleteExpression.IndexOf("class");
            int indexSemiColon = semi.CompleteExpression.IndexOf(":");
            if ((indexCL != -1) && (indexSemiColon != -1) && (indexCL == (indexSemiColon - 2)))
            {
                string baseClass = semi.CompleteExpression[indexSemiColon + 1];
                string derivedClass = semi.CompleteExpression[indexSemiColon - 1];
                repo_.locations[repo_.locations.Count - 1].relationshipExpressions.Add(string.Format("{0} inherits from {1}", derivedClass, baseClass));
                repo_.locations[repo_.locations.Count - 1].relationShips.Add(RelationShip.Inheritence);
            }
        }
    }

    /////////////////////////////////////////////////////////
    // pops scope info from stack when leaving scope
    public class PopStack : AAction
    {
        Repository repo_;

        public PopStack(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(CSemiExp semi)
        {
            Elem elem;
            try
            {
                elem = repo_.stack.pop();
                for (int i = 0; i < repo_.locations.Count; ++i)
                {
                    Elem temp = repo_.locations[i];
                    if (elem.type == temp.type)
                    {
                        if (elem.name == temp.name)
                        {
                            if ((repo_.locations[i]).end == 0)
                            {
                                (repo_.locations[i]).end = repo_.semi.lineCount;
                                break;
                            }
                        }
                    }
                }
            }
            catch
            {
                Console.Write("popped empty stack on semiExp: ");
                semi.display();
                return;
            }
            CSemiExp local = new CSemiExp();
            local.Add(elem.type).Add(elem.name);
            if (local[0] == "control")
                return;

            if (AAction.displaySemi)
            {
                //Console.Write("\n  line# {0,-5}", repo_.semi.lineCount);
                //Console.Write("leaving  ");
                //string indent = new string(' ', 2 * (repo_.stack.count + 1));
                //Console.Write("{0}", indent);
                //this.display(local); // defined in abstract action
            }
        }
    }

    ///////////////////////////////////////////////////////////
    // action to print function signatures - not used in demo
    public class PrintFunction : AAction
    {
        Repository repo_;

        public PrintFunction(Repository repo)
        {
            repo_ = repo;
        }
        public override void display(CSemiExp semi)
        {
            Console.Write("\n    line# {0}", repo_.semi.lineCount - 1);
            Console.Write("\n    ");
            for (int i = 0; i < semi.count; ++i)
                if (semi[i] != "\n" && !semi.isComment(semi[i]))
                    Console.Write("{0} ", semi[i]);
        }
        public override void doAction(CSemiExp semi)
        {
            this.display(semi);
        }
    }

    /////////////////////////////////////////////////////////
    // concrete printing action, useful for debugging
    public class Print : AAction
    {
        Repository repo_;

        public Print(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(CSemiExp semi)
        {
            Console.Write("\n  line# {0}", repo_.semi.lineCount - 1);
            this.display(semi);
        }
    }

    /////////////////////////////////////////////////////////
    // rule to detect namespace declarations
    public class DetectNamespace : ARule
    {
        public override bool test(CSemiExp semi)
        {
            int index = semi.Contains("namespace");
            if (index != -1)
            {
                CSemiExp local = new CSemiExp();
                // create local semiExp with tokens for type and name
                local.displayNewLines = false;
                local.Add(semi[index]).Add(semi[index + 1]);
                doActions(local);
                return true;
            }
            return false;
        }
    }

    /////////////////////////////////////////////////////////
    // rule to dectect class definitions
    public class DetectClass : ARule
    {
        public override bool test(CSemiExp semi)
        {
            int indexCL = semi.Contains("class");
            int indexIF = semi.Contains("interface");
            int indexST = semi.Contains("struct");
            int indexEN = semi.Contains("enum");
            int index = Math.Max(indexCL, indexIF);
            index = Math.Max(index, indexST);
            index = Math.Max(index, indexEN);

            if (index != -1)
            {
                CSemiExp local = new CSemiExp();
                // local semiExp with tokens for type and name
                local.displayNewLines = false;
                local.CompleteExpression = semi.SemiExp;
                local.Add(semi[index]).Add(semi[index + 1]);
                doActions(local);
                return true;
            }
            return false;
        }
    }

    /////////////////////////////////////////////////////////
    // rule to dectect function definitions
    public class DetectFunction : ARule
    {
        public static bool isSpecialToken(string token)
        {
            string[] SpecialToken = { "if", "for", "foreach", "while", "catch", "using", "try", "switch" };
            foreach (string stoken in SpecialToken)
                if (stoken == token)
                    return true;
            return false;
        }
        public override bool test(CSemiExp semi)
        {

            if (semi[semi.count - 1] != "{")
                return false;

            int index = semi.FindFirst("(");
            int endIndex = semi.FindFirst(")");
            StringBuilder sb = new StringBuilder();

            if (index > 0 && !isSpecialToken(semi[index - 1]))
            {
                CSemiExp local = new CSemiExp();

                local.CompleteExpression = semi.SemiExp;
                local.Add("function").Add(semi[index - 1]);
                if ((endIndex - index) != 1)
                    for (int i = index; i <= endIndex; i++)
                    {
                        if (semi[i] == "," || semi[i] == "(")
                            sb.Append(semi[i + 1]).Append(" ");
                    }
                local.FunctionArguments = sb.ToString();
                doActions(local);
                return true;
            }
            return false;
        }
    }

    /////////////////////////////////////////////////////////
    // detect entering anonymous scope
    // - expects namespace, class, and function scopes
    //   already handled, so put this rule after those
    public class DetectAnonymousScope : ARule
    {
        public override bool test(CSsemi.CSemiExp semi)
        {
            int index = semi.Contains("{");
            int count = NoBrace(semi);
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                    // create local semiExp with tokens for type and name
                    local.displayNewLines = false;
                    local.Add("BracesAbsent").Add("anonymous");
                    doActions(local);
                }
                return true;
            }
            else
            {
                if (index != -1)
                {
                    CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                    // create local semiExp with tokens for type and name
                    local.displayNewLines = false;
                    local.Add("control").Add("anonymous");
                    doActions(local);
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// This method will check for special keywords which are specific to loops and conditions
        /// that leads to increase in complexity by 1
        /// </summary>
        /// <param name="semi"></param>
        /// <returns></returns>
        public static int containsSpecialToken(CSsemi.CSemiExp semi)
        {
            int count = 0;
            string[] SpecialToken = { "if", "for", "foreach", "while", "catch", "using", "try", "else" };
            foreach (string stoken in SpecialToken)
                if (semi.Contains(stoken) != -1)
                {
                    List<int> locations = semi.Complexity_Occur(stoken);
                    if (locations.Count > 0)
                    {
                        count = count + locations.Count;
                    }
                    else
                        count++;
                }
            return count;
        }

        /// <summary>
        /// This method will store a '{' manually after the special tokens if they are not present already
        /// </summary>
        /// <param name="semi"></param>
        /// <returns></returns>
        public int NoBrace(CSsemi.CSemiExp semi)
        {
            int count_st = containsSpecialToken(semi);
            if (count_st > 0)
                if (semi[semi.count - 1] != "{")
                    return count_st;
            return 0;
        }
    }

    /////////////////////////////////////////////////////////
    // detect leaving scope
    public class DetectLeavingScope : ARule
    {
        public override bool test(CSemiExp semi)
        {
            int index = semi.Contains("}");
            if (index != -1)
            {
                doActions(semi);
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// This method will check for the keywords in the semiexpression with the existing list of all the value types(created in the beginning of type Dictionary<typename,bool>)
    /// if the keyword is present the dictionary then its of composite relationship
    /// </summary>
    public class DetectComposition : ARule
    {
        bool isValueTypePresent;
        public override bool test(CSemiExp semi)
        {
            if (semi.count > 2)
            {
                Repository rep = Repository.getInstance();
                isValueTypePresent = semi.SemiExp.Any(identifier => rep.ValueTypeCollection.ContainsKey(identifier) && rep.ValueTypeCollection[identifier]);

                if (isValueTypePresent)
                {
                    var searchedIndexOfValueTypeKeyword = semi.SemiExp.FindIndex(identifier => rep.ValueTypeCollection.ContainsKey(identifier));
                    CSemiExp local = new CSemiExp();
                    // local semiExp with tokens for type and name
                    local.displayNewLines = false;
                    local.Add(semi[searchedIndexOfValueTypeKeyword]).Add(semi[searchedIndexOfValueTypeKeyword + 1]);
                    doActions(local);
                    return true;
                }
            }
            return false;
        }
    }

    public class DetectAggregation : ARule
    {
        bool isNewKeywordPresent;
        public override bool test(CSemiExp semi)
        {
            if (semi.count > 2)
            {
                Repository rep = Repository.getInstance();
                isNewKeywordPresent = semi.SemiExp.Any(identifier => identifier == "new");
                if (isNewKeywordPresent)
                {
                    var searchedIndexOfNewKeyword = semi.SemiExp.FindIndex(identifier => identifier == "new");
                    CSemiExp local = new CSemiExp();
                    local.CompleteExpression = semi.SemiExp;
                    // local semiExp with tokens for type and name
                    local.displayNewLines = false;

                    local.Add("aggregation").Add(semi[searchedIndexOfNewKeyword + 1]);
                    doActions(local);
                    return false;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Class to identify delegates from the semi expression 
    /// </summary>
    public class DetectDelegates : ARule
    {
        public override bool test(CSemiExp semi)
        {
            int indexDE = semi.Contains("delegate");

            if (indexDE != -1)
            {
                CSemiExp local = new CSemiExp();
                // local semiExp with tokens for type and name
                local.displayNewLines = false;
                local.CompleteExpression = semi.SemiExp;
                local.Add(semi[indexDE]).Add(semi[indexDE + 1]).Add(semi[indexDE + 2]);
                doActions(local);
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// This enum will store corresponding relationships in the table 
    /// </summary>
    public enum RelationShip { Inheritence, Composition, Aggregation, Using }

    public class BuildCodeAnalyzer
    {
        Repository repo = new Repository();
        private CSemiExp semi;

        public BuildCodeAnalyzer(CSemiExp semi)
        {
            repo.semi = semi;
        }

        public virtual Parser build()
        {
            Parser parser = new Parser();

            // decide what to show
            AAction.displaySemi = true;
            AAction.displayStack = false;  // this is default so redundant

            // action used for namespaces, classes, and functions
            PushStack push = new PushStack(repo);

            // capture namespace info
            DetectNamespace detectNS = new DetectNamespace();
            detectNS.add(push);
            parser.add(detectNS);

            // capture class info
            DetectClass detectCl = new DetectClass();
            detectCl.add(push);
            parser.add(detectCl);

            // capture function info
            DetectFunction detectFN = new DetectFunction();
            detectFN.add(push);
            parser.add(detectFN);

            AddOtherDetections(parser, push);

            return parser;
        }

        /// <summary>
        /// This method associates more set of seting classes for rule deteection
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="push"></param>
        private void AddOtherDetections(Parser parser, PushStack push)
        {
            // handle entering anonymous scopes, e.g., if, while, etc.
            DetectAnonymousScope anon = new DetectAnonymousScope();
            anon.add(push);
            parser.add(anon);

            DetectDelegates detectDeleg = new DetectDelegates();
            detectDeleg.add(push);
            parser.add(detectDeleg);

            // handle leaving scopes
            DetectLeavingScope leave = new DetectLeavingScope();
            PopStack pop = new PopStack(repo);
            leave.add(pop);
            parser.add(leave);

            //capture namespace info
            DetectComposition detectComps = new DetectComposition();
            detectComps.add(push);
            parser.add(detectComps);

            DetectAggregation detectAggr = new DetectAggregation();
            detectAggr.add(push);
            parser.add(detectAggr);
        }
    }
}

