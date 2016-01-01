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
using System.Collections.Generic;
using System.IO;
using VDS.RDF;

namespace SE2Rdf
{
	partial class GlobalInformationConverter
	{
		private static void WriteOntology(GeneralUris generalUris, string destDir, VDS.RDF.INamespaceMapper nsMapper)
		{
			ConsoleHelper.WriteMilestone("Writing ontology ...");
			
			using (var tempNsMapper = new NamespaceMapper(false)) {
				tempNsMapper.Import(nsMapper);
				tempNsMapper.AddNamespace("owl", new Uri(NamespaceMapper.OWL));
				
				using (var destWriter = new SequentialTurtleWriter(File.CreateText(Path.Combine(destDir, "_ontology.ttl")), tempNsMapper)) {
					WriteOntologyDefinitions(generalUris, destWriter);
					
					GlobalData.UpdateStats(destWriter);
				}
			}
			Console.WriteLine(" done.");
		}
		
		private static readonly Uri typeUri = new Uri(NamespaceMapper.RDF + "type");
		
		private static readonly Uri subClassOfUri = new Uri(NamespaceMapper.RDFS + "subClassOf");
		
		private static readonly Uri subPropertyOfUri = new Uri(NamespaceMapper.RDFS + "subPropertyOf");
		
		private static readonly Uri restrictionUri = new Uri(NamespaceMapper.OWL + "Restriction");
		
