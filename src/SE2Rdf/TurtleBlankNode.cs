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

namespace SE2Rdf
{
	/// <summary>
	/// Represents a reusable blank node in a Turtle document.
	/// </summary>
	public sealed class TurtleBlankNode
	{
		internal TurtleBlankNode(SequentialTurtleWriter owner, long id)
		{
			if (owner == null) {
				throw new ArgumentNullException("owner");
			}
			
			this.owner = owner;
			this.id = id;
		}
		
		private readonly SequentialTurtleWriter owner;
		
		internal void CheckOwner(SequentialTurtleWriter expected)
		{
			if (owner != expected) {
				throw new ArgumentException("The blank node does not belong to the current Turtle writer.");
			}
		}
		
		private readonly long id;
		
		internal string TurtleRepresentation {
			get {
				return "_:b" + id.ToString(System.Globalization.CultureInfo.InvariantCulture);
			}
		}
	}
}
