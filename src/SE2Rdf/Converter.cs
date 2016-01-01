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
using System.Xml;

namespace SE2Rdf
{
	internal static partial class Converter
	{
		public static void Convert(GeneralUris generalUris, string srcFile, string destDir, SiteInfo website)
		{
			string fileNameOnly = Path.GetFileName(srcFile);
			Console.WriteLine("Processing {0} ...", fileNameOnly);
			
			var siteUris = new SiteUris(generalUris, website);
			
			var nsMapper = siteUris.CreateNamespaceMapper();
			
			using (var destWriter = new SequentialTurtleWriter(File.CreateText(Path.Combine(destDir, website + "-" + Path.GetFileNameWithoutExtension(srcFile) + ".ttl")), nsMapper)) {
				using (var fs = File.OpenRead(srcFile)) {
					using (var reader = XmlReader.Create(fs)) {
						while (reader.NodeType != XmlNodeType.Element) {
							if (!reader.Read()) {
								ConsoleHelper.WriteErrorLine("No contents found in file {0}.", srcFile);
								return;
							}
						}
						
						switch (reader.LocalName) {
							case "badges":
								ConsoleHelper.WriteInfoLine("List of badges identified.");
								ConvertBadges(siteUris, reader, destWriter);
								break;
							case "comments":
								ConsoleHelper.WriteInfoLine("List of comments identified.");
								ConvertComments(siteUris, reader, destWriter);
								break;
							case "posthistory":
								ConsoleHelper.WriteInfoLine("List of posthistory identified.");
								ConvertPostHistory(siteUris, reader, destWriter);
								break;
							case "postlinks":
								ConsoleHelper.WriteInfoLine("List of postlinks identified.");
								ConvertPostLinks(siteUris, reader, destWriter);
								break;
							case "posts":
								ConsoleHelper.WriteInfoLine("List of posts identified.");
								ConvertPosts(siteUris, reader, destWriter);
								break;
							case "tags":
								ConsoleHelper.WriteInfoLine("List of tags identified.");
								ConvertTags(siteUris, reader, destWriter);
								break;
							case "users":
								ConsoleHelper.WriteInfoLine("List of users identified.");
								ConvertUsers(siteUris, reader, destWriter);
								break;
							case "votes":
								ConsoleHelper.WriteInfoLine("List of votes identified.");
								ConvertVotes(siteUris, reader, destWriter);
								break;
							default:
								ConsoleHelper.WriteWarningLine("Unknown root element \"{0}\". Skipping document.", reader.LocalName);
								break;
						}
					}
				}
				
				GlobalData.UpdateStats(destWriter);
			}
			
			Console.WriteLine("Conversion of {0} completed.", fileNameOnly);
		}
	}
}
