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
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

using VDS.RDF;

namespace SE2Rdf
{
	/// <summary>
	/// A class that sequentially writes <see href="http://www.w3.org/TeamSubmission/turtle/">Turtle</see> data into a stream.
	/// </summary>
	internal class SequentialTurtleWriter : IDisposable
	{
		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		/// <param name="dest">The destination stream.</param>
		/// <param name="nsMapper">The namespace prefix list to use.</param>
		/// <exception cref="ArgumentNullException">Any of the arguments is <see langword="null"/>.</exception>
		/// <remarks>
		/// <para>Initializes a new instance with the given destination stream and the given namespace prefix list.
		///   All prefixes listed in <paramref name="nsMapper"/> will be declared in the resulting Turtle output, as the writer cannot know in advance which prefixes will actually be used.</para>
		/// </remarks>
		public SequentialTurtleWriter(StreamWriter dest, INamespaceMapper nsMapper)
		{
			if (dest == null) {
				throw new ArgumentNullException("dest");
			}
			if (nsMapper == null) {
				throw new ArgumentNullException("nsMapper");
			}
			
			this.dest = dest;
			this.nsMapper = nsMapper;
			
			foreach (var prefix in nsMapper.Prefixes) {
				dest.WriteLine("@prefix {0}: <{1}> .", prefix, nsMapper.GetNamespaceUri(prefix));
			}
		}
		
		/// <summary>
		/// The destination stream.
		/// </summary>
		private readonly StreamWriter dest;
		
		/// <summary>
		/// The namespace prefix list to use.
		/// </summary>
		private readonly INamespaceMapper nsMapper;
		
