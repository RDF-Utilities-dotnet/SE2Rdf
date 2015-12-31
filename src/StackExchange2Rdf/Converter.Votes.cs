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

namespace StackExchange2Rdf
{
	partial class Converter
	{
		private static void ConvertVotes(SiteUris uris, XmlReader r, SequentialTurtleWriter w)
		{
			var unknownVoteTypeIds = new UnknownValueStore<string>();
			
			while (r.Read()) {
				switch (r.NodeType) {
					case XmlNodeType.Element:
						switch (r.LocalName) {
							case "row":
								using (var subR = r.ReadSubtree()) {
									subR.Read();
									ConvertVote(uris, subR, w, unknownVoteTypeIds);
								}
								break;
						}
						break;
					case XmlNodeType.EndElement:
						long unknownVoteTypeIdCount = unknownVoteTypeIds.RegisteredValueCount;
						if (unknownVoteTypeIdCount > 0) {
							ConsoleHelper.WriteWarningLine("{0} unknown VoteTypeId value(s) found: {1}", unknownVoteTypeIdCount, unknownVoteTypeIds);
						}
						
						return;
				}
			}
		}
		
		private static void ConvertVote(SiteUris uris, XmlReader r, SequentialTurtleWriter w, UnknownValueStore<string> unknownVoteTypeIds)
		{
			Uri subjectUri;
			if (r.MoveToAttribute("Id")) {
				subjectUri = uris.CreateVoteUri(r.Value);
				w.StartTriple(subjectUri);
			} else {
				r.MoveToElement();
				ConsoleHelper.WriteErrorLine("No Id attribute found on element {0}. Skipping element.", r.ReadOuterXml());
				return;
			}
			
			if (r.MoveToAttribute("VoteTypeId")) {
				switch (r.Value) {
					case "1": // acceptance
						if (GlobalData.Options.FullTimeInfo) {
							w.AddToTriple(uris.GeneralUris.TypeProperty, uris.GeneralUris.AcceptanceType);
							uris.LinkToSite(w);
							if (r.MoveToAttribute("PostId")) {
								w.StartTriple(uris.CreatePostUri(r.Value));
								w.AddToTriple(uris.GeneralUris.AcceptanceProperty, subjectUri);
								w.StartTriple(subjectUri);
							}
							if (r.MoveToAttribute("CreationDate")) {
								w.AddToTriple(uris.GeneralUris.DateProperty, DateTime.Parse(r.Value, System.Globalization.CultureInfo.InvariantCulture));
							}
						}
						break;
					case "2": // upvote
						if (GlobalData.Options.FullTimeInfo) {
							w.AddToTriple(uris.GeneralUris.TypeProperty, uris.GeneralUris.UpVoteType);
							uris.LinkToSite(w);
							if (r.MoveToAttribute("CreationDate")) {
								w.AddToTriple(uris.GeneralUris.DateProperty, DateTime.Parse(r.Value, System.Globalization.CultureInfo.InvariantCulture));
							}
							if (r.MoveToAttribute("PostId")) {
								w.StartTriple(uris.CreatePostUri(r.Value));
								w.AddToTriple(uris.GeneralUris.VoteProperty, subjectUri);
								w.StartTriple(subjectUri);
							}
						}
						break;
					case "3": // downvote
						if (GlobalData.Options.FullTimeInfo) {
							w.AddToTriple(uris.GeneralUris.TypeProperty, uris.GeneralUris.DownVoteType);
							uris.LinkToSite(w);
							if (r.MoveToAttribute("CreationDate")) {
								w.AddToTriple(uris.GeneralUris.DateProperty, DateTime.Parse(r.Value, System.Globalization.CultureInfo.InvariantCulture));
							}
							if (r.MoveToAttribute("PostId")) {
								w.StartTriple(uris.CreatePostUri(r.Value));
								w.AddToTriple(uris.GeneralUris.VoteProperty, subjectUri);
								w.StartTriple(subjectUri);
							}
						}
						break;
					case "4": // offensive
						break;
					case "5": // favorite
						if (GlobalData.Options.FullTimeInfo) {
							w.AddToTriple(uris.GeneralUris.TypeProperty, uris.GeneralUris.FavoriteType);
							uris.LinkToSite(w);
							if (r.MoveToAttribute("CreationDate")) {
								w.AddToTriple(uris.GeneralUris.DateProperty, DateTime.Parse(r.Value, System.Globalization.CultureInfo.InvariantCulture));
							}
							if (r.MoveToAttribute("PostId")) {
								w.AddToTriple(uris.GeneralUris.PostProperty, uris.CreatePostUri(r.Value));
							}
							if (r.MoveToAttribute("UserId")) {
								w.StartTriple(uris.CreateUserUri(r.Value));
								w.AddToTriple(uris.GeneralUris.FavoriteProperty, subjectUri);
								w.StartTriple(subjectUri);
							}
						} else {
							if (r.MoveToAttribute("UserId")) {
								w.StartTriple(uris.CreateUserUri(r.Value));
								if (r.MoveToAttribute("PostId")) {
									w.AddToTriple(uris.GeneralUris.FavoriteProperty, uris.CreatePostUri(r.Value));
								}
								w.StartTriple(subjectUri);
							}
						}
						break;
					case "6": // closed
						break;
					case "7": // reopened
						break;
					case "8": // bounty started
						w.AddToTriple(uris.GeneralUris.TypeProperty, uris.GeneralUris.StartOfBountyType);
						if (r.MoveToAttribute("PostId")) {
							w.AddToTriple(uris.GeneralUris.PostProperty, uris.CreatePostUri(r.Value));
						}
						if (r.MoveToAttribute("UserId")) {
							w.AddToTriple(uris.GeneralUris.DonorProperty, uris.CreateUserUri(r.Value));
						}
						if (r.MoveToAttribute("CreationDate")) {
							w.AddToTriple(uris.GeneralUris.DateProperty, DateTime.Parse(r.Value, System.Globalization.CultureInfo.InvariantCulture));
						}
						if (r.MoveToAttribute("BountyAmount")) {
							w.AddToTriple(uris.GeneralUris.OfferedAmountProperty, long.Parse(r.Value));
						}
						break;
					case "9": // bounty closed
						w.AddToTriple(uris.GeneralUris.TypeProperty, uris.GeneralUris.EndOfBountyType);
						if (r.MoveToAttribute("PostId")) {
							w.AddToTriple(uris.GeneralUris.AnswerProperty, uris.CreatePostUri(r.Value));
						}
						if (r.MoveToAttribute("CreationDate")) {
							w.AddToTriple(uris.GeneralUris.DateProperty, DateTime.Parse(r.Value, System.Globalization.CultureInfo.InvariantCulture));
						}
						if (r.MoveToAttribute("BountyAmount")) {
							w.AddToTriple(uris.GeneralUris.TransferredAmountProperty, long.Parse(r.Value));
						}
						break;
					case "10": // deletion
						break;
					case "11": // undeletion
						break;
					case "12": // spam
						break;
					case "13": // moderator informed
						break;
					case "15": // under moderator review
						break;
					case "16": // approved edit suggestion
						break;
					default:
						unknownVoteTypeIds.RegisterUnknownValue(r.Value);
						break;
				}
			} else {
				r.MoveToElement();
				ConsoleHelper.WriteErrorLine("No VoteTypeId attribute found on element {0}. Skipping element.", r.ReadOuterXml());
			}
		}
	}
}
