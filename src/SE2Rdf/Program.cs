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
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

using SharpCompress.Archive;
using SharpCompress.Archive.SevenZip;
using SharpCompress.Reader;

namespace SE2Rdf
{
	class Program
	{
		private static readonly string BaseDir = Environment.CurrentDirectory;
		
		private static readonly string DefaultFileUrl = "https://archive.org/download/stackexchange/stackexchange_files.xml";
		
		public static void Main(string[] args)
		{
			var culture = new System.Globalization.CultureInfo("en-US");
			System.Threading.Thread.CurrentThread.CurrentCulture = culture;
			System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
			
			try {
				using (var parser = new CommandLine.Parser()) {
					if (!parser.ParseArguments(args, GlobalData.Options)) {
						Console.WriteLine(GlobalData.Options.GetUsage());
						return;
					}
				}
				
				if (GlobalData.Options.MaxFileCount < 1) {
					ConsoleHelper.WriteErrorLine("As the maximum file count is set to {0}, no files can be imported.",
					                             GlobalData.Options.MaxFileCount);
					return;
				}
				if (GlobalData.Options.Files.Count < 1) {
					ConsoleHelper.WriteInfoLine("No input file specified. Using default URL {0}.",
					                            DefaultFileUrl);
					GlobalData.Options.Files.Add(DefaultFileUrl);
				}
				
				if (!string.IsNullOrWhiteSpace(GlobalData.Options.SiteNamePattern)) {
					try {
						GlobalData.SiteNamePattern = new Regex(GlobalData.Options.SiteNamePattern);
					}
					catch (ArgumentException) {
						ConsoleHelper.WriteErrorLine("Invalid regular expression: \"{0}\"",
						                             GlobalData.Options.SiteNamePattern);
						return;
					}
				}
				
				GeneralUris generalUris = new GeneralUris(GlobalData.Options.GeneralPrefix);
				
				try {
					Uri filelist = new Uri(GlobalData.Options.Files[0]);
					
					RetrieveData(generalUris, filelist);
				}
				catch (UriFormatException) {
					Console.WriteLine("Invalid filelist URL:");
					throw;
				}
			}
			catch (Exception ex) {
				Console.WriteLine(ex.ToString());
			}
		}
		
		/// <summary>
		/// Checks whether a given site is going to be included in the conversion based upon filter settings.
		/// </summary>
		/// <param name="site">The site to check.
		///   This must not be <see langword="null"/>.</param>
		/// <returns>A value that indicates whether to include the site.</returns>
		private static bool IncludeSite(SiteInfo site)
		{
			if (GlobalData.SiteNamePattern != null) {
				if (!GlobalData.SiteNamePattern.IsMatch(site.FormalName)) {
					return false;
				}
			}
			
			if (!string.IsNullOrWhiteSpace(GlobalData.Options.Language)) {
				string lng = GlobalData.Options.Language.ToLowerInvariant();
				if (lng == "en") {
					if (!site.IsEnglishSite) {
						return false;
					}
				} else {
					if (site.IsEnglishSite) {
						return false;
					} else {
						if (site.Language.ToLowerInvariant() != lng) {
							return false;
						}
					}
				}
			}
			
			return true;
		}
		
