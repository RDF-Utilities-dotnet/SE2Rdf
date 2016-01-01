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
using System.Net;
using System.Xml;
using System.IO;
using System.Security.Cryptography;
using System.Linq;

namespace SE2Rdf
{
	/// <summary>
	/// Represents a downloadable file from the data dump.
	/// </summary>
	internal class KnownFile : IComparable<KnownFile>
	{
		/// <summary>
		/// Initializes a <see cref="KnownFile"/> instance based upon data read from an Xml-based file list.
		/// </summary>
		/// <param name="reader">An Xml reader that points to an entry in a file list.</param>
		/// <returns>The new <see cref="KnownFile"/> instance, or <see langword="null"/> if the file list entry does not describe a valid archive.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="reader"/> is <see langword="null"/>.</exception>
		public static KnownFile RecognizeFile(XmlReader reader)
		{
			if (reader == null) {
				throw new ArgumentNullException("reader");
			}
			
			string name = null;
			string format = null;
			string md5 = null;
			long size = 0;
			
			if (reader.MoveToAttribute("name")) {
				name = reader.Value;
				reader.MoveToElement();
			}
			
			while (reader.Read()) {
				switch (reader.NodeType) {
					case XmlNodeType.Element:
						switch (reader.LocalName) {
							case "format":
								using (var subR = reader.ReadSubtree()) {
									subR.Read();
									format = subR.ReadElementContentAsString();
								}
								break;
							case "size":
								using (var subR = reader.ReadSubtree()) {
									subR.Read();
									size = subR.ReadElementContentAsLong();
								}
								break;
							case "md5":
								using (var subR = reader.ReadSubtree()) {
									subR.Read();
									md5 = subR.ReadElementContentAsString();
								}
								break;
							default:
								reader.ReadSubtree().Close();
								break;
						}
						break;
				}
			}
			
			switch (format) {
				case "7z":
					return new KnownFile(name, size, md5);
				default:
					return null;
			}
		}
		
		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		/// <param name="name">The name of the file.</param>
		/// <param name="size">The size of the file in bytes.</param>
		/// <param name="md5">The MD5 hash of the file contents.</param>
		/// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="md5"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> is not positive.</exception>
		private KnownFile(string name, long size, string md5)
		{
			if (name == null) {
				throw new ArgumentNullException("name");
			}
			if (size <= 0) {
				throw new ArgumentOutOfRangeException("size");
			}
			if (md5 == null) {
				throw new ArgumentNullException("md5");
			}
			
			this.name = name;
			this.size = size;
			this.md5 = md5;
		}
		
		/// <summary>
		/// The name of the file.
		/// </summary>
		private readonly string name;
		
		/// <summary>
		/// The size of the file in bytes.
		/// </summary>
		/// <seealso cref="Size"/>
		private readonly long size;
		
		/// <summary>
		/// The size of the file in bytes.
		/// </summary>
		public long Size {
			get {
				return size;
			}
		}
		
		/// <summary>
		/// The MD5 hash of the file contents.
		/// </summary>
		private readonly string md5;
		
		/// <summary>
		/// Downloads the file to the local file system.
		/// </summary>
		/// <param name="baseUri">The base URI that the relative file name can be expanded from.</param>
		/// <param name="directory">The destination directory.</param>
		/// <returns>The path to the downloaded file.</returns>
		/// <exception cref="ArgumentNullException">Any of the arguments is <see langword="null"/>.</exception>
		/// <remarks>
		/// <para>This method downloads the file to the local file system.
		///   After downloading, the MD5 hash of the file contents is verified.</para>
		/// </remarks>
		public string Download(Uri baseUri, string directory)
		{
			if (baseUri == null) {
				throw new ArgumentNullException("baseUri");
			}
			
			string fn = Path.Combine(directory, name);
			
			ConsoleHelper.WriteMilestone("Downloading {0} ...", name);
			using (var client = new WebClient()) {
				client.DownloadFile(new Uri(baseUri, name), fn);
			}
			System.Console.WriteLine(" done.");
			
			var fInfo = new FileInfo(fn);
			
			long fileSize = fInfo.Length;
			if (fileSize == this.size) {
				ConsoleHelper.WriteSuccessLine("File size of {0} bytes verified.", fileSize);
			} else {
				ConsoleHelper.WriteWarningLine("File size is {0} bytes, which differs from the expected size of {1} bytes.", fileSize, this.size);
			}
			
			byte[] hash;
			using (var algo = MD5.Create()) {
				using (var fs = fInfo.OpenRead()) {
					hash = algo.ComputeHash(fs);
				}
			}
			string fileMD5 = string.Join("", hash.Select(b => string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:X2}", b))).ToLowerInvariant();
			if (fileMD5 == this.md5) {
				ConsoleHelper.WriteSuccessLine("MD5 hash ({0}) verified.", fileMD5);
			} else {
				ConsoleHelper.WriteWarningLine("MD5 hash of file ({0}) does not match the expected one ({1}).", fileMD5, this.md5);
			}
			
			return fn;
		}
		
		/// <summary>
		/// Compares the object to another instance.
		/// </summary>
		/// <param name="other">The other instance or <see langword="null"/>.</param>
		/// <returns>A value that indicates an ordering between the instances.</returns>
		public int CompareTo(KnownFile other)
		{
			if (other == null) {
				return -1;
			} else {
				return this.name.CompareTo(other.name);
			}
		}
		
		/// <summary>
		/// Creates a <see cref="SiteInfo"/> object for the site the file is based upon.
		/// </summary>
		/// <returns>The newly created <see cref="SiteInfo"/> object.</returns>
		public SiteInfo RetrieveSiteInfo()
		{
			return SiteExtractor.GetSiteName(this.name);
		}
		
		/// <summary>
		/// Returns a string representation of the object.
		/// </summary>
		/// <returns>The string representation.</returns>
		public override string ToString()
		{
			return name;
		}
	}
}