		private static void WriteOntologyDefinitions(GeneralUris generalUris, SequentialTurtleWriter w)
		{
			// ontology metadata
			
			string ontologyUri = generalUris.OntologyPrefix.AbsoluteUri ?? "";
			if (ontologyUri.Length > 0) {
				switch (ontologyUri[ontologyUri.Length - 1]) {
					case '/':
					case '#':
						ontologyUri = ontologyUri.Substring(0, ontologyUri.Length - 1);
						break;
				}
			}
			w.StartTriple(new Uri(ontologyUri));
			w.AddToTriple(generalUris.TypeProperty, new Uri(NamespaceMapper.OWL + "Ontology"));
			w.AddToTriple(generalUris.TitleProperty, "SE2Rdf Output");
			w.AddToTriple(generalUris.DateProperty, DateTime.Now);
			w.AddToTriple(new Uri(NamespaceMapper.OWL + "imports"), new Uri("http://purl.org/dc/elements/1.1")); // TODO: is this correct/requierd?
			
			// types
			
			Uri postType = new Uri(generalUris.OntologyPrefix.AbsoluteUri + "Post");
			WriteClassDecl(w, generalUris.QuestionType, "Question");
			w.AddToTriple(subClassOfUri, postType);
			w.AddAnonymousToTriple(subClassOfUri);
			// TODO: does not seem to work yet in VOWL => test in Protege
			//w.AddToTriple(generalUris.TypeProperty, restrictionUri);
			w.AddToTriple(new Uri(NamespaceMapper.OWL + "onProperty"), generalUris.TitleProperty);
			w.AddToTriple(new Uri(NamespaceMapper.OWL + "cardinality"), 1);
			w.FinishAnonymousNode();
			
			WriteClassDecl(w, generalUris.AnswerType, "Answer");
			w.AddToTriple(subClassOfUri, postType);
			
			Uri tagWikiType = new Uri(generalUris.OntologyPrefix.AbsoluteUri + "TagWiki");
			WriteClassDecl(w, generalUris.TagExcerptType, "Tag Excerpt");
			w.AddToTriple(subClassOfUri, tagWikiType);
			WriteClassDecl(w, generalUris.TagDescriptionType, "Tag Description");
			w.AddToTriple(subClassOfUri, tagWikiType);
			
			WriteClassDecl(w, generalUris.SiteInfoType, "Q&A Site");
			WriteClassDecl(w, generalUris.UserType, "Site-specific User");
			
			WriteClassDecl(w, generalUris.AccountType, "Account");
			w.AddToTriple(subClassOfUri, generalUris.PersonType);
			
			WriteClassDecl(w, generalUris.CommentType, "Comment");
			WriteClassDecl(w, generalUris.TagType, "Tag");
			WriteClassDecl(w, generalUris.AcceptanceType, "Acceptance");
			
			Uri voteType = new Uri(generalUris.OntologyPrefix.AbsoluteUri + "Vote");
			WriteClassDecl(w, voteType, "Vote");
			WriteClassDecl(w, generalUris.UpVoteType, "Upvote");
			w.AddToTriple(subClassOfUri, voteType);
			WriteClassDecl(w, generalUris.DownVoteType, "Downvote");
			w.AddToTriple(subClassOfUri, voteType);
			
			WriteClassDecl(w, generalUris.FavoriteType, "Favorite");
			WriteClassDecl(w, generalUris.BadgeType, "Badge");
			WriteClassDecl(w, generalUris.AssignedBadgeType, "Assigned Badge");
			
			Uri postActionType = new Uri(generalUris.OntologyPrefix.AbsoluteUri + "PostAction");
			WriteClassDecl(w, postActionType, "Post Action");
			WriteClassDecl(w, generalUris.PostClosureType, "Closure");
			w.AddToTriple(subClassOfUri, postActionType);
			WriteClassDecl(w, generalUris.PostReopeningType, "Reopening");
			w.AddToTriple(subClassOfUri, postActionType);
			WriteClassDecl(w, generalUris.PostDeletionType, "Deletion");
			w.AddToTriple(subClassOfUri, postActionType);
			WriteClassDecl(w, generalUris.PostUndeletionType, "Undeletion");
			w.AddToTriple(subClassOfUri, postActionType);
			WriteClassDecl(w, generalUris.PostLockingType, "Locking");
			w.AddToTriple(subClassOfUri, postActionType);
			WriteClassDecl(w, generalUris.PostUnlockingType, "Unlocking");
			w.AddToTriple(subClassOfUri, postActionType);
			WriteClassDecl(w, generalUris.PostProtectionType, "Protection");
			w.AddToTriple(subClassOfUri, postActionType);
			WriteClassDecl(w, generalUris.PostUnprotectionType, "Unprotection");
			w.AddToTriple(subClassOfUri, postActionType);
			
			WriteClassDecl(w, generalUris.StartOfBountyType, "Start of Bounty");
			WriteClassDecl(w, generalUris.EndOfBountyType, "End of Bounty");
			WriteClassDecl(w, generalUris.CloseReasonType, "Close Reason");
			
			// properties
			
			WritePropDecl(w, generalUris.StackExchangeWebsiteProperty, true,
			              new[] { postType, generalUris.TagType, generalUris.UserType, generalUris.BadgeType },
			              new[] { generalUris.SiteInfoType });
			WritePropDecl(w, generalUris.ScoreProperty, false,
			              new[] { generalUris.CommentType, postType },
			              new[] { new Uri(NamespaceMapper.XMLSCHEMA + "integer") });
			WritePropDecl(w, generalUris.OwnerProperty, true,
			              new[] { generalUris.CommentType, postType, tagWikiType },
			              new[] { generalUris.UserType });
			WritePropDecl(w, generalUris.CloseReasonProperty, true,
			              new[] { generalUris.PostClosureType },
			              new[] { generalUris.CloseReasonType });
			WritePropDecl(w, generalUris.ParticipantProperty, true,
			              new[] { postActionType },
			              new[] { generalUris.UserType });
			WritePropDecl(w, generalUris.CommentProperty, true,
			              new[] { postType },
			              new[] { generalUris.CommentType });
			WritePropDecl(w, generalUris.ViewCountProperty, false,
			              new[] { generalUris.QuestionType, generalUris.UserType, generalUris.AccountType },
			              new[] { new Uri(NamespaceMapper.XMLSCHEMA + "integer") });
			WritePropDecl(w, generalUris.TagProperty, true,
			              new[] { postType }, // TODO: verify!
			              new[] { generalUris.TagType });
			WritePropDecl(w, generalUris.AnswerProperty, true,
			              new[] { generalUris.QuestionType, generalUris.EndOfBountyType },
			              new[] { generalUris.AnswerType });
			
			WritePropDecl(w, generalUris.AcceptedAnswerProperty, true,
			              new[] { generalUris.QuestionType },
			              new[] { generalUris.AnswerType });
			w.AddToTriple(subPropertyOfUri, generalUris.AnswerProperty);
			
			WritePropDecl(w, generalUris.LastEditDateProperty, false,
			              new[] { postType, tagWikiType },
			              new[] { new Uri(NamespaceMapper.XMLSCHEMA + "dateTime") });
			WritePropDecl(w, generalUris.DuplicateProperty, true,
			              new[] { generalUris.QuestionType },
			              new[] { generalUris.QuestionType },
			              new Uri(NamespaceMapper.OWL + "IrreflexiveProperty"));
			WritePropDecl(w, generalUris.EventProperty, true,
			              new[] { postType },
			              new[] { postActionType });
			WritePropDecl(w, generalUris.TagExcerptProperty, true,
			              new[] { generalUris.TagType },
			              new[] { generalUris.TagExcerptType });
			WritePropDecl(w, generalUris.TagDescriptionProperty, true,
			              new[] { generalUris.TagType },
			              new[] { generalUris.TagDescriptionType });
			WritePropDecl(w, generalUris.BadgeProperty, true,
			              new[] { generalUris.UserType },
			              new[] { generalUris.BadgeType });
			WritePropDecl(w, generalUris.ReputationProperty, false,
			              new[] { generalUris.UserType },
			              new[] { new Uri(NamespaceMapper.XMLSCHEMA + "integer") });
			WritePropDecl(w, generalUris.UpVotesProperty, false,
			              new[] { generalUris.UserType },
			              new[] { new Uri(NamespaceMapper.XMLSCHEMA + "integer") });
			WritePropDecl(w, generalUris.DownVotesProperty, false,
			              new[] { generalUris.UserType },
			              new[] { new Uri(NamespaceMapper.XMLSCHEMA + "integer") });
			WritePropDecl(w, generalUris.AccountProperty, true,
			              new[] { generalUris.UserType },
			              new[] { generalUris.AccountType });
			w.AddToTriple(generalUris.TypeProperty, new Uri(NamespaceMapper.OWL + "FunctionalProperty"));
			WritePropDecl(w, generalUris.LastSeenProperty, false,
			              new[] { generalUris.UserType },
			              new[] { new Uri(NamespaceMapper.XMLSCHEMA + "dateTime") });
			WritePropDecl(w, generalUris.FavoriteProperty, true,
			              new[] { generalUris.UserType },
			              new[] { generalUris.QuestionType });
			WritePropDecl(w, generalUris.IsMetaSiteProperty, false,
			              new[] { generalUris.SiteInfoType },
			              new[] { new Uri(NamespaceMapper.XMLSCHEMA + "boolean") });
			WritePropDecl(w, generalUris.ParentSiteProperty, true,
			              new[] { generalUris.SiteInfoType },
			              new[] { generalUris.SiteInfoType },
			              new Uri(NamespaceMapper.OWL + "IrreflexiveProperty"));
			WritePropDecl(w, generalUris.PostProperty, true,
			              new[] { generalUris.StartOfBountyType },
			              new[] { generalUris.QuestionType });
			WritePropDecl(w, generalUris.DonorProperty, true,
			              new[] { generalUris.StartOfBountyType },
			              new[] { generalUris.UserType });
			WritePropDecl(w, generalUris.OfferedAmountProperty, false,
			              new[] { generalUris.StartOfBountyType },
			              new[] { new Uri(NamespaceMapper.XMLSCHEMA + "integer") });
			WritePropDecl(w, generalUris.TransferredAmountProperty, false,
			              new[] { generalUris.EndOfBountyType },
			              new[] { new Uri(NamespaceMapper.XMLSCHEMA + "integer") });
		}
		
		private static void WriteClassDecl(SequentialTurtleWriter w, Uri classUri, string className)
		{
			w.StartTriple(classUri);
			w.AddToTriple(typeUri, new Uri(NamespaceMapper.OWL + "Class"));
			w.AddToTriple(new Uri(NamespaceMapper.RDFS + "label"), className);
		}
		
		private static void WritePropDecl(SequentialTurtleWriter w, Uri propUri, bool isObjectProperty, IList<Uri> domain, IList<Uri> range, Uri propTypeUri = null)
		{
			w.StartTriple(propUri);
			if (propTypeUri == null) {
				w.AddToTriple(typeUri, new Uri(NamespaceMapper.OWL + (isObjectProperty ? "ObjectProperty" : "DatatypeProperty")));
			} else {
				w.AddToTriple(typeUri, propTypeUri);
			}
			WriteUnionPropIfNecessary(w, new Uri(NamespaceMapper.RDFS + "domain"), domain);
			WriteUnionPropIfNecessary(w, new Uri(NamespaceMapper.RDFS + "range"), range);
		}
		
		private static void WriteUnionPropIfNecessary(SequentialTurtleWriter w, Uri propUri, IList<Uri> objects)
		{
			if (objects.Count > 0) {
				if (objects.Count == 1) {
					w.AddToTriple(propUri, objects[0]);
				} else {
					w.AddAnonymousToTriple(propUri);
					w.AddToTriple(typeUri, new Uri(NamespaceMapper.OWL + "Class"));
					w.AddToTriple(new Uri(NamespaceMapper.OWL + "unionOf"), true, objects);
					w.FinishAnonymousNode();
				}
			}
		}
	}
}
