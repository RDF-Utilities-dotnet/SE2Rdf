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

using VDS.RDF;

namespace SE2Rdf
{
	/// <summary>
	/// An object that provides URIs used throughout the exported data.
	/// </summary>
	internal sealed class GeneralUris
	{
		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		/// <param name="generalPrefix">The base prefix for all dataset-specific URIs.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generalPrefix"/> is <see langword="null"/>.</exception>
		public GeneralUris(string generalPrefix)
		{
			if (generalPrefix == null) {
				throw new ArgumentNullException("generalPrefix");
			}
			
			this.generalPrefix = new Uri("http://rdf.vis.uni-stuttgart.de/stackexchange/");
		}
		
		private readonly Uri generalPrefix;
		
		private Uri GlobalPrefix {
			get {
				return new Uri(generalPrefix.AbsoluteUri + "g/");
			}
		}
		
		public Uri OntologyPrefix {
			get {
				return new Uri(generalPrefix.AbsoluteUri + "ontology#");
			}
		}
		
		public Uri SiteDataPrefix {
			get {
				return new Uri(generalPrefix.AbsoluteUri + "s/");
			}
		}
		
		#region properties
		public Uri LabelProperty {
			get {
				return new Uri("http://www.w3.org/2000/01/rdf-schema#label"); 
			}
		}
		
		public Uri TypeProperty {
			get {
				return new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#type");
			}
		}
		
		public Uri TitleProperty {
			get {
				return new Uri("http://purl.org/dc/elements/1.1/title");
			}
		}
		
		public Uri DateProperty {
			get {
				return new Uri("http://purl.org/dc/elements/1.1/date");
			}
		}
		
		public Uri DescriptionProperty {
			get {
				return new Uri("http://purl.org/dc/elements/1.1/description");
			}
		}
		
		public Uri StackExchangeWebsiteProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "site");
			}
		}
		
		public Uri IdProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "id");
			}
		}
		
		public Uri LastEditDateProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "last-edited");
			}
		}
		
		public Uri LastActivityDateProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "last-activity");
			}
		}
		
		public Uri OwnerProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "owner"); 
			}
		}
		
		public Uri ScoreProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "score");
			}
		}
		
		public Uri ViewCountProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "viewCount");
			}
		}
		
		public Uri LanguageProperty {
			get {
				return new Uri("http://purl.org/dc/elements/1.1/language");
			}
		}
		
		#region site-specific
		public Uri IsMetaSiteProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "is-meta");
			}
		}
		
		public Uri ParentSiteProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "parent");
			}
		}
		#endregion
		
		#region question/answer-specific
		public Uri AcceptedAnswerProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "accepted");
			}
		}
		
		public Uri AnswerProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "answer");
			}
		}
		
		public Uri TagProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "tag");
			}
		}
		
		public Uri LinkProperty {
			get {
				return new Uri("http://purl.org/dc/elements/1.1/relation");
			}
		}
		
		public Uri DuplicateProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "duplicate");
			}
		}
		#endregion
		
		#region tag-specific
		public Uri TagExcerptProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "tag-excerpt");
			}
		}
		
		public Uri TagDescriptionProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "tag-description");
			}
		}
		#endregion
		
		#region user-specific
		public Uri UserNameProperty {
			get {
				return new Uri("http://xmlns.com/foaf/0.1/nick");
			}
		}
		
		public Uri ReputationProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "reputation");
			}
		}
		
		public Uri LocationProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "location");
			}
		}
		
		public Uri UpVotesProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "up-votes");
			}
		}
		
		public Uri DownVotesProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "down-votes");
			}
		}
		
		public Uri AgeProperty {
			get {
				return new Uri("http://xmlns.com/foaf/0.1/age");
			}
		}
		
		public Uri WebsiteProperty {
			get {
				return new Uri("http://xmlns.com/foaf/0.1/homepage");
			}
		}
		
		public Uri AccountProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "account");
			}
		}
		
		public Uri LastSeenProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "last-seen");
			}
		}
		#endregion
		
		#region comment-specific
		public Uri CommentProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "comment");
			}
		}
		#endregion
		
		#region vote-specific
		public Uri AcceptanceProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "acceptance");
			}
		}
		
		public Uri VoteProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "vote");
			}
		}
		
		public Uri FavoriteProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "favorite");
			}
		}
		
		public Uri PostProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "post");
			}
		}
		
		public Uri OfferedAmountProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "offered-amount");
			}
		}
		
		public Uri TransferredAmountProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "transferred-amount");
			}
		}
		
		public Uri DonorProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "donor");
			}
		}
		#endregion
		
		#region badge-specific
		public Uri BadgeProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "badge");
			}
		}
		#endregion
		
		#region post-history-specific
		public Uri CloseReasonProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "reason");
			}
		}
		
		public Uri EventProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "event");
			}
		}
		
		public Uri ParticipantProperty {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "participant");
			}
		}
		#endregion
		#endregion
		
		#region types
		public Uri QuestionType {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "Question");
			}
		}
		
		public Uri AnswerType {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "Answer");
			}
		}
		
		public Uri TagExcerptType {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "TagExcerpt");
			}
		}
		
		public Uri TagDescriptionType {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "TagDescription");
			}
		}
		
		public Uri SiteInfoType {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "SiteInfo");
			}
		}
		
		public Uri UserType {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "User");
			}
		}
		
		public Uri AccountType {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "Account");
			}
		}
		
		public Uri PersonType {
			get {
				return new Uri("http://xmlns.com/foaf/0.1/Person");
			}
		}
		
		public Uri CommentType {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "Comment");
			}
		}
		
		public Uri TagType {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "Tag");
			}
		}
		
		public Uri AcceptanceType {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "Acceptance");
			}
		}
		
		public Uri UpVoteType {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "UpVote");
			}
		}
		
		public Uri DownVoteType {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "DownVote");
			}
		}
		
		public Uri FavoriteType {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "Favorite");
			}
		}
		
		public Uri BadgeType {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "Badge");
			}
		}
		
		public Uri AssignedBadgeType {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "AssignedBadge");
			}
		}
		
		public Uri PostClosureType {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "Closure");
			}
		}
		
		public Uri PostReopeningType {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "Reopening");
			}
		}
		
		public Uri PostDeletionType {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "Deletion");
			}
		}
		
		public Uri PostUndeletionType {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "Undeletion");
			}
		}
		
		public Uri PostLockingType {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "Locking");
			}
		}
		
		public Uri PostUnlockingType {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "Unlocking");
			}
		}
		
		public Uri PostProtectionType {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "Protection");
			}
		}
		
		public Uri PostUnprotectionType {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "Unprotection");
			}
		}
		
		public Uri StartOfBountyType {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "StartOfBounty");
			}
		}
		
		public Uri EndOfBountyType {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "EndOfBounty");
			}
		}
		
		public Uri CloseReasonType {
			get {
				return new Uri(OntologyPrefix.AbsoluteUri + "CloseReason");
			}
		}
		#endregion
		
		#region network-wide entities
		public Uri CreateAccountUri(string id)
		{
			return new Uri(GlobalPrefix, "account" + id);
		}
		
		private Uri SitePrefix {
			get {
				return new Uri(GlobalPrefix, "site/");
			}
		}
		
		private readonly string siteShortPrefix = "site";
		
		public Uri CreateSiteUri(SiteInfo info)
		{
			if (info == null) {
				throw new ArgumentNullException("info");
			}
			
			return new Uri(SitePrefix, info.FormalName);
		}
		#endregion
		
		#region constants
		#region close reasons
		private Uri CloseReasonPrefix {
			get {
				return new Uri(GlobalPrefix, "close-reason/");
			}
		}
		
		public Uri DuplicateCloseReason {
			get {
				return new Uri(CloseReasonPrefix, "duplicate");
			}
		}
		
		public Uri OffTopicCloseReason {
			get {
				return new Uri(CloseReasonPrefix, "off-topic");
			}
		}
		
		public Uri SubjectiveCloseReason {
			get {
				return new Uri(CloseReasonPrefix, "subjective");
			}
		}
		
		public Uri NotAQuestionCloseReason {
			get {
				return new Uri(CloseReasonPrefix, "not-a-question");
			}
		}
		
		public Uri TooLocalizedCloseReason {
			get {
				return new Uri(CloseReasonPrefix, "too-localized");
			}
		}
		
		public Uri GeneralReferenceCloseReason {
			get {
				return new Uri(CloseReasonPrefix, "general-reference");
			}
		}
		
		public Uri NoiseCloseReason {
			get {
				return new Uri(CloseReasonPrefix, "noise");
			}
		}
		
		public Uri UnclearCloseReason {
			get {
				return new Uri(CloseReasonPrefix, "unclear");
			}
		}
		
		public Uri TooBroadCloseReason {
			get {
				return new Uri(CloseReasonPrefix, "too-broad");
			}
		}
		#endregion
		#endregion
		
		public INamespaceMapper CreateNamespaceMapper()
		{
			var result = new NamespaceMapper();
			result.AddNamespace("dc", new Uri("http://purl.org/dc/elements/1.1/"));
			result.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));
			
			result.AddNamespace("se", GlobalPrefix);
			result.AddNamespace(siteShortPrefix, SitePrefix);
			result.AddNamespace("se-owl", OntologyPrefix);
			result.AddNamespace("cr", CloseReasonPrefix);
			return result;
		}
	}
}
