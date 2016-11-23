using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

namespace XMLFilter_v1
{
    class Program
    {

        private static void Search(XmlDocument xmlDocument, XmlNode node, string string_to_find)
        {
            int noOfBlocksCovered = 0, noOfBlocksNotCovered = 0;
            string string_name = "";
            
            noOfBlocksCovered = GetNumberData(xmlDocument, node, "BlocksCovered");
            noOfBlocksNotCovered = GetNumberData(xmlDocument, node, "BlocksNotCovered");
            string_name = GetNameData(xmlDocument, node, string_to_find);

            string_to_find = string_to_find.Remove(string_to_find.Length - 4);
            if (string_to_find == "Class")
            {
                Console.SetCursorPosition(6, Console.CursorTop+1);
            }
            Console.Write("{0}: {1}", string_to_find, string_name);
            Console.SetCursorPosition(50, Console.CursorTop);
            Console.Write("Blocks covered: " + String.Format("{0:0.##%}", (float)noOfBlocksCovered / (noOfBlocksCovered + noOfBlocksNotCovered)));
            if (noOfBlocksCovered != 0)
                Console.Write(" ({0}/{1})", noOfBlocksCovered, noOfBlocksCovered + noOfBlocksNotCovered);
            //Console.Write("\n"); //to see more clear, remove comment tags. 
        }



        private static void Reading(XmlDocument xmlDocument, XmlNode node)
        {
            Console.Write("\n");
            Search(xmlDocument, node, "ModuleName");
            foreach (XmlNode childNode in node)
            {
                if (NodeNameHasValue(childNode, "NamespaceTable"))
                {
                    foreach (XmlNode childOfChildNode in childNode)
                    {
                        if (NodeNameHasValue( childOfChildNode,"Class"))
                        {
                            Search(xmlDocument, childOfChildNode, "ClassName");
                        }
                    }
                }
            }
        }
   

        static void Main(string[] args)
        {
            
            XmlDocument xmlDocument = new XmlDocument();
            do
            {
                Console.Clear();
                Console.WriteLine("Type the path to the XML file, or folder");
                string pathname = "";
                pathname = Console.ReadLine();
                
                //int number;

                if (File.Exists(pathname))
                {
                    Console.WriteLine("The path is correct!\nAre you sure you want to use this file? (If not, type: NO, no)");
                    Console.WriteLine(pathname);
                    string option = Console.ReadLine();

                    if (option.ToUpper() != "NO")
                    {
                        Console.WriteLine("Path chosen to open XML");
                        xmlDocument.Load(pathname);
                        break;
                    }     
                }
                else if (Directory.Exists(pathname))
                {
                    //int i = 0;
                    if (!pathname.EndsWith("\\"))
                    {
                        pathname += '\\';
                    }
                    string[] files = Directory.GetFiles(pathname, "*.coveragexml");
                    if (files.Length!=0)
                    {
                        Console.WriteLine("\nFiles with the extension .coveragexml");
                        int i=0;
                        foreach (string s in files)
                        {
                            i++;
                            Console.WriteLine(i + ". " + s);
                        }
                        Console.WriteLine("\nPlease write the number of the file you want to load.\nTo Cancel, write a number different from those above");
                        int number;
                        Int32.TryParse(Console.ReadLine(), out number);
                        if (number <= i && number > 0)
                        {
                            Console.WriteLine(files[number - 1]);
                            xmlDocument.Load(files[number - 1]);
                            break;
                        } 
                    }
                    else
                    {
                        Console.WriteLine("No file was found with the extension .coveragexml in this directory,\nPlease, press any key");
                        Console.ReadKey();
                    }
                }
                else
                {
                    Console.WriteLine("Invalid path, press any key");
                    Console.ReadKey();
                }

            }while (true);

            
            Console.Clear();
            string ending = "test.dll";
            Console.WriteLine("Which ending do you want to neglect? (by default: test.dll)");
            string g = Console.ReadLine().ToString();
            if (!(g == null || g.Length == 0))
            {
                ending = g;
            }

            ComputeCoverage(xmlDocument, ending);

            Console.ReadKey();
        }

        private static void ComputeCoverage(XmlDocument xmlDocument, string ending)
        {
            IEnumerable<XmlNode> filteredNodes = FilterNodes(xmlDocument, "Module");
            foreach (XmlNode node in filteredNodes)
            {
                Process(xmlDocument, ending, node);
            }
        }

        //TODO use this method wherever possible to avoid duplication: 1st attempt
        private static IEnumerable<XmlNode> FilterNodes(XmlDocument xmlDocument, string nodeName)
        {
            XmlNode rootNode = xmlDocument.DocumentElement;
            foreach (XmlNode childNode in rootNode)
            {
                if (NodeNameHasValue(childNode, nodeName))
                {
                    yield return childNode;
                }
            }
        }

        private static IEnumerable<XmlNode> FilterNodes(XmlDocument xmlDocument, string nodeName, XmlNode rootNode)
        {
            if (!rootNode.HasChildNodes)
                rootNode = xmlDocument.DocumentElement;
        
            foreach (XmlNode childNode in rootNode)
            {
                if (NodeNameHasValue(childNode, nodeName))
                {
                    yield return childNode;
                }
            }
        }

        private static void Process(XmlDocument xmlDocument, string ending, XmlNode childNode)
        {
            //TODO si daca nu e primul? :1st attempt
            XmlNode moduleNameNode = FilterNodes(xmlDocument, "Module").First();
            //TODO make it work for upper & lower case: DONE
            if (!(moduleNameNode.InnerText.ToUpper().EndsWith(ending.ToUpper())))
            {
                Reading(xmlDocument, childNode);
            }
        }

        private static bool NodeNameHasValue(XmlNode childNode, string name)
        {
            return childNode.Name == name;
        }

        private static int GetNumberData(XmlDocument xmlDocument, XmlNode node, string stringh)
        {
            return Int32.Parse(FilterNodes(xmlDocument, stringh, node).First().InnerText);
        }

        private static string GetNameData(XmlDocument xmlDocument, XmlNode node, string stringh)
        {
            return FilterNodes(xmlDocument, stringh, node).First().InnerText;
        }
    }
}