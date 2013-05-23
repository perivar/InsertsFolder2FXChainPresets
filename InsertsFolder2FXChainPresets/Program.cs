using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using System.Text;
using System.Collections.Generic;

using CommonUtils;

namespace InsertsFolder2FXChainPresets
{
	class Program
	{
		public static string VERSION = "1.0.3";
		
		public static void Main(string[] args)
		{
			if (args.Length == 2) {
				//string folderPresetsFilePath = @"../../resources/InsertsFolderPresets.pxml";
				//string outputDirectoryPath = @"C:/Temp";
				
				string folderPresetsFilePath = args[0];
				string outputDirectoryPath = args[1];
				
				if (InsertFolder2FXChainConvert2(folderPresetsFilePath, outputDirectoryPath)) {
					Console.WriteLine("Succesfully converted InsertsFolderPresets.pxml");
				} else {
					Console.WriteLine("Failed converted InsertsFolderPresets.pxml!");
				}
			} else {
				PrintUsage();
			}
			
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}
		
		private static void PrintUsage() {
			Console.WriteLine("------------------------------------------------------");
			Console.WriteLine("InsertsFolder2FXChainPresets. Version {0}.", VERSION);
			Console.WriteLine("------------------------------------------------------");
			Console.WriteLine("Copyright (C) 2012-2013 Per Ivar Nerseth.");
			Console.WriteLine();
			Console.WriteLine("Usage: FindSimilar.exe <full path to InsertsFolderPresets.pxml> <full path to output directory>");
			Console.WriteLine();
			Console.WriteLine("Example:");
			Console.WriteLine("FindSimilar.exe \"C:\\Users\\<username>\\AppData\\Roaming\\Steinberg\\Cubase 6_64\\Presets\\InsertsFolderPresets.pxml\" \"C:\\Users\\<username>\\Documents\\Steinberg\\FX Chain Presets\"");
			Console.WriteLine();
		}
		
