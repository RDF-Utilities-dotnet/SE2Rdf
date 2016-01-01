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
using System.Collections.Generic;

using Newtonsoft.Json;

namespace SE2Rdf
{
	partial class Converter
	{
		private static void ConvertPostHistory(SiteUris uris, XmlReader r, SequentialTurtleWriter w)
		{
			var unknownPostHistoryTypeIds = new UnknownValueStore<string>();
			
			while (r.Read()) {
				switch (r.NodeType) {
					case XmlNodeType.Element:
						switch (r.LocalName) {
							case "row":
								using (var subR = r.ReadSubtree()) {
									subR.Read();
									ConvertPostHistoryItem(uris, subR, w, unknownPostHistoryTypeIds);
								}
								break;
						}
						break;
					case XmlNodeType.EndElement:
						long unknownPostHistoryTypeIdCount = unknownPostHistoryTypeIds.RegisteredValueCount;
						if (unknownPostHistoryTypeIdCount > 0) {
							ConsoleHelper.WriteWarningLine("{0} unknown PostHistoryTypeId value(s) found: {1}", unknownPostHistoryTypeIdCount, unknownPostHistoryTypeIds);
						}
						
						return;
				}
			}
		}
		
		private static void ConvertPostHistoryItem(SiteUris uris, XmlReader r, SequentialTurtleWriter w, UnknownValueStore<string> unknownPostHistoryTypeIds)
		{
			Uri subjectUri;
			if (r.MoveToAttribute("Id")) {
				subjectUri = uris.CreatePostHistoryUri(r.Value);
				w.StartTriple(subjectUri);
			} else {
				r.MoveToElement();
				ConsoleHelper.WriteErrorLine("No Id attribute found on element {0}. Skipping element.", r.ReadOuterXml());
				return;
			}
			
			if (r.MoveToAttribute("PostHistoryTypeId")) {
				switch (r.Value) {
					case "1": // initial title
						break;
					case "2": // initial body
						break;
					case "3": // initial tags
						break;
					case "4": // edit title
						break;
					case "5": // edit body
						break;
					case "6": // edit tags
						break;
					case "7": // rollback title
						break;
					case "8": // rollback body
						break;
					case "9": // rollback tags
						break;
					case "10": // post closed
						w.AddToTriple(uris.GeneralUris.TypeProperty, uris.GeneralUris.PostClosureType);
						if (r.MoveToAttribute("Comment")) {
							switch (r.Value) {
								case "1": // Exact Duplicate
								case "101": // duplicate
									w.AddToTriple(uris.GeneralUris.CloseReasonProperty, uris.GeneralUris.DuplicateCloseReason);
									break;
								case "2": // Off-topic
								case "102": // Off-topic
									w.AddToTriple(uris.GeneralUris.CloseReasonProperty, uris.GeneralUris.OffTopicCloseReason);
									break;
								case "3": // Subjective and argumentative
								case "105": // Primarily opinion-based
									w.AddToTriple(uris.GeneralUris.CloseReasonProperty, uris.GeneralUris.SubjectiveCloseReason);
									break;
								case "4": // Not a real question
									w.AddToTriple(uris.GeneralUris.CloseReasonProperty, uris.GeneralUris.NotAQuestionCloseReason);
									break;
								case "7": // Too localized
									w.AddToTriple(uris.GeneralUris.CloseReasonProperty, uris.GeneralUris.TooLocalizedCloseReason);
									break;
								case "10": // General reference
									w.AddToTriple(uris.GeneralUris.CloseReasonProperty, uris.GeneralUris.GeneralReferenceCloseReason);
									break;
								case "20": // Noise or pointless
									w.AddToTriple(uris.GeneralUris.CloseReasonProperty, uris.GeneralUris.NoiseCloseReason);
									break;
								case "103": // Unclear what you're asking
									w.AddToTriple(uris.GeneralUris.CloseReasonProperty, uris.GeneralUris.UnclearCloseReason);
									break;
								case "104": // Too broad
									w.AddToTriple(uris.GeneralUris.CloseReasonProperty, uris.GeneralUris.TooBroadCloseReason);
									break;
								default:
									ConsoleHelper.WriteWarningLine("Unknown post close reason: {0}", r.Value);
									break;
							}
						}
						if (r.MoveToAttribute("CreationDate")) {
							w.AddToTriple(uris.GeneralUris.DateProperty, DateTime.Parse(r.Value, System.Globalization.CultureInfo.InvariantCulture));
						}
						AddParticipants(uris, r, w);
						LinkToPost(uris, subjectUri, r, w);
						break;
					case "11": // post reopened
						w.AddToTriple(uris.GeneralUris.TypeProperty, uris.GeneralUris.PostReopeningType);
						if (r.MoveToAttribute("CreationDate")) {
							w.AddToTriple(uris.GeneralUris.DateProperty, DateTime.Parse(r.Value, System.Globalization.CultureInfo.InvariantCulture));
						}
						AddParticipants(uris, r, w);
						LinkToPost(uris, subjectUri, r, w);
						break;
					case "12": // post deleted
						w.AddToTriple(uris.GeneralUris.TypeProperty, uris.GeneralUris.PostDeletionType);
						if (r.MoveToAttribute("CreationDate")) {
							w.AddToTriple(uris.GeneralUris.DateProperty, DateTime.Parse(r.Value, System.Globalization.CultureInfo.InvariantCulture));
						}
						AddParticipants(uris, r, w);
						LinkToPost(uris, subjectUri, r, w);
						break;
					case "13": // post undeleted
						w.AddToTriple(uris.GeneralUris.TypeProperty, uris.GeneralUris.PostUndeletionType);
						if (r.MoveToAttribute("CreationDate")) {
							w.AddToTriple(uris.GeneralUris.DateProperty, DateTime.Parse(r.Value, System.Globalization.CultureInfo.InvariantCulture));
						}
						AddParticipants(uris, r, w);
						LinkToPost(uris, subjectUri, r, w);
						break;
					case "14": // post locked
						w.AddToTriple(uris.GeneralUris.TypeProperty, uris.GeneralUris.PostLockingType);
						if (r.MoveToAttribute("CreationDate")) {
							w.AddToTriple(uris.GeneralUris.DateProperty, DateTime.Parse(r.Value, System.Globalization.CultureInfo.InvariantCulture));
						}
						AddParticipants(uris, r, w);
						LinkToPost(uris, subjectUri, r, w);
						break;
					case "15": // post unlocked
						w.AddToTriple(uris.GeneralUris.TypeProperty, uris.GeneralUris.PostUnlockingType);
						if (r.MoveToAttribute("CreationDate")) {
							w.AddToTriple(uris.GeneralUris.DateProperty, DateTime.Parse(r.Value, System.Globalization.CultureInfo.InvariantCulture));
						}
						AddParticipants(uris, r, w);
						LinkToPost(uris, subjectUri, r, w);
						break;
					case "16": // community owned
						break;
					case "17": // post migrated superseded with 35/36
						break;
					case "18": // question merged
						break;
					case "19": // question protected
						w.AddToTriple(uris.GeneralUris.TypeProperty, uris.GeneralUris.PostProtectionType);
						if (r.MoveToAttribute("CreationDate")) {
							w.AddToTriple(uris.GeneralUris.DateProperty, DateTime.Parse(r.Value, System.Globalization.CultureInfo.InvariantCulture));
						}
						AddParticipants(uris, r, w);
						LinkToPost(uris, subjectUri, r, w);
						break;
					case "20": // question unprotected
						w.AddToTriple(uris.GeneralUris.TypeProperty, uris.GeneralUris.PostUnprotectionType);
						if (r.MoveToAttribute("CreationDate")) {
							w.AddToTriple(uris.GeneralUris.DateProperty, DateTime.Parse(r.Value, System.Globalization.CultureInfo.InvariantCulture));
						}
						AddParticipants(uris, r, w);
						LinkToPost(uris, subjectUri, r, w);
						break;
					case "21": // post disassociated
						break;
					case "22": // question unmerged
						break;
					case "24": // suggested edit applied
						break;
					case "25": // post tweeted
						break;
					case "31": // comment discussion moved to chat
						break;
					case "33": // post notice added
						break;
					case "34": // post notice removed
						break;
					case "35": // post migrated away replaces 17
						break;
					case "36": // post migrated here replaces 17
						break;
					case "37": // post merge source
						break;
					case "38": // post merge destination
						break;
					default:
						unknownPostHistoryTypeIds.RegisterUnknownValue(r.Value);
						break;
				}
			} else {
				r.MoveToElement();
				ConsoleHelper.WriteErrorLine("No PostHistoryTypeId attribute found on element {0}. Skipping element.", r.ReadOuterXml());
			}
		}
		
