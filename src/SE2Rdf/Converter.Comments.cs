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
		private static void ConvertComments(SiteUris uris, XmlReader r, SequentialTurtleWriter w)
		{
			while (r.Read()) {
				switch (r.NodeType) {
					case XmlNodeType.Element:
						switch (r.LocalName) {
							case "row":
								using (var subR = r.ReadSubtree()) {
									subR.Read();
									ConvertComment(uris, subR, w);
								}
								break;
						}
						break;
					case XmlNodeType.EndElement:
						return;
				}
			}
		}
		
		private static void ConvertComment(SiteUris uris, XmlReader r, SequentialTurtleWriter w)
		{
			Uri subjectUri;
			if (r.MoveToAttribute("Id")) {
				subjectUri = uris.CreateCommentUri(r.Value);
				w.StartTriple(subjectUri);
			} else {
				r.MoveToElement();
				ConsoleHelper.WriteErrorLine("No Id attribute found on element {0}. Skipping element.", r.ReadOuterXml());
				return;
			}
			
			w.AddToTriple(uris.GeneralUris.TypeProperty, uris.GeneralUris.CommentType);
			uris.LinkToSite(w);
			if (r.MoveToAttribute("Score")) {
				w.AddToTriple(uris.GeneralUris.ScoreProperty, long.Parse(r.Value));
			}
			if (r.MoveToAttribute("Text")) {
				w.AddToTriple(uris.GeneralUris.DescriptionProperty, r.Value);
			}
			if (r.MoveToAttribute("CreationDate")) {
				w.AddToTriple(uris.GeneralUris.DateProperty, DateTime.Parse(r.Value, System.Globalization.CultureInfo.InvariantCulture));
			}
			if (r.MoveToAttribute("UserId")) {
				w.AddToTriple(uris.GeneralUris.OwnerProperty, uris.CreateUserUri(r.Value));
			}
			
			if (r.MoveToAttribute("PostId")) {
				w.StartTriple(uris.CreatePostUri(r.Value));
				w.AddToTriple(uris.GeneralUris.CommentProperty, subjectUri);
				w.StartTriple(subjectUri);
			} else {
				ConsoleHelper.WriteWarningLine("Orphaned comment: {0}", subjectUri);
			}
		}
	}
}
