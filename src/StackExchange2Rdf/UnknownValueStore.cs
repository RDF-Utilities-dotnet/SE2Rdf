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
using System.Collections.Generic;
using System.Linq;

namespace StackExchange2Rdf
{
	/// <summary>
	/// Stores values found in input files that could not be interpreted, along with their number of occurrences.
	/// </summary>
	/// <typeparam name="T">The type of the found values.</typeparam>
	internal class UnknownValueStore<T>
	{
		/// <summary>
		/// The internal value storage.
		/// </summary>
		private readonly Dictionary<T, long> unknownValues = new Dictionary<T, long>();
		
		/// <summary>
		/// Counts an unknown value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
		public void RegisterUnknownValue(T value)
		{
			if (value == null) {
				throw new ArgumentNullException("value");
			}
			
			long count;
			if (unknownValues.TryGetValue(value, out count)) {
				unknownValues[value] = count + 1;
			} else {
				unknownValues[value] = 1;
			}
		}
		
		/// <summary>
		/// Indicates the total number of different unknown values.
		/// </summary>
		public long RegisteredValueCount {
			get {
				return unknownValues.Values.Sum();
			}
		}
		
		/// <summary>
		/// Formats the currently unknown values and their number of occurrences for user output.
		/// </summary>
		/// <returns>The formatted text.</returns>
		public override string ToString()
		{
			var result = new System.Text.StringBuilder();
			foreach (var pair in unknownValues.OrderByDescending(p => p.Value)) {
				if (result.Length > 0) {
					result.Append("; ");
				}
				result.AppendFormat("\"{0}\" ({1})", pair.Key, pair.Value);
			}
			return result.ToString();
		}
	}
}
