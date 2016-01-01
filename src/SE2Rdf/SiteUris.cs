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
	internal class SiteUris
	{
		public SiteUris(GeneralUris generalUris, SiteInfo site)
		{
			if (generalUris == null) {
				throw new ArgumentNullException("generalUris");
			}
			if (site == null) {
				throw new ArgumentNullException("site");
			}
			
			this.generalUris = generalUris;
			this.site = site;
			
			stackExchangeSite = generalUris.CreateSiteUri(site);
			BaseUri = new Uri(generalUris.SiteDataPrefix, site.Name + (site.IsMetaSite ? "-meta" : "") + "/");
			
			tagPrefix = new Uri(BaseUri, "tag/");
			badgePrefix = new Uri(BaseUri, "badge/");
		}
		
		private readonly GeneralUris generalUris;
		
		public GeneralUris GeneralUris {
			get {
				return generalUris;
			}
		}
		
		private readonly SiteInfo site;
		
		public SiteInfo Site {
			get {
				return site;
			}
		}
		
		public readonly Uri BaseUri;
		
		public Uri CreatePostUri(string id)
		{
			return new Uri(BaseUri, "post" + id);
		}
		
		public Uri CreateUserUri(string id)
		{
			return new Uri(BaseUri, "user" + id);
		}
		
		public Uri CreateCommentUri(string id)
		{
			return new Uri(BaseUri, "comment" + id);
		}
		
		private static string EscapeResourceName(string tagName)
		{
			var result = new System.Text.StringBuilder();
			foreach (char ch in tagName) {
				switch (ch) {
					case '/':
						result.Append("<sl");
						break;
					case '#':
						result.Append("<sh");
						break;
					case '&':
						result.Append("<am");
						break;
					case '~':
						result.Append("<t");
						break;
					case '.':
						result.Append("<pp");
						break;
					case '-':
						result.Append("<m");
						break;
					case '!':
						result.Append("<ex");
						break;
					case '$':
						result.Append("<d");
						break;
					case '\'':
						result.Append("<q1");
						break;
					case '"':
						result.Append("<q2");
						break;
					case '(':
						result.Append("<bo");
						break;
					case ')':
						result.Append("<bc");
						break;
					case '*':
						result.Append("<as");
						break;
					case '+':
						result.Append("<pl");
						break;
					case ',':
						result.Append("<c");
						break;
					case ';':
						result.Append("<sc");
						break;
					case '=':
						result.Append("<eq");
						break;
					case '?':
						result.Append("<qm");
						break;
					case '@':
						result.Append("<at");
						break;
					case '%':
						result.Append("<pc");
						break;
					case '_':
						result.Append("<ul");
						break;
					case ' ':
						result.Append("<<");
						break;
					case '<':
						result.Append("<l<");
						break;
					case '>':
						result.Append("<l>");
						break;
					default:
						result.Append(ch);
						break;
				}
			}
			return result.ToString();
		}
		
		private readonly Uri tagPrefix;
		
		public Uri CreateTagUri(string tagName)
		{
			return new Uri(tagPrefix, EscapeResourceName(tagName));
		}
		
		private readonly Uri badgePrefix;
		
		public Uri CreateBadgeUri(string badgeName)
		{
			return new Uri(badgePrefix, EscapeResourceName(badgeName));
		}
		
		public Uri CreateAssignedBadgeUri(string id)
		{
			return new Uri(BaseUri, "badge" + id);
		}
		
		public Uri CreateVoteUri(string id)
		{
			return new Uri(BaseUri, "vote" + id);
		}
		
		private readonly Uri stackExchangeSite;
		
		public void LinkToSite(SequentialTurtleWriter writer)
		{
			if (writer == null) {
				throw new ArgumentNullException("writer");
			}
			
			writer.AddToTriple(GeneralUris.StackExchangeWebsiteProperty, stackExchangeSite);
		}
		
		public Uri CreatePostHistoryUri(string id)
		{
			return new Uri(BaseUri, "ph" + id);
		}
		
		public INamespaceMapper CreateNamespaceMapper()
		{
			var result = new NamespaceMapper();
			result.Import(generalUris.CreateNamespaceMapper());
			result.AddNamespace("se", BaseUri);
			result.AddNamespace("tag", tagPrefix);
			result.AddNamespace("badge", badgePrefix);
			return result;
		}
	}
}
