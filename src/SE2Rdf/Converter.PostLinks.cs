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
	partial class Converter
	{
		private static void ConvertPostLinks(SiteUris uris, XmlReader r, SequentialTurtleWriter w)
		{
			var unknownLinkTypeIds = new UnknownValueStore<string>();
			
			while (r.Read()) {
				switch (r.NodeType) {
					case XmlNodeType.Element:
						switch (r.LocalName) {
							case "row":
								using (var subR = r.ReadSubtree()) {
									subR.Read();
									ConvertPostLink(uris, subR, w, unknownLinkTypeIds);
								}
								break;
						}
						break;
					case XmlNodeType.EndElement:
						long unknownLinkTypeIdCount = unknownLinkTypeIds.RegisteredValueCount;
						if (unknownLinkTypeIdCount > 0) {
							ConsoleHelper.WriteWarningLine("{0} unknown LinkTypeId value(s) found: {1}", unknownLinkTypeIdCount, unknownLinkTypeIds);
						}
						
						return;
				}
			}
		}
		
		private static void ConvertPostLink(SiteUris uris, XmlReader r, SequentialTurtleWriter w, UnknownValueStore<string> unknownLinkTypeIds)
		{
			if (r.MoveToAttribute("LinkTypeId")) {
				switch (r.Value) {
					case "1": // linked
						if (r.MoveToAttribute("PostId")) {
							w.StartTriple(uris.CreatePostUri(r.Value));
							if (r.MoveToAttribute("RelatedPostId")) {
								w.AddToTriple(uris.GeneralUris.LinkProperty, uris.CreatePostUri(r.Value));
							}
						}
						break;
					case "3": // duplicate
						if (r.MoveToAttribute("RelatedPostId")) {
							w.StartTriple(uris.CreatePostUri(r.Value));
							if (r.MoveToAttribute("PostId")) {
								w.AddToTriple(uris.GeneralUris.DuplicateProperty, uris.CreatePostUri(r.Value));
							}
						}
						break;
					default:
						unknownLinkTypeIds.RegisterUnknownValue(r.Value);
						break;
				}
			} else {
				r.MoveToElement();
				ConsoleHelper.WriteErrorLine("No LinkTypeId attribute found on element {0}. Skipping element.", r.ReadOuterXml());
			}
		}
	}
}