		/// <summary>
		/// Releases any managed and unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			try {
				ConcludeTripleIfRequired();
			}
			finally {
				dest.Dispose();
			}
		}
		
		/// <summary>
		/// Stores the next blank node ID.
		/// </summary>
		/// <seealso cref="CreateBlankNode"/>
		private long nextBlankNodeId = 0;
		
		/// <summary>
		/// Creates a new blank node.
		/// </summary>
		/// <returns>The newly created blank node.</returns>
		public TurtleBlankNode CreateBlankNode()
		{
			return new TurtleBlankNode(this, nextBlankNodeId++);
		}
		
		/// <summary>
		/// Closes any currently open triples.
		/// </summary>
		private void ConcludeTripleIfRequired()
		{
			if (isTripleStarted) {
				while (nestedAnonymousObjects > 0) {
					FinishAnonymousNode();
				}
				
				dest.WriteLine(" .");
				
				isTripleStarted = false;
			}
		}
		
		/// <summary>
		/// Indicates whether a triple is currently started.
		/// </summary>
		private bool isTripleStarted = false;
		
		/// <summary>
		/// Indicates the currently used subject.
		/// </summary>
		private string storedSubject = null;
		
		/// <summary>
		/// Formats an IRI for use in the Turtle document.
		/// </summary>
		/// <param name="uri">The IRI.
		///   This must not be <see langword="null"/>.</param>
		/// <returns>The IRI as it can be output in the Turtle document, and in a shortened form if an appropriate prefix has been defined.</returns>
		private string FormatUri(Uri uri)
		{
			string uriString = uri.AbsoluteUri;
			string result;
			if (nsMapper.ReduceToQName(uriString, out result)) {
				return result;
			} else {
				return "<" + uriString + ">";
			}
		}
		
		#region star of triple
		/// <summary>
		/// Starts a new triple with a given IRI as a subject.
		/// </summary>
		/// <param name="subject">The subject.</param>
		/// <exception cref="ArgumentNullException"><paramref name="subject"/> is <see langword="null"/>.</exception>
		public void StartTriple(Uri subject)
		{
			if (subject == null) {
				throw new ArgumentNullException("subject");
			}
			
			storedSubject = "\n" + FormatUri(subject);
			isAnonymousSubjectStored = false;
			anonymousSubjectTriples = 0;
		}
		
		/// <summary>
		/// Starts a new triple with a given blank node as a subject.
		/// </summary>
		/// <param name="subject">The subject.</param>
		/// <exception cref="ArgumentNullException"><paramref name="subject"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="subject"/> was not created by the current Turtle writer.</exception>
		public void StartTriple(TurtleBlankNode subject)
		{
			if (subject == null) {
				throw new ArgumentNullException("subject");
			}
			subject.CheckOwner(this);
			
			storedSubject = "\n" + subject.TurtleRepresentation;
			isAnonymousSubjectStored = false;
			anonymousSubjectTriples = 0;
		}
		
		/// <summary>
		/// Starts a new triple with an anonymous blank node as a subject.
		/// </summary>
		/// <remarks>
		/// <para>This method starts a new triple with an anonymous blank node as a subject.
		///   The blank node must be terminated with a call to <see cref="FinishAnonymousNode"/>.</para>
		/// </remarks>
		/// <seealso cref="FinishAnonymousNode"/>
		public void StartAnonymousTriple()
		{
			storedSubject = "\n[";
			isAnonymousSubjectStored = true;
			isAnonymousNodeEmpty = true;
			anonymousSubjectTriples = 0;
		}
		
		/// <summary>
		/// Stores whether the stored subject is an anonymous blank node.
		/// </summary>
		private bool isAnonymousSubjectStored;
		
		/// <summary>
		/// Counts the number of triples in the anonymous blank node subject being constructed.
		/// </summary>
		private int anonymousSubjectTriples = 0;
		#endregion
		
		/// <summary>
		/// Terminates an anonymous blank node.
		/// </summary>
		/// <seealso cref="StartAnonymousTriple"/>
		/// <seealso cref="AddAnonymousToTriple"/>
		public void FinishAnonymousNode()
		{
			if (isAnonymousSubjectStored) {
				if (isAnonymousNodeEmpty) {
					storedSubject += "]";
					isAnonymousNodeEmpty = false;
				} else {
					if (nestedAnonymousObjects > 0) {
						string indentation = new System.Text.StringBuilder().Append(' ', nestedAnonymousObjects * 2).ToString();
						storedSubject += "\n" + indentation + "]";
					} else {
						storedSubject += "\n]";
					}
				}
				if (nestedAnonymousObjects > 0) {
					nestedAnonymousObjects--;
				} else {
					isAnonymousSubjectStored = false;
				}
			} else {
				if (nestedAnonymousObjects > 0) {
					if (isAnonymousNodeEmpty) {
						dest.Write("]");
						isAnonymousNodeEmpty = false;
					} else {
						string indentation = new System.Text.StringBuilder().Append(' ', nestedAnonymousObjects * 2).ToString();
						dest.Write("\n{0}]",
						           indentation);
					}
					nestedAnonymousObjects--;
				}
			}
		}
		
		/// <summary>
		/// Indicates whether the currently innermost anonymous blank node is empty.
		/// </summary>
		private bool isAnonymousNodeEmpty;
		
		#region internal triple
		/// <summary>
		/// Formats the predicate to assume its shortest form.
		/// </summary>
		/// <param name="predicate">The predicate IRI.
		///   This must not be <see langword="null"/>.</param>
		/// <returns>The formatted predicate.</returns>
		private string FormatPredicate(Uri predicate)
		{
			if (predicate.AbsoluteUri == NamespaceMapper.RDF + "type") {
				return "a";
			} else {
				return FormatUri(predicate);
			}
		}
		
		/// <summary>
		/// Indicates the nesting level of anonymous blank object nodes.
		/// </summary>
		/// <remarks>
		/// <para>This variable keeps track of the nesting depth of anonymous blank object nodes.
		///   The default value of <c>0</c> indicates that no blank object nodes have been started.</para>
		/// </remarks>
		/// <seealso cref="AddAnonymousToTriple"/>
		private int nestedAnonymousObjects;
		
		/// <summary>
		/// Adds one or more triples with a given predicate to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate IRI.</param>
		/// <param name="objectsAsCollection">Indicates whether the objects are to be added as a collection rather than as individual triples.</param>
		/// <param name="objects">An enumeration of objects, or <see langword="null"/> to indicate the start of an anonymous blank object.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		private void RawAddToTriple(Uri predicate, bool objectsAsCollection, IEnumerable<string> objects)
		{
			if (predicate == null) {
				throw new ArgumentNullException("predicate");
			}
			
			bool isAnonymousObject = objects == null;
			string[] allObjects = null;
			if (!isAnonymousObject) {
				allObjects = objects.Where(o => o != null).ToArray();
				if ((allObjects.Length <= 0) && !objectsAsCollection) {
					return;
				}
			}
			
			if (storedSubject == null) {
				if (isTripleStarted) {
					if ((nestedAnonymousObjects > 0) && isAnonymousNodeEmpty) {
						isAnonymousNodeEmpty = false;
					} else {
						dest.Write(" ;");
					}
				} else {
					throw new InvalidOperationException("Currently, no triple has been started.");
				}
			} else {
				if (isAnonymousSubjectStored) {
					if (isAnonymousNodeEmpty) {
						isAnonymousNodeEmpty = false;
					} else {
						storedSubject += " ;";
					}
				} else {
					ConcludeTripleIfRequired();
					dest.Write(storedSubject);
					storedSubject = null;
					isTripleStarted = true;
					writtenTriples += anonymousSubjectTriples;
					anonymousSubjectTriples = 0;
				}
			}
			
			var additionalText = new System.Text.StringBuilder();
			int countedTriples = 0;
			
			string indentation = new System.Text.StringBuilder().Append(' ', (nestedAnonymousObjects + 1) * 2).ToString();
			if (isAnonymousObject) {
				additionalText.AppendFormat("\n{1}{0} [",
				                            FormatPredicate(predicate),
				                            indentation);
				nestedAnonymousObjects++;
				isAnonymousNodeEmpty = true;
				countedTriples++;
			} else {
				if (objectsAsCollection) {
					additionalText.AppendFormat("\n{1}{0} (",
					                            FormatPredicate(predicate),
					                            indentation);
					foreach (string nextObject in allObjects) {
						additionalText.AppendFormat(" {0}",
						                            nextObject);
						countedTriples += 2;
					}
					if (allObjects.Length > 0) {
						additionalText.Append(" ");
					}
					additionalText.Append(")");
					countedTriples++;
				} else {
					bool isFirst = true;
					foreach (string nextObject in allObjects) {
						if (isFirst) {
							additionalText.AppendFormat("\n{2}{0} {1}",
							                            FormatPredicate(predicate),
							                            nextObject,
							                            indentation);
							isFirst = false;
						} else {
							additionalText.Append(", " + nextObject);
						}
						countedTriples++;
					}
				}
			}
			
			if (isAnonymousSubjectStored) {
				storedSubject += additionalText.ToString();
				anonymousSubjectTriples += countedTriples;
			} else {
				dest.Write(additionalText.ToString());
				writtenTriples += countedTriples;
			}
		}
		#endregion
		
		#region triple with anonymous blank object
		/// <summary>
		/// Adds a triple with a given predicate and an anonymous blank node as an object to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		/// <remarks>
		/// <para>This method adds a triple with a given predicate and an anonymous blank node as an object to the subject indicated by the last call to <see cref="StartTriple"/>.
		///   The blank node must be terminated with a call to <see cref="FinishAnonymousNode"/>.</para>
		/// </remarks>
		/// <seealso cref="FinishAnonymousNode"/>
		public void AddAnonymousToTriple(Uri predicate)
		{
			RawAddToTriple(predicate, false, null);
		}
		#endregion
		
		#region triple with IRI object
		/// <summary>
		/// Adds individual triples with a given predicate and IRI objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="objects">The objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> or <paramref name="objects"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, IEnumerable<Uri> objects)
		{
			AddToTriple(predicate, false, objects);
		}
		
		/// <summary>
		/// Adds triples with a given predicate and IRI objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="objectsAsCollection">Indicates whether the objects are to be added as a collection rather than as individual triples.</param>
		/// <param name="objects">The objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> or <paramref name="objects"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, bool objectsAsCollection, IEnumerable<Uri> objects)
		{
			if (objects == null) {
				throw new ArgumentNullException("objects");
			}
			
			RawAddToTriple(predicate, objectsAsCollection, objects.Where(o => o != null).Select(o => FormatUri(o)));
		}
		
		/// <summary>
		/// Adds individual triples with a given predicate and IRI objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="object">The first object.</param>
		/// <param name="moreObjects">More objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, Uri @object, params Uri[] moreObjects)
		{
			AddToTriple(predicate, false, @object, moreObjects);
		}
		
		/// <summary>
		/// Adds triples with a given predicate and IRI objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="objectsAsCollection">Indicates whether the objects are to be added as a collection rather than as individual triples.</param>
		/// <param name="object">The first object.</param>
		/// <param name="moreObjects">More objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, bool objectsAsCollection, Uri @object, params Uri[] moreObjects)
		{
			if (predicate == null) {
				throw new ArgumentNullException("predicate");
			}
			
			AddToTriple(predicate, objectsAsCollection,
			            Enumerable.Concat(new[] { @object }, moreObjects ?? new Uri[0]));
		}
		#endregion
		
		#region triple with blank node object
		/// <summary>
		/// Adds individual triples with a given predicate and blank node objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="objects">The objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> or <paramref name="objects"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="objects"/> was not created by the current Turtle writer.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, IEnumerable<TurtleBlankNode> objects)
		{
			AddToTriple(predicate, false, objects);
		}
		
		/// <summary>
		/// Adds triples with a given predicate and blank node objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="objectsAsCollection">Indicates whether the objects are to be added as a collection rather than as individual triples.</param>
		/// <param name="objects">The objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> or <paramref name="objects"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="objects"/> was not created by the current Turtle writer.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, bool objectsAsCollection, IEnumerable<TurtleBlankNode> objects)
		{
			if (objects == null) {
				throw new ArgumentNullException("objects");
			}
			
			foreach (var bn in objects.Where(o => o != null)) {
				bn.CheckOwner(this);
			}
			
			RawAddToTriple(predicate, objectsAsCollection, objects.Where(o => o != null).Select(o => o.TurtleRepresentation));
		}
		
		/// <summary>
		/// Adds individual triples with a given predicate and blank node objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="object">The first object.</param>
		/// <param name="moreObjects">More objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="object"/> or any of the elements of <paramref name="moreObjects"/> was not created by the current Turtle writer.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, TurtleBlankNode @object, params TurtleBlankNode[] moreObjects)
		{
			AddToTriple(predicate, false, @object, moreObjects);
		}
		
		/// <summary>
		/// Adds triples with a given predicate and blank node objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="objectsAsCollection">Indicates whether the objects are to be added as a collection rather than as individual triples.</param>
		/// <param name="object">The first object.</param>
		/// <param name="moreObjects">More objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="object"/> or any of the elements of <paramref name="moreObjects"/> was not created by the current Turtle writer.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, bool objectsAsCollection, TurtleBlankNode @object, params TurtleBlankNode[] moreObjects)
		{
			if (predicate == null) {
				throw new ArgumentNullException("predicate");
			}
			
			AddToTriple(predicate, objectsAsCollection,
			            Enumerable.Concat(new[] { @object }, moreObjects ?? new TurtleBlankNode[0]));
		}
		#endregion
		
		#region triple with string object
		/// <summary>
		/// Adds individual triples with a given predicate and string literal objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="objects">The objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> or <paramref name="objects"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, IEnumerable<string> objects)
		{
			AddToTriple(predicate, false, objects);
		}
		
		/// <summary>
		/// Adds triples with a given predicate and string literal objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="objectsAsCollection">Indicates whether the objects are to be added as a collection rather than as individual triples.</param>
		/// <param name="objects">The objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> or <paramref name="objects"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, bool objectsAsCollection, IEnumerable<string> objects)
		{
			if (objects == null) {
				throw new ArgumentNullException("objects");
			}
			
			RawAddToTriple(predicate, objectsAsCollection, objects.Where(o => o != null).Select(o => "\"" + EscapeLiteralString(o) + "\""));
		}
		
		/// <summary>
		/// Adds individual triples with a given predicate and string literal objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="object">The first object.</param>
		/// <param name="moreObjects">More objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, string @object, params string[] moreObjects)
		{
			AddToTriple(predicate, false, @object, moreObjects);
		}
		
		/// <summary>
		/// Adds triples with a given predicate and string literal objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="objectsAsCollection">Indicates whether the objects are to be added as a collection rather than as individual triples.</param>
		/// <param name="object">The first object.</param>
		/// <param name="moreObjects">More objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, bool objectsAsCollection, string @object, params string[] moreObjects)
		{
			if (predicate == null) {
				throw new ArgumentNullException("predicate");
			}
			
			AddToTriple(predicate, objectsAsCollection,
			            Enumerable.Concat(new[] { @object }, moreObjects ?? new string[0]));
		}
		#endregion
		
		#region triple with long object
		/// <summary>
		/// Adds individual triples with a given predicate and <see cref="long"/> objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="objects">The objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> or <paramref name="objects"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, IEnumerable<long> objects)
		{
			AddToTriple(predicate, false, objects);
		}
		
		/// <summary>
		/// Adds triples with a given predicate and <see cref="long"/> objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="objectsAsCollection">Indicates whether the objects are to be added as a collection rather than as individual triples.</param>
		/// <param name="objects">The objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> or <paramref name="objects"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, bool objectsAsCollection, IEnumerable<long> objects)
		{
			if (objects == null) {
				throw new ArgumentNullException("objects");
			}
			
			RawAddToTriple(predicate, objectsAsCollection, objects.Select(o => o.ToString(CultureInfo.InvariantCulture)));
		}
		
		/// <summary>
		/// Adds individual triples with a given predicate and <see cref="long"/> objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="object">The first object.</param>
		/// <param name="moreObjects">More objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, long @object, params long[] moreObjects)
		{
			AddToTriple(predicate, false, @object, moreObjects);
		}
		
		/// <summary>
		/// Adds triples with a given predicate and <see cref="long"/> objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="objectsAsCollection">Indicates whether the objects are to be added as a collection rather than as individual triples.</param>
		/// <param name="object">The first object.</param>
		/// <param name="moreObjects">More objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, bool objectsAsCollection, long @object, params long[] moreObjects)
		{
			if (predicate == null) {
				throw new ArgumentNullException("predicate");
			}
			
			AddToTriple(predicate, objectsAsCollection,
			            Enumerable.Concat(new[] { @object }, moreObjects ?? new long[0]));
		}
		#endregion
		
		#region triple with DateTime object
		/// <summary>
		/// Adds individual triples with a given predicate and <see cref="DateTime"/> objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="objects">The objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> or <paramref name="objects"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, IEnumerable<DateTime> objects)
		{
			AddToTriple(predicate, false, objects);
		}
		
		/// <summary>
		/// Adds triples with a given predicate and <see cref="DateTime"/> objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="objectsAsCollection">Indicates whether the objects are to be added as a collection rather than as individual triples.</param>
		/// <param name="objects">The objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> or <paramref name="objects"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, bool objectsAsCollection, IEnumerable<DateTime> objects)
		{
			if (objects == null) {
				throw new ArgumentNullException("objects");
			}
			
			RawAddToTriple(predicate, objectsAsCollection, objects.Select(o => FormatDateTimeLiteral(o)));
		}
		
		/// <summary>
		/// Adds individual triples with a given predicate and <see cref="DateTime"/> objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="object">The first object.</param>
		/// <param name="moreObjects">More objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, DateTime @object, params DateTime[] moreObjects)
		{
			AddToTriple(predicate, false, @object, moreObjects);
		}
		
		/// <summary>
		/// Adds triples with a given predicate and <see cref="DateTime"/> objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="objectsAsCollection">Indicates whether the objects are to be added as a collection rather than as individual triples.</param>
		/// <param name="object">The first object.</param>
		/// <param name="moreObjects">More objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, bool objectsAsCollection, DateTime @object, params DateTime[] moreObjects)
		{
			if (predicate == null) {
				throw new ArgumentNullException("predicate");
			}
			
			AddToTriple(predicate, objectsAsCollection,
			            Enumerable.Concat(new[] { @object }, moreObjects ?? new DateTime[0]));
		}
		
		/// <summary>
		/// The type IRI of <see cref="DateTime"/> values in Turtle.
		/// </summary>
		private static readonly Uri dateTimeType = new Uri("http://www.w3.org/2001/XMLSchema#dateTime");
		
		/// <summary>
		/// Formats a <see cref="DateTime"/> literal value.
		/// </summary>
		/// <param name="dt">The <see cref="DateTime"/> value.</param>
		/// <returns>The formatted literal.</returns>
		private static string FormatDateTimeString(DateTime dt)
		{
			string dtFormat;
			if (dt.Millisecond != 0) {
				dtFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fff'Z'";
			} else {
				dtFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'";
			}
			
			return dt.ToString(dtFormat, CultureInfo.InvariantCulture);
		}
		
		/// <summary>
		/// Formats a <see cref="DateTime"/> value as a Turtle literal.
		/// </summary>
		/// <param name="literal">The <see cref="DateTime"/> value.</param>
		/// <returns>The formatted Turtle literal.</returns>
		private string FormatDateTimeLiteral(DateTime literal)
		{
			return string.Format(CultureInfo.InvariantCulture,
			                     "\"{0}\"^^{1}",
			                     FormatDateTimeString(literal), FormatUri(dateTimeType));
		}
		#endregion
		
		#region triple with boolean object
		/// <summary>
		/// Adds individual triples with a given predicate and <see cref="bool"/> objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="objects">The objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> or <paramref name="objects"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, IEnumerable<bool> objects)
		{
			AddToTriple(predicate, false, objects);
		}
		
		/// <summary>
		/// Adds triples with a given predicate and <see cref="bool"/> objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="objectsAsCollection">Indicates whether the objects are to be added as a collection rather than as individual triples.</param>
		/// <param name="objects">The objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> or <paramref name="objects"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, bool objectsAsCollection, IEnumerable<bool> objects)
		{
			if (objects == null) {
				throw new ArgumentNullException("objects");
			}
			
			RawAddToTriple(predicate, objectsAsCollection, objects.Select(o => o ? "true" : "false"));
		}
		
		/// <summary>
		/// Adds individual triples with a given predicate and <see cref="bool"/> objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="object">The first object.</param>
		/// <param name="moreObjects">More objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, bool @object, params bool[] moreObjects)
		{
			AddToTriple(predicate, false, @object, moreObjects);
		}
		
		/// <summary>
		/// Adds triples with a given predicate and <see cref="bool"/> objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="objectsAsCollection">Indicates whether the objects are to be added as a collection rather than as individual triples.</param>
		/// <param name="object">The first object.</param>
		/// <param name="moreObjects">More objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, bool objectsAsCollection, bool @object, params bool[] moreObjects)
		{
			if (predicate == null) {
				throw new ArgumentNullException("predicate");
			}
			
			AddToTriple(predicate, objectsAsCollection,
			            Enumerable.Concat(new[] { @object }, moreObjects ?? new bool[0]));
		}
		#endregion
		
		#region triple with decimal object
		/// <summary>
		/// Adds individual triples with a given predicate and <see cref="decimal"/> objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="objects">The objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> or <paramref name="objects"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, IEnumerable<decimal> objects)
		{
			AddToTriple(predicate, false, objects);
		}
		
		/// <summary>
		/// Adds triples with a given predicate and <see cref="decimal"/> objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="objectsAsCollection">Indicates whether the objects are to be added as a collection rather than as individual triples.</param>
		/// <param name="objects">The objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> or <paramref name="objects"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, bool objectsAsCollection, IEnumerable<decimal> objects)
		{
			if (objects == null) {
				throw new ArgumentNullException("objects");
			}
			
			RawAddToTriple(predicate, objectsAsCollection, objects.Select(o => o.ToString("#0.##############################", System.Globalization.CultureInfo.InvariantCulture)));
		}
		
		/// <summary>
		/// Adds individual triples with a given predicate and <see cref="decimal"/> objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="object">The first object.</param>
		/// <param name="moreObjects">More objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, decimal @object, params decimal[] moreObjects)
		{
			AddToTriple(predicate, false, @object, moreObjects);
		}
		
		/// <summary>
		/// Adds triples with a given predicate and <see cref="decimal"/> objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="objectsAsCollection">Indicates whether the objects are to be added as a collection rather than as individual triples.</param>
		/// <param name="object">The first object.</param>
		/// <param name="moreObjects">More objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, bool objectsAsCollection, decimal @object, params decimal[] moreObjects)
		{
			if (predicate == null) {
				throw new ArgumentNullException("predicate");
			}
			
			AddToTriple(predicate, objectsAsCollection,
			            Enumerable.Concat(new[] { @object }, moreObjects ?? new decimal[0]));
		}
		#endregion
		
		#region triple with double object
		/// <summary>
		/// Adds individual triples with a given predicate and <see cref="double"/> objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="objects">The objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> or <paramref name="objects"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, IEnumerable<double> objects)
		{
			AddToTriple(predicate, false, objects);
		}
		
		/// <summary>
		/// Adds triples with a given predicate and <see cref="double"/> objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="objectsAsCollection">Indicates whether the objects are to be added as a collection rather than as individual triples.</param>
		/// <param name="objects">The objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> or <paramref name="objects"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, bool objectsAsCollection, IEnumerable<double> objects)
		{
			if (objects == null) {
				throw new ArgumentNullException("objects");
			}
			
			RawAddToTriple(predicate, objectsAsCollection, objects.Select(o => o.ToString("#0.########################################################E-0", System.Globalization.CultureInfo.InvariantCulture)));
		}
		
		/// <summary>
		/// Adds individual triples with a given predicate and <see cref="double"/> objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="object">The first object.</param>
		/// <param name="moreObjects">More objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, double @object, params double[] moreObjects)
		{
			AddToTriple(predicate, false, @object, moreObjects);
		}
		
		/// <summary>
		/// Adds triples with a given predicate and <see cref="double"/> objects to the subject indicated by the last call to <see cref="StartTriple"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="objectsAsCollection">Indicates whether the objects are to be added as a collection rather than as individual triples.</param>
		/// <param name="object">The first object.</param>
		/// <param name="moreObjects">More objects.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="StartTriple"/> has not been called before.</exception>
		public void AddToTriple(Uri predicate, bool objectsAsCollection, double @object, params double[] moreObjects)
		{
			if (predicate == null) {
				throw new ArgumentNullException("predicate");
			}
			
			AddToTriple(predicate, objectsAsCollection,
			            Enumerable.Concat(new[] { @object }, moreObjects ?? new double[0]));
		}
		#endregion
		
		/// <summary>
		/// Escapes a string literal for use in Turtle syntax.
		/// </summary>
		/// <param name="str">The literal string.
		///   This must not be <see langword="null"/>.</param>
		/// <returns>The escaped literal string.</returns>
		private string EscapeLiteralString(string str)
		{
			var result = new System.Text.StringBuilder();
			foreach (char ch in str) {
				switch (ch) {
					case '\t':
						result.Append("\\t");
						break;
					case '\n':
						result.Append("\\n");
						break;
					case '\r':
						result.Append("\\r");
						break;
					case '"':
						result.Append("\\\"");
						break;
					case '\'':
						result.Append("\\'");
						break;
					case '\\':
						result.Append("\\\\");
						break;
					default:
						if (char.IsControl(ch)
						    || char.IsLowSurrogate(ch)
						    || char.IsHighSurrogate(ch)) {
							result.AppendFormat(CultureInfo.InvariantCulture,
							                    @"\u{0:X4}", (int)ch);
						} else {
							result.Append(ch);
						}
						break;
				}
			}
			return result.ToString();
		}
		
		/// <summary>
		/// Indicates the current size of the stream.
		/// </summary>
		/// <returns></returns>
		public long GetCurrentStreamSize()
		{
			dest.Flush();
			return dest.BaseStream.Length;
		}
		
		/// <summary>
		/// Stores the number of triples written up to now.
		/// </summary>
		/// <seealso cref="GetCurrentTripleCount"/>
		private long writtenTriples;
		
		/// <summary>
		/// Returns the number of triples written up to now.
		/// </summary>
		/// <returns></returns>
		public long GetCurrentTripleCount()
		{
			return writtenTriples;
		}
	}
}
