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
using System.Text.RegularExpressions;
using System.Linq;

namespace SE2Rdf
{
	partial class Converter
	{
		private static void ConvertPosts(SiteUris uris, XmlReader r, SequentialTurtleWriter w)
		{
			var unknownPostTypeIds = new UnknownValueStore<string>();
			
			while (r.Read()) {
				switch (r.NodeType) {
					case XmlNodeType.Element:
						switch (r.LocalName) {
							case "row":
								using (var subR = r.ReadSubtree()) {
									subR.Read();
									ConvertPost(uris, subR, w, unknownPostTypeIds);
								}
								break;
						}
						break;
					case XmlNodeType.EndElement:
						long unknownPostTypeIdCount = unknownPostTypeIds.RegisteredValueCount;
						if (unknownPostTypeIdCount > 0) {
							ConsoleHelper.WriteWarningLine("{0} unknown PostTypeId value(s) found: {1}", unknownPostTypeIdCount, unknownPostTypeIds);
						}
						
						return;
				}
			}
		}
		
		private static readonly Regex tagRegex = new Regex(@"<([^<>]+)>");
		
		private static void ConvertPost(SiteUris uris, XmlReader r, SequentialTurtleWriter w, UnknownValueStore<string> unknownPostTypeIds)
		{
			Uri subjectUri;
			if (r.MoveToAttribute("Id")) {
				subjectUri = uris.CreatePostUri(r.Value);
				w.StartTriple(subjectUri);
			} else {
				r.MoveToElement();
				ConsoleHelper.WriteErrorLine("No Id attribute found on element {0}. Skipping element.", r.ReadOuterXml());
				return;
			}
			
			if (r.MoveToAttribute("PostTypeId")) {
				switch (r.Value) {
					case "1": // question
						w.AddToTriple(uris.GeneralUris.TypeProperty, uris.GeneralUris.QuestionType);
						uris.LinkToSite(w);
						if (r.MoveToAttribute("AcceptedAnswerId")) {
							w.AddToTriple(uris.GeneralUris.AcceptedAnswerProperty, uris.CreatePostUri(r.Value));
						}
						if (r.MoveToAttribute("ViewCount")) {
							w.AddToTriple(uris.GeneralUris.ViewCountProperty, long.Parse(r.Value));
						}
						if (r.MoveToAttribute("Title")) {
							w.AddToTriple(uris.GeneralUris.TitleProperty, r.Value);
							w.AddToTriple(uris.GeneralUris.LabelProperty, r.Value);
						}
						if (r.MoveToAttribute("Score")) {
							w.AddToTriple(uris.GeneralUris.ScoreProperty, long.Parse(r.Value));
						}
						break;
					case "2": // answer
						w.AddToTriple(uris.GeneralUris.TypeProperty, uris.GeneralUris.AnswerType);
						uris.LinkToSite(w);
						if (r.MoveToAttribute("ParentId")) {
							w.StartTriple(uris.CreatePostUri(r.Value));
							w.AddToTriple(uris.GeneralUris.AnswerProperty, subjectUri);
							w.StartTriple(subjectUri);
						} else {
							ConsoleHelper.WriteWarningLine("Orphaned answer: {0}", subjectUri);
						}
						if (r.MoveToAttribute("Score")) {
							w.AddToTriple(uris.GeneralUris.ScoreProperty, long.Parse(r.Value));
						}
						break;
					case "3": // orphaned tag wiki
						break;
					case "4": // tag info excerpt
						w.AddToTriple(uris.GeneralUris.TypeProperty, uris.GeneralUris.TagExcerptType);
						break;
					case "5": // tag description
						w.AddToTriple(uris.GeneralUris.TypeProperty, uris.GeneralUris.TagDescriptionType);
						break;
					case "6": // moderator nomination
						break;
					case "7": // "Wiki placeholder" (seems to only be the election description)
						//w.AddToTriple(uris.GeneralUris.TypeProperty, uris.GeneralUris.SiteInfoType);
						break;
					default:
						unknownPostTypeIds.RegisterUnknownValue(r.Value);
						break;
				}
				if (r.MoveToAttribute("CreationDate")) {
					w.AddToTriple(uris.GeneralUris.DateProperty, DateTime.Parse(r.Value, System.Globalization.CultureInfo.InvariantCulture));
				}
				if (r.MoveToAttribute("LastEditDate")) {
					w.AddToTriple(uris.GeneralUris.LastEditDateProperty, DateTime.Parse(r.Value, System.Globalization.CultureInfo.InvariantCulture));
				}
				if (r.MoveToAttribute("LastActivity")) {
					w.AddToTriple(uris.GeneralUris.LastActivityDateProperty, DateTime.Parse(r.Value, System.Globalization.CultureInfo.InvariantCulture));
				}
				if (r.MoveToAttribute("OwnerUserId")) {
					w.AddToTriple(uris.GeneralUris.OwnerProperty, uris.CreateUserUri(r.Value));
				}
				// TODO: LastEditorUserId (given in post history)
				// TODO: FavoriteCount (linked to users?)
				if (r.MoveToAttribute("Body")) {
					w.AddToTriple(uris.GeneralUris.DescriptionProperty, r.Value);
				}
				if (r.MoveToAttribute("Tags")) {
					w.AddToTriple(uris.GeneralUris.TagProperty,
					              tagRegex.Matches(r.Value).Cast<Match>().Select(m => uris.CreateTagUri(m.Groups[1].Value)));
				}
			} else {
				r.MoveToElement();
				ConsoleHelper.WriteErrorLine("No PostTypeId attribute found on element {0}. Skipping element.", r.ReadOuterXml());
			}
		}
	}
}
