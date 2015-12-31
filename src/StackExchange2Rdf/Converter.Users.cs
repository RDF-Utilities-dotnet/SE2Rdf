/*
------------------------------------------------------------------------------
This source file is a part of StackExchange2Rdf.

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
using System.Collections.Generic;

namespace StackExchange2Rdf
{
	partial class Converter
	{
		private static void ConvertUsers(SiteUris uris, XmlReader r, SequentialTurtleWriter w)
		{
			var malformedIris = new List<string>();
			long totalMalformedIriCount = 0;
			
			while (r.Read()) {
				switch (r.NodeType) {
					case XmlNodeType.Element:
						switch (r.LocalName) {
							case "row":
								using (var subR = r.ReadSubtree()) {
									subR.Read();
									ConvertUser(uris, subR, w, malformedIris, ref totalMalformedIriCount);
								}
								break;
						}
						break;
					case XmlNodeType.EndElement:
						if (totalMalformedIriCount > 0) {
							string example;
							if (malformedIris.Count > 0) {
								var exampleBuilder = new System.Text.StringBuilder(" (e.g. ");
								for (int i = 0; i < malformedIris.Count; i++) {
									if (i > 0) {
										exampleBuilder.Append("; ");
									}
									exampleBuilder.Append(malformedIris[i]);
								}
								exampleBuilder.Append(")");
								example = exampleBuilder.ToString();
							} else {
								example = "";
							}
							ConsoleHelper.WriteWarningLine("{1} malformed URL(s) found{0}, treated as string literals.", example, totalMalformedIriCount);
						}
						return;
				}
			}
		}
		
		private static readonly Regex websiteRegex = new Regex(@"^[a-z]+://(?:[A-Za-z0-9](?:-?[A-Za-z0-9])*\.)+[A-Za-z]+(?:$|/)");
		
		private static readonly Regex emptyWebsiteRegex = new Regex(@"http://(?:n/a|na|n\.a\.|nothing|google|gmail|facebook|apple|yahoo|safari|iphone|website|nowebsite|noneyet|none|empty|null|-+|localhost|127\.0\.0\.1|about:blank|underconstruction)\.?/?$");
		
		private static void ConvertUser(SiteUris uris, XmlReader r, SequentialTurtleWriter w, ICollection<string> malformedIris, ref long totalMalformedIriCount)
		{
			Uri subjectUri;
			if (r.MoveToAttribute("Id")) {
				subjectUri = uris.CreateUserUri(r.Value);
				w.StartTriple(subjectUri);
			} else {
				r.MoveToElement();
				ConsoleHelper.WriteErrorLine("No Id attribute found on element {0}. Skipping element.", r.ReadOuterXml());
				return;
			}
			
			w.AddToTriple(uris.GeneralUris.TypeProperty,
			              uris.GeneralUris.UserType);
			uris.LinkToSite(w);
			if (r.MoveToAttribute("DisplayName")) {
				w.AddToTriple(uris.GeneralUris.LabelProperty, r.Value);
				w.AddToTriple(uris.GeneralUris.UserNameProperty, r.Value);
			}
			if (r.MoveToAttribute("CreationDate")) {
				w.AddToTriple(uris.GeneralUris.DateProperty, DateTime.Parse(r.Value, System.Globalization.CultureInfo.InvariantCulture));
			}
			if (r.MoveToAttribute("Reputation")) {
				w.AddToTriple(uris.GeneralUris.ReputationProperty, long.Parse(r.Value));
			}
			if (r.MoveToAttribute("Location")) {
				w.AddToTriple(uris.GeneralUris.LocationProperty, r.Value);
			}
			if (r.MoveToAttribute("Age")) {
				w.AddToTriple(uris.GeneralUris.AgeProperty, long.Parse(r.Value));
			}
			if (r.MoveToAttribute("Views")) {
				w.AddToTriple(uris.GeneralUris.ViewCountProperty, long.Parse(r.Value));
			}
			if (r.MoveToAttribute("UpVotes")) {
				w.AddToTriple(uris.GeneralUris.UpVotesProperty, long.Parse(r.Value));
			}
			if (r.MoveToAttribute("DownVotes")) {
				w.AddToTriple(uris.GeneralUris.DownVotesProperty, long.Parse(r.Value));
			}
			if (r.MoveToAttribute("WebsiteUrl")) {
				string websiteUrl = r.Value;
				if (!string.IsNullOrWhiteSpace(websiteUrl)) {
					if (websiteUrl.ToLowerInvariant().StartsWith("http")) {
						websiteUrl = "http" + websiteUrl.Substring(4);
					}
					if (!emptyWebsiteRegex.IsMatch(websiteUrl.ToLowerInvariant())) {
						if (websiteRegex.IsMatch(websiteUrl)) {
							try {
								Uri homepageUrl = new Uri(websiteUrl);
								w.AddToTriple(uris.GeneralUris.WebsiteProperty, homepageUrl);
							}
							catch (UriFormatException) {
								totalMalformedIriCount++;
								if (malformedIris.Count < GlobalData.Options.MaxDisplayedMalformedIris) {
									malformedIris.Add(websiteUrl);
								}
								w.AddToTriple(uris.GeneralUris.WebsiteProperty, websiteUrl);
							}
						} else {
							totalMalformedIriCount++;
							if (malformedIris.Count < GlobalData.Options.MaxDisplayedMalformedIris) {
								malformedIris.Add(websiteUrl);
							}
							w.AddToTriple(uris.GeneralUris.WebsiteProperty, websiteUrl);
						}
					}
				}
			}
			if (r.MoveToAttribute("AccountId")) {
				w.AddToTriple(uris.GeneralUris.AccountProperty, uris.GeneralUris.CreateAccountUri(r.Value));
				GlobalData.AccountIds.Add(r.Value);
			}
			if (r.MoveToAttribute("AboutMe")) {
				string desc = r.Value;
				if (!string.IsNullOrWhiteSpace(desc)) {
					w.AddToTriple(uris.GeneralUris.DescriptionProperty, desc);
				}
			}
			if (r.MoveToAttribute("LastAccessDate")) {
				w.AddToTriple(uris.GeneralUris.LastSeenProperty, DateTime.Parse(r.Value, System.Globalization.CultureInfo.InvariantCulture));
			}
		}
	}
}
