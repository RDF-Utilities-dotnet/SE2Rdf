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
		private static void ConvertTags(SiteUris uris, XmlReader r, SequentialTurtleWriter w)
		{
			while (r.Read()) {
				switch (r.NodeType) {
					case XmlNodeType.Element:
						switch (r.LocalName) {
							case "row":
								using (var subR = r.ReadSubtree()) {
									subR.Read();
									ConvertTag(uris, subR, w);
								}
								break;
						}
						break;
					case XmlNodeType.EndElement:
						return;
				}
			}
		}
		
		private static void ConvertTag(SiteUris uris, XmlReader r, SequentialTurtleWriter w)
		{
			Uri subjectUri;
			if (r.MoveToAttribute("TagName")) {
				subjectUri = uris.CreateTagUri(r.Value);
				w.StartTriple(subjectUri);
			} else {
				r.MoveToElement();
				ConsoleHelper.WriteErrorLine("No TagName attribute found on element {0}. Skipping element.", r.ReadOuterXml());
				return;
			}
			
			w.AddToTriple(uris.GeneralUris.LabelProperty, r.Value);
			w.AddToTriple(uris.GeneralUris.TypeProperty, uris.GeneralUris.TagType);
			uris.LinkToSite(w);
			if (r.MoveToAttribute("ExcerptPostId")) {
				w.AddToTriple(uris.GeneralUris.TagExcerptProperty, uris.CreatePostUri(r.Value));
			}
			if (r.MoveToAttribute("WikiPostId")) {
				w.AddToTriple(uris.GeneralUris.TagDescriptionProperty, uris.CreatePostUri(r.Value));
			}
			// TODO: Count
		}
	}
}