		private static void LinkToPost(SiteUris uris, Uri subjectUri, XmlReader r, SequentialTurtleWriter w)
		{
			if (r.MoveToAttribute("PostId")) {
				w.StartTriple(uris.CreatePostUri(r.Value));
				w.AddToTriple(uris.GeneralUris.EventProperty, subjectUri);
				w.StartTriple(subjectUri);
			} else {
				ConsoleHelper.WriteWarningLine("Orphaned post history item: {0}", subjectUri.AbsoluteUri);
			}
		}
		
		#region participants
		private class EventUserInfo
		{
			public string Id { get; set; }
			
			public string DisplayName { get; set; }
		}
		
		private class EventInfo
		{
			public IList<EventUserInfo> Voters { get; set; }
		}
		
		private static void AddParticipants(SiteUris uris, XmlReader r, SequentialTurtleWriter w)
		{
			if (r.MoveToAttribute("Text")) {
				EventInfo info;
				try {
					info = JsonConvert.DeserializeObject<EventInfo>(r.Value);
				}
				catch (Exception ex) {
					ConsoleHelper.WriteWarningLine("Invalid Json string: {0} ({2} message: {1}); skipping information.", r.Value, ex.Message, ex.GetType().FullName);
					return;
				}
				if (info.Voters != null) {
					foreach (var voter in info.Voters) {
						if (!string.IsNullOrWhiteSpace(voter.Id)) {
							w.AddToTriple(uris.GeneralUris.ParticipantProperty, uris.CreateUserUri(voter.Id));
						} else if (!string.IsNullOrWhiteSpace(voter.DisplayName)) {
							w.AddToTriple(uris.GeneralUris.ParticipantProperty, voter.DisplayName);
						}
					}
				}
			}
		}
		#endregion
	}
}