		/// <summary>
		/// Downloads and converts the data.
		/// </summary>
		/// <param name="generalUris">An object that provides general URIs used in the exported dataset.</param>
		/// <param name="filelist">The URL of a filelist Xml file.</param>
		/// <exception cref="ArgumentNullException">Any of the arguments is <see langword="null"/>.</exception>
		private static void RetrieveData(GeneralUris generalUris, Uri filelist)
		{
			if (generalUris == null) {
				throw new ArgumentNullException("generalUris");
			}
			if (filelist == null) {
				throw new ArgumentNullException("filelist");
			}
			
			string tempDir = Path.Combine(BaseDir, "tmp");
			string destDir = Path.Combine(BaseDir, "rdf");
			
			Directory.CreateDirectory(tempDir);
			Directory.CreateDirectory(destDir);
			
			DateTime startTime = DateTime.Now;
			ConsoleHelper.WriteInfoLine("Current time: {0:yyyy-MM-dd HH:mm:ss}", startTime);
			
			if (!GlobalData.Options.OntologyOnly) {
				ConsoleHelper.WriteMilestone("Downloading files list ...");
				using (var client = new WebClient()) {
					client.DownloadFile(filelist, Path.Combine(tempDir, "files.xml"));
				}
				Console.WriteLine(" done.");
				
				var files = LoadFilesList(Path.Combine(tempDir, "files.xml")).OrderBy(f => f).ToArray();
				ConsoleHelper.WriteInfoLine("{0} file(s) in list, totalling to a compressed size of {1:F1} GB.",
				                            files.Length, (double)files.Sum(f => f.Size) / 1024 / 1024 / 1024);
				
				int processedFilesCount = 0;
				foreach (var f in files) {
					SiteInfo siteInfo;
					try {
						siteInfo = f.RetrieveSiteInfo();
					}
					catch (ArgumentException ex) {
						ConsoleHelper.WriteErrorLine("Skipping file {0}, as it cannot be associated with a website.\n{1}", f, ex);
						continue;
					}
					
					if (IncludeSite(siteInfo)) {
						GlobalData.Sites[siteInfo.Id] = siteInfo;
						
						if (!GlobalData.Options.SiteListOnly) {
							string fn = f.Download(filelist, tempDir);
							
							string[] rawFiles = null;
							switch (Path.GetExtension(fn)) {
								case ".7z":
									rawFiles = ExtractSevenZipArchive(fn);
									break;
								default:
									ConsoleHelper.WriteWarningLine("File {0} has an unknown file extension.", fn);
									break;
							}
							
							if (rawFiles != null) {
								ConsoleHelper.WriteInfoLine("{0} file(s) extracted.", rawFiles.Length);
								
								foreach (var rawFile in rawFiles) {
									Converter.Convert(generalUris, rawFile, destDir, siteInfo);
								}
							}
						}
						
						processedFilesCount++;
						if (processedFilesCount >= GlobalData.Options.MaxFileCount) {
							break;
						}
					}
				}
			}
			
			GlobalInformationConverter.Convert(generalUris, tempDir, filelist, destDir);
			
			if (!GlobalData.Options.KeepTemporaryFiles) {
				Console.Write("Removing temporary files ... ");
				try {
					Directory.Delete(tempDir, true);
					Console.WriteLine(" done.");
				}
				catch {
					ConsoleHelper.WriteErrorLine("Please remove the directory {0} manually.", tempDir);
				}
			}
			
			Console.WriteLine();
			GlobalData.PrintStats();
			
			DateTime endTime = DateTime.Now;
			ConsoleHelper.WriteInfoLine("Current time: {0:yyyy-MM-dd HH:mm:ss}", endTime);
			ConsoleHelper.WriteInfoLine("Total duration: {0}", endTime - startTime);
		}
		
		/// <summary>
		/// Loads the complete list of available content files.
		/// </summary>
		/// <param name="listName">The filename of the list.</param>
		/// <returns>An enumeration of <see cref="KnownFile">file info objects</see>.</returns>
		private static IEnumerable<KnownFile> LoadFilesList(string listName)
		{
			using (var fs = File.OpenRead(listName)) {
				using (var reader = XmlReader.Create(fs)) {
					while (reader.Read()) {
						switch (reader.NodeType) {
							case XmlNodeType.Element:
								if (reader.LocalName == "file") {
									using (var subR = reader.ReadSubtree()) {
										subR.Read();
										var f = KnownFile.RecognizeFile(subR);
										if (f != null) {
											yield return f;
										}
									}
								}
								break;
						}
					}
				}
			}
		}
		
		/// <summary>
		/// Extracts a 7z archive and returns a list of extracted filenames.
		/// </summary>
		/// <param name="filename">The name of the 7z file.</param>
		/// <returns>The list of file paths to the extracted files.</returns>
		private static string[] ExtractSevenZipArchive(string filename)
		{
			string dir = filename + "-files";
			Directory.CreateDirectory(dir);
			
			var result = new List<string>();
			
			using (var archive = SevenZipArchive.Open(filename)) {
				foreach (var entry in archive.Entries) {
					if (!entry.IsDirectory) {
						Console.Write("Extracting {0} ...", entry.Key);
						var singleFile = Path.Combine(dir, entry.Key);
						using (var fs = File.Create(singleFile)) {
							entry.WriteTo(fs);
						}
						result.Add(singleFile);
						Console.WriteLine(" done.");
					}
				}
			}
			
			return result.ToArray();
		}
	}
}