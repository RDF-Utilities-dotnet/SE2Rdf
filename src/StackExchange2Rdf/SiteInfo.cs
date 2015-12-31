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

namespace StackExchange2Rdf
{
	/// <summary>
	/// Stores some superficial data on a Q&amp;A site.
	/// </summary>
	internal sealed class SiteInfo : IEquatable<SiteInfo>
	{
		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		/// <param name="id">The unique ID of the site.</param>
		/// <param name="name">The name of the site.
		///   Several sites can have the same name, in particular a site and its meta-site.</param>
		/// <param name="language">The language tag of the site, unless it is English.</param>
		/// <param name="isMetaSite">Indicates whether the site is a meta-site.</param>
		/// <exception cref="ArgumentNullException"><paramref name="id"/> or <paramref name="name"/> is <see langword="null"/> or contains no non-whitespace characters.</exception>
		public SiteInfo(string id, string name, bool isMetaSite, string language)
		{
			if (string.IsNullOrWhiteSpace(id)) {
				throw new ArgumentNullException("id");
			}
			if (string.IsNullOrWhiteSpace(name)) {
				throw new ArgumentNullException("name");
			}
			
			this.id = id;
			this.name = name;
			this.isMetaSite = isMetaSite;
			this.language = language;
		}
		
		/// <summary>
		/// The unique ID of the site.
		/// </summary>
		/// <seealso cref="Id"/>
		private readonly string id;
		
		/// <summary>
		/// The unique ID of the site.
		/// </summary>
		public string Id {
			get {
				return id;
			}
		}
		
		/// <summary>
		/// The name of the site.
		/// </summary>
		/// <seealso cref="Name"/>
		private readonly string name;
		
		/// <summary>
		/// The name of the site.
		/// </summary>
		/// <value>
		/// <para>Gets the name of the site.
		///   Several sites can have the same name, in particular a site and its meta-site.</para>
		/// </value>
		public string Name {
			get {
				return name;
			}
		}
		
		/// <summary>
		/// Indicates whether the site is a meta-site.
		/// </summary>
		/// <seealso cref="IsMetaSite"/>
		private readonly bool isMetaSite;
		
		/// <summary>
		/// Indicates whether the site is a meta-site.
		/// </summary>
		public bool IsMetaSite {
			get {
				return isMetaSite;
			}
		}
		
		/// <summary>
		/// The optional non-English language tag of the site.
		/// </summary>
		/// <seealso cref="Language"/>
		private readonly string language;
		
		/// <summary>
		/// The optional non-English language tag of the site.
		/// </summary>
		public string Language {
			get {
				return language;
			}
		}
		
		/// <summary>
		/// Indicates whether the site's overall language is English.
		/// </summary>
		public bool IsEnglishSite {
			get {
				return string.IsNullOrWhiteSpace(language);
			}
		}
		
		/// <summary>
		/// Returns a normalized name for the site.
		/// </summary>
		public string FormalName {
			get {
				return (IsEnglishSite ? "" : (language + "-")) + name + (isMetaSite ? "-meta" : "");
			}
		}
		
		/// <summary>
		/// Returns a string representation of the object.
		/// </summary>
		/// <returns>The string representation.</returns>
		public override string ToString()
		{
			return FormalName;
		}
		
		/// <summary>
		/// Compares the instance to another instance for equality.
		/// </summary>
		/// <param name="other">The other instance or <see langword="null"/>.</param>
		/// <returns>A value that indicates whether the two instances denote the same site.</returns>
		public bool Equals(SiteInfo other)
		{
			if (other == null) {
				return false;
			} else {
				return (other.id == this.id)
					&& (other.name == this.name)
					&& (other.isMetaSite == this.isMetaSite);
			}
		}
		
		/// <summary>
		/// Compares the instance to another instance for equality.
		/// </summary>
		/// <param name="other">The other instance or <see langword="null"/>.</param>
		/// <returns>A value that indicates whether the two instances denote the same site.</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as SiteInfo);
		}
		
		/// <summary>
		/// Computes a hash code for the instance.
		/// </summary>
		/// <returns>The hash code.</returns>
		public override int GetHashCode()
		{
			return id.GetHashCode() ^ name.GetHashCode() ^ isMetaSite.GetHashCode();
		}
	}
}
