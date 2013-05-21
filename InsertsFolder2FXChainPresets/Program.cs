using System;
using System.Xml;
using System.Xml.Linq;
using System.IO;

using CommonUtils;

namespace InsertsFolder2FXChainPresets
{
	class Program
	{
		public static string VERSION = "1.0.0";
		
		public static void Main(string[] args)
		{
			if (args.Length == 2) {
				//string folderPresetsFilePath = @"../../resources/InsertsFolderPresets.pxml";
				//string outputDirectoryPath = @"C:/Temp";
				
				string folderPresetsFilePath = args[0];
				string outputDirectoryPath = args[1];
				
				if (InsertFolder2FXChainConvert(folderPresetsFilePath, outputDirectoryPath)) {
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

						fxChainXmlDoc.Save(outputFileName);
						/*
					// This does not write utf-8 ?
					XmlTextWriter xmlWriter = new XmlTextWriter(strFileName, null);
					xmlWriter.Formatting = Formatting.Indented;
					xmlWriter.Indentation = 3;
					newXmlDoc.Save(xmlWriter);
						 */
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