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
using System.Linq;
using System.Text.RegularExpressions;

namespace SE2Rdf
{
	/// <summary>
	/// Stores some global data for the application.
	/// </summary>
	internal static class GlobalData
	{
		#region options
		/// <summary>
		/// The command line options.
		/// </summary>
		/// <seealso cref="Options"/>
		private static readonly ProgramOptions options = new ProgramOptions();
		
		/// <summary>
		/// The command line options.
		/// </summary>
		public static ProgramOptions Options {
			get {
				return options;
			}
		}
		
		/// <summary>
		/// A regular expression that can restrict sites to convert.
		/// </summary>
		/// <seealso cref="SiteNamePattern"/>
		private static Regex siteNamePattern;
		
		/// <summary>
		/// A regular expression that can restrict sites to convert.
		/// </summary>
		public static Regex SiteNamePattern {
			get {
				return siteNamePattern;
			}
			set {
				siteNamePattern = value;
			}
		}
		#endregion
		
		#region amount of data
		/// <summary>
		/// The total number of created triples.
		/// </summary>
		/// <seealso cref="IncTripleCount"/>
		private static long tripleCount;
		
		/// <summary>
		/// Increases the total number of created triples by the given amount.
		/// </summary>
		/// <param name="by">The number of triples to add.</param>
		public static void IncTripleCount(long by)
		{
			tripleCount += by;
		}
		
		/// <summary>
		/// The total number of written bytes.
		/// </summary>
		/// <seealso cref="IncByteCount"/>
		private static long byteCount;
		
		/// <summary>
		/// Increases the total number of written bytes by the given amount.
		/// </summary>
		/// <param name="by">The number of bytes to add.</param>
		public static void IncByteCount(long by)
		{
			byteCount += by;
		}
		
		/// <summary>
		/// Updates the total number of written bytes by the length of a Turtle document.
		/// </summary>
		/// <param name="writer">The Turtle writer.</param>
		/// <exception cref="ArgumentNullException"><paramref name="writer"/> is <see langword="null"/>.</exception>
		public static void UpdateStats(SequentialTurtleWriter writer)
		{
			if (writer == null) {
				throw new ArgumentNullException("writer");
			}
			
			long bytes = writer.GetCurrentStreamSize();
			long triples = writer.GetCurrentTripleCount();
			ConsoleHelper.WriteSuccessLine("{0:F1} MB written; {1} triple(s) created.", (double)bytes / 1024 / 1024, triples);
			byteCount += bytes;
			tripleCount += triples;
		}
		#endregion
		
		#region sites
		/// <summary>
		/// A complete dictionary of known Q&amp;A sites.
		/// </summary>
		/// <seealso cref="Sites"/>
		private static readonly Dictionary<string, SiteInfo> sites = new Dictionary<string, SiteInfo>();
		
		/// <summary>
		/// A complete dictionary of known Q&amp;A sites.
		/// </summary>
		public static IDictionary<string, SiteInfo> Sites {
			get {
				return sites;
			}
		}
		
		/// <summary>
		/// Stores badge names per site.
		/// </summary>
		private static readonly Dictionary<SiteInfo, ISet<string>> badgesPerSite = new Dictionary<SiteInfo, ISet<string>>();
		
		/// <summary>
		/// Registers a badge name for a given site.
		/// </summary>
		/// <param name="site">The site.</param>
		/// <param name="badgeName">The human-readable name of the badge.</param>
		/// <exception cref="ArgumentNullException">Any of the arguments is <see langword="null"/>.</exception>
		/// <remarks>
		/// <para>This method registers a given batch by its human-readable name for a given site.
		///   Repeated attempts to register a badge with the same name will be ignored, hence calling this method one or more times for each badge is safe.</para>
		/// </remarks>
		public static void RegisterBadge(SiteInfo site, string badgeName)
		{
			if (site == null) {
				throw new ArgumentNullException("site");
			}
			if (badgeName == null) {
				throw new ArgumentNullException("badgeName");
			}
			
			ISet<string> badgeNames;
			if (!badgesPerSite.TryGetValue(site, out badgeNames)) {
				badgeNames = new HashSet<string>();
				badgesPerSite[site] = badgeNames;
			}
			badgeNames.Add(badgeName);
		}
		
		/// <summary>
		/// Returns all badges for all sites.
		/// </summary>
		/// <returns>The complete enumeration of badges.</returns>
		public static IEnumerable<Tuple<SiteInfo, IEnumerable<string>>> GetBadgesPerSite()
		{
			return badgesPerSite.Select(pair => new Tuple<SiteInfo, IEnumerable<string>>(pair.Key, pair.Value));
		}
		#endregion
		
		#region users
		private static readonly HashSet<string> accountIds = new HashSet<string>();
		
		public static ISet<string> AccountIds {
			get {
				return accountIds;
			}
		}
		#endregion
		
		/// <summary>
		/// Prints the complete statistics of the operation.
		/// </summary>
		public static void PrintStats()
		{
			Console.ForegroundColor = ConsoleColor.DarkCyan;
			Console.WriteLine("Total number of triples: {0:#,#} ({1:F1} GB)",
			                  tripleCount,
			                  (double)byteCount / 1024 / 1024 / 1024);
			Console.ResetColor();
		}
	}
}
