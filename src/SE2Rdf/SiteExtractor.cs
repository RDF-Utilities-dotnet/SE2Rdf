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
using System.Text.RegularExpressions;

namespace SE2Rdf
{
	/// <summary>
	/// A helper class whose methods can extract some information about Q&amp;A sites.
	/// </summary>
	internal static class SiteExtractor
	{
		/// <summary>
		/// A regular expression used to break apart a filename from the data dump.
		/// </summary>
		private static readonly Regex siteRegex = new Regex(@"^(?<url>(?<meta>meta\.)?(?:(?<language>[a-z]{2})\.)??(?<site>[^\.]+)(?:\.stackexchange)?\.(?:com|net))(?:-[^\.]+)?\.[^\.]+$");
		
		/// <summary>
		/// Extracts some information on a Q&amp;A site based upon a filename from the data dump.
		/// </summary>
		/// <param name="filename">The filename of an archive from the data dump.</param>
		/// <returns>A <see cref="SiteInfo"/> object that provides structured access to the site information.</returns>
		/// <exception cref="ArgumentException"><paramref name="filename"/> does not match the expected format.</exception>
		public static SiteInfo GetSiteName(string filename)
		{
			Match m = siteRegex.Match(System.IO.Path.GetFileName(filename));
			if (m.Success && (m.Groups.Count >= 5)) {
				return new SiteInfo(m.Groups["url"].Value, m.Groups["site"].Value, m.Groups["meta"].Length > 0, m.Groups["language"].Value);
			} else {
				throw new ArgumentException(string.Format(System.Globalization.CultureInfo.InvariantCulture,
				                                          "No website name can be extracted from the filename {0}.",
				                                          filename));
			}
		}
	}
}
