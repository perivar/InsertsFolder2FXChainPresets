using System;
using System.Xml;
using System.Xml.Linq;
using System.IO;

using CommonUtils;

namespace InsertsFolder2FXChainPresets
{
	class Program
	{
		public static void Main(string[] args)
		{
			// How to split xml file into several files example
			// http://stackoverflow.com/questions/11912109/split-xml-document-apart-creating-multiple-output-files-from-repeating-elements
			
			string filePath = @"../../resources/InsertsFolderPresets.pxml";
			if (InsertFolder2FXChainConvert(filePath)) {
				Console.WriteLine("Succesfully converted InsertsFolderPresets.pxml");
			} else {
				Console.WriteLine("Failed converted InsertsFolderPresets.pxml!");
			}
			
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}
		
		public static bool InsertFolder2FXChainConvert(string folderPresetsFilePath) {

			string presetName;
			string outputFileName;
			XmlDocument fxChainXmlDoc = null;
			if (File.Exists(folderPresetsFilePath)) {
				
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
				Console.WriteLine("Input File not found! ({0})", folderPresetsFilePath);
				return false;
			}
		}
	}
}