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

using CommandLine;
using CommandLine.Text;

namespace SE2Rdf
{
	/// <summary>
	/// The container for command line options of the application.
	/// </summary>
	internal class ProgramOptions
	{
		/// <summary>
		/// Indicates whether temporary files are kept after the conversion.
		/// </summary>
		[Option('k', "keep-temp-files", DefaultValue = false, HelpText = "If set to true, the downloaded data files are not removed upon completion.")]
		public bool KeepTemporaryFiles { get; set; }
		
		/// <summary>
		/// Indicates whether only the list of sites should be created.
		/// </summary>
		[Option('l', "sitelist-only", HelpText = "If set, only the list of sites is generated and no other data is downloaded or converted.")]
		public bool SiteListOnly { get; set; }
		
		/// <summary>
		/// Indicates the maximum number of data files to convert.
		/// </summary>
		[Option('m', "maxFiles", DefaultValue = int.MaxValue, HelpText = "Maximum number of data files to download.")]
		public int MaxFileCount { get; set; }
		
		/// <summary>
		/// Indicates whether only the ontology should be generated.
		/// </summary>
		[Option('o', "ontology-only", DefaultValue = false, HelpText = "If text, any downloading of information is skipped, and only the ontology is generated.")]
		public bool OntologyOnly { get; set; }
		
		/// <summary>
		/// Indicates the base prefix for all dataset-specific IRIs.
		/// </summary>
		[Option('p', "prefix", DefaultValue = "http://rdf.vis.uni-stuttgart.de/stackexchange/", HelpText = "Sets the namespace prefix to use in all generated IRIs.")]
		public string GeneralPrefix { get; set; }
		
		/// <summary>
		/// Indicates a regular expression that will be used to filter the sites to convert.
		/// </summary>
		[Option('s', "sites", HelpText = "A regular expression that restricts sites to convert.")]
		public string SiteNamePattern { get; set; }
		
		/// <summary>
		/// Indicates whether the full history of all data should be converted.
		/// </summary>
		[Option('t', "fullTimeInfo", HelpText = "Add additional entities to inform about the temporal sequence of events.")]
		public bool FullTimeInfo { get; set; }
		
		/// <summary>
		/// Indicates the maximum number of malformed IRIs to show for each file in the error output.
		/// </summary>
		[Option("invalid-iris", DefaultValue = 3, HelpText = "Determines how many malformed IRIs to show per file for debugging.")]
		public int MaxDisplayedMalformedIris { get; set; }
		
		/// <summary>
		/// A language tag that can be used to filter the selection of sites to convert.
		/// </summary>
		[Option("language", HelpText = "A language tag that restricts Q&A sites to convert.")]
		public string Language { get; set; }
		
		/// <summary>
		/// Indicates the URL of the file list.
		/// </summary>
		[ValueList(typeof(List<string>), MaximumElements = 1)]
		public IList<string> Files { get; set; }
		
		/// <summary>
		/// Returns a help text on the command line interface.
		/// </summary>
		/// <returns>The help text.</returns>
		[HelpOption]
		public string GetUsage()
		{
			var text = HelpText.AutoBuild(this);
			return text.ToString();
		}
	}
}
