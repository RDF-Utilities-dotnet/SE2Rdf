/*
------------------------------------------------------------------------------
This source file is a part of SE2Rdf.

Copyright (c) 2015 VIS/University of Stuttgart

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
------------------------------------------------------------------------------
 */
using System;
using System.IO;
using System.Net;
using System.Xml;

namespace SE2Rdf
{
	partial class GlobalInformationConverter
	{
		private static void ConvertSiteList(GeneralUris generalUris, string tempDir, Uri baseUri, string destDir, VDS.RDF.INamespaceMapper nsMapper)
		{
			string srcFile = Path.Combine(tempDir, "Sites.xml");
			
			ConsoleHelper.WriteMilestone("Downloading site list ...");
			using (var client = new WebClient()) {
				client.DownloadFile(new Uri(baseUri, "Sites.xml"), srcFile);
			}
			Console.WriteLine(" done.");
			
			using (var destWriter = new SequentialTurtleWriter(File.CreateText(Path.Combine(destDir, "_sites.ttl")), nsMapper)) {
				using (var fs = File.OpenRead(srcFile)) {
					using (var reader = XmlReader.Create(fs)) {
						while (reader.NodeType != XmlNodeType.Element) {
							if (!reader.Read()) {
								ConsoleHelper.WriteErrorLine("No contents found in file {0}.", srcFile);
								return;
							}
						}
						
						if (reader.LocalName == "sitelist") {
							ConvertSites(generalUris, reader, destWriter);
						} else {
							ConsoleHelper.WriteWarningLine("Unknown root element \"{0}\". Skipping document.", reader.LocalName);
						}
					}
				}
				
				GlobalData.UpdateStats(destWriter);
			}
			
			Console.WriteLine("Conversion of site list completed.");
		}
		
		private static void ConvertSites(GeneralUris generalUris, XmlReader r, SequentialTurtleWriter w)
		{
			long skipped = 0;
			
			while (r.Read()) {
				switch (r.NodeType) {
					case XmlNodeType.Element:
						switch (r.LocalName) {
							case "row":
								using (var subR = r.ReadSubtree()) {
									subR.Read();
									if (!ConvertSite(generalUris, subR, w)) {
										skipped++;
									}
								}
								break;
						}
						break;
					case XmlNodeType.EndElement:
						return;
				}
			}
			
			if (skipped > 0) {
				ConsoleHelper.WriteWarningLine("{0} items from the list of sites were skipped.", skipped);
			}
		}
		
		private static bool ConvertSite(GeneralUris generalUris, XmlReader r, SequentialTurtleWriter w)
		{
			Uri subjectUri;
			string address;
			if (r.MoveToAttribute("Address")) {
				SiteInfo info;
				if (GlobalData.Sites.TryGetValue(r.Value, out info)) {
					address = r.Value;
					subjectUri = generalUris.CreateSiteUri(info);
					w.StartTriple(subjectUri);
					w.AddToTriple(generalUris.IsMetaSiteProperty, info.IsMetaSite);
					w.AddToTriple(generalUris.LanguageProperty, info.IsEnglishSite ? "en" : info.Language);
				} else {
					return false;
				}
			} else {
				r.MoveToElement();
				ConsoleHelper.WriteErrorLine("No Address attribute found on element {0}. Skipping element.", r.ReadOuterXml());
				return false;
			}
			
			w.AddToTriple(generalUris.TypeProperty, generalUris.SiteInfoType);
			w.AddToTriple(generalUris.WebsiteProperty, new Uri("http://" + address));
			if (r.MoveToAttribute("Name")) {
				w.AddToTriple(generalUris.LabelProperty, r.Value);
				w.AddToTriple(generalUris.TitleProperty, r.Value);
			}
			if (r.MoveToAttribute("Description")) {
				w.AddToTriple(generalUris.DescriptionProperty, r.Value);
			}
			if (r.MoveToAttribute("ParentAddress")) {
				SiteInfo parentInfo;
				if (GlobalData.Sites.TryGetValue(r.Value, out parentInfo)) {
					w.AddToTriple(generalUris.ParentSiteProperty, generalUris.CreateSiteUri(parentInfo));
				} else {
					ConsoleHelper.WriteWarningLine("Unknown parent site {0}; skipping information.", r.Value);
				}
			}
			
			return true;
		}
	}
}