		public static bool InsertFolder2FXChainConvert(string folderPresetsFilePath, string outputDirectoryPath) {

			// How to split xml file into several files example
			// http://stackoverflow.com/questions/11912109/split-xml-document-apart-creating-multiple-output-files-from-repeating-elements
			
			string presetName;
			string outputFileName;
			XmlDocument fxChainXmlDoc = null;
			if (File.Exists(folderPresetsFilePath)) {
				if (Directory.Exists(outputDirectoryPath)) {
					XmlDocument folderPresetsXmlDoc = new XmlDocument();
					folderPresetsXmlDoc.Load(folderPresetsFilePath);

					// Split
					XmlNodeList xnList = folderPresetsXmlDoc.SelectNodes("/Presets/rootObjects/root/list/item");
					foreach (XmlNode xn in xnList)
					{
						//XmlNode stringNode = xn["string"];
						//XmlNode memberNode = xn["member"];
						XmlNode stringNode = xn.SelectSingleNode("string[@name='Name']");
						XmlNode memberNode = xn.SelectSingleNode("member[@name='Object']");
						
						// Read preset name
						presetName = stringNode.Attributes["value"].Value;
						
						// Create the XmlDocument with default content
						fxChainXmlDoc = new XmlDocument();
						XmlNode docNode = fxChainXmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
						fxChainXmlDoc.AppendChild(docNode);
						
						XmlNode fxChainNode = fxChainXmlDoc.CreateElement("FxChainPreset");
						fxChainXmlDoc.AppendChild(fxChainNode);
						
						XmlElement mediaBayNode = fxChainXmlDoc.CreateElement("member");
						mediaBayNode.SetAttribute("name", "MediaBay");
						fxChainNode.AppendChild(mediaBayNode);
						
						XmlElement listNode = fxChainXmlDoc.CreateElement("list");
						listNode.SetAttribute("name", "PAttr");
						listNode.SetAttribute("type", "obj");
						mediaBayNode.AppendChild(listNode);
						
						XmlElement objNode = fxChainXmlDoc.CreateElement("obj");
						objNode.SetAttribute("class", "StMedia::PStringIDMediaAttribute");
						objNode.SetAttribute("ID", "4130621632");
						listNode.AppendChild(objNode);
						
						// Use Linq XML (XElement) because they are easier to work with
						XElement id = new XElement("string",
						                           new XAttribute("name", "ID"),
						                           new XAttribute("value", "MediaType")
						                          );
						XElement type = new XElement("int",
						                             new XAttribute("name", "Type"),
						                             new XAttribute("value", "8")
						                            );
						XElement flags = new XElement("int",
						                              new XAttribute("name", "Flags"),
						                              new XAttribute("value", "2")
						                             );
						XElement str = new XElement("string",
						                            new XAttribute("name", "String"),
						                            new XAttribute("value", "FxChainPreset"),
						                            new XAttribute("wide", "true")
						                           );
						
						objNode.AppendChild(id.GetXmlElement(fxChainXmlDoc));
						objNode.AppendChild(type.GetXmlElement(fxChainXmlDoc));
						objNode.AppendChild(flags.GetXmlElement(fxChainXmlDoc));
						objNode.AppendChild(str.GetXmlElement(fxChainXmlDoc));
						
						// Copy over the XML content from old XML file
						XmlNode targetNode = fxChainXmlDoc.CreateElement("member"); 	// Create a separate node to hold the member element
						targetNode = fxChainXmlDoc.ImportNode(memberNode, true);    	// Bring over member
						targetNode.Attributes["name"].Value = "Folder";           	// Change attribute name from Object to Folder
						fxChainNode.AppendChild(targetNode);
						
						outputFileName = StringUtils.MakeValidFileName(presetName) + ".fxchainpreset";
						outputFileName = outputDirectoryPath + Path.DirectorySeparatorChar + outputFileName;
						
						// force explicit tag close for editController
						XElement document = fxChainXmlDoc.GetXElement();
						foreach (XElement childElement in
						         from x in document.DescendantNodes().OfType<XElement>()
						         where ( x.Attribute("name") != null
						                && x.Attribute("name").Value == "editController"
						                && x.IsEmpty)
						         select x)
						{
							childElement.Value = string.Empty;
						}
						//fxChainXmlDoc.Save(outputFileName);
						
						#region Write Steinberg XML format
						XmlWriterSettings settings = new XmlWriterSettings();
						settings.Encoding = Encoding.UTF8;
						settings.Indent = true;
						settings.IndentChars = ("   ");
						//settings.NewLineChars = "\r\n";
						//settings.NewLineHandling = NewLineHandling.Replace;
						
						StringBuilder stringBuilder = new StringBuilder();
						using (XmlWriter writer = XmlWriter.Create(stringBuilder, settings)) {
							document.Save(writer);
						}
						
						// ugly way to remove whitespace in self closing tags when writing xml document
						stringBuilder.Replace(" />", "/>");
						
						// change encoding to utf-8
						stringBuilder.Replace(Encoding.Unicode.WebName, Encoding.UTF8.WebName, 0, 56);
						
						// save to file
						using(TextWriter sw = new StreamWriter(outputFileName, false, Encoding.UTF8)) //Set encoding
						{
							sw.Write(stringBuilder.ToString());
						}
						#endregion
						
						Console.WriteLine("Saving {0} ...", outputFileName);
					}
					return true;
				} else {
					Console.WriteLine("Output Directory not found! (\"{0}\")", outputDirectoryPath);
				}
			} else {
				Console.WriteLine("Input File not found! (\"{0}\")", folderPresetsFilePath);
			}
			return false;
		}
		
		public static bool InsertFolder2FXChainConvert2(string folderPresetsFilePath, string outputDirectoryPath) {

			// How to split xml file into several files example
			// http://social.msdn.microsoft.com/Forums/en-US/csharplanguage/thread/4d4ee78d-7798-4d0e-a267-d2bc104dc747
			
			// http://stackoverflow.com/questions/6289784/xpath-and-xpathselectelement
			// The XPathSelectElement method can only be used to select elements, not attributes.
			// For attributes, you need to use the more general XPathEvaluate method:
			// var result = ((IEnumerable<object>)doc.XPathEvaluate("root/databases/db2/@name"))
			//                          .OfType<XAttribute>()
			//                          .Single()
			//                          .Value;
			
			string presetName;
			string outputFileName;
			XDocument fxChainXmlDoc = null;
			if (File.Exists(folderPresetsFilePath)) {
				if (Directory.Exists(outputDirectoryPath)) {
					XDocument folderPresetsXmlDoc = XDocument.Load(folderPresetsFilePath, LoadOptions.PreserveWhitespace);

					// Split
					IEnumerable<XElement> xnList = folderPresetsXmlDoc.XPathSelectElements("/Presets/rootObjects/root/list/item");
					foreach (XElement xn in xnList)
					{
						var stringNode = xn.Elements("string").First(element => element.Attribute("name").Value == "Name");
						var memberNode = xn.Elements("member").First(element => element.Attribute("name").Value == "Object");
						
						// Read preset name
						presetName = stringNode.Attribute("value").Value;
						
						// Create the XmlDocument with default content
						fxChainXmlDoc = new XDocument(new XDeclaration("1.0", "utf-8", null));
						
						// Use Linq XML (XElement) because they are easier to work with
						XElement fxChainNode = new XElement("FxChainPreset");
						fxChainXmlDoc.Add(fxChainNode);
						
						XElement mediaBayNode = new XElement("member",
						                                     new XAttribute("name", "MediaBay")
						                                    );
						fxChainNode.Add(mediaBayNode);
						
						XElement listNode = new XElement("list",
						                                 new XAttribute("name", "PAttr"),
						                                 new XAttribute("type", "obj")
						                                );
						mediaBayNode.Add(listNode);

						XElement objNode = new XElement("obj",
						                                new XAttribute("class", "StMedia::PStringIDMediaAttribute"),
						                                new XAttribute("ID", "4130621632")
						                               );
						listNode.Add(objNode);
						
						XElement id = new XElement("string",
						                           new XAttribute("name", "ID"),
						                           new XAttribute("value", "MediaType")
						                          );
						XElement type = new XElement("int",
						                             new XAttribute("name", "Type"),
						                             new XAttribute("value", "8")
						                            );
						XElement flags = new XElement("int",
						                              new XAttribute("name", "Flags"),
						                              new XAttribute("value", "2")
						                             );
						XElement str = new XElement("string",
						                            new XAttribute("name", "String"),
						                            new XAttribute("value", "FxChainPreset"),
						                            new XAttribute("wide", "true")
						                           );
						
						objNode.Add(id);
						objNode.Add(type);
						objNode.Add(flags);
						objNode.Add(str);
						
						// Copy over the XML content from old XML file
						XElement targetNode = new XElement(memberNode);
						targetNode.Attribute("name").SetValue("Folder");           	// Change attribute name from Object to Folder
						fxChainNode.Add(targetNode);
						
						outputFileName = StringUtils.MakeValidFileName(presetName) + ".fxchainpreset";
						outputFileName = outputDirectoryPath + Path.DirectorySeparatorChar + outputFileName;
						
						//fxChainXmlDoc.Save(outputFileName, SaveOptions.DisableFormatting);
						fxChainXmlDoc.Save(outputFileName);
						
						Console.WriteLine("Saving {0} ...", outputFileName);
					}
					return true;
				} else {
					Console.WriteLine("Output Directory not found! (\"{0}\")", outputDirectoryPath);
				}
			} else {
				Console.WriteLine("Input File not found! (\"{0}\")", folderPresetsFilePath);
			}
			return false;
		}
	}
}