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
	internal static class ConsoleHelper
	{
		#region info
		private const ConsoleColor INFO_COLOR = ConsoleColor.Cyan;
		private const string INFO_PREFIX = "INFO: ";
		
		public static void WriteInfo(string format, params object[] args)
		{
			Console.ForegroundColor = INFO_COLOR;
			Console.Write(INFO_PREFIX + format, args);
			Console.ResetColor();
		}
		
		public static void WriteInfoLine(string format, params object[] args)
		{
			Console.ForegroundColor = INFO_COLOR;
			Console.WriteLine(INFO_PREFIX + format, args);
			Console.ResetColor();
		}
		#endregion
		
		#region warning
		private const ConsoleColor WARNING_COLOR = ConsoleColor.Yellow;
		private const string WARNING_PREFIX = "WARNING: ";
		
		public static void WriteWarning(string format, params object[] args)
		{
			Console.ForegroundColor = WARNING_COLOR;
			Console.Write(WARNING_PREFIX + format, args);
			Console.ResetColor();
		}
		
		public static void WriteWarningLine(string format, params object[] args)
		{
			Console.ForegroundColor = WARNING_COLOR;
			Console.WriteLine(WARNING_PREFIX + format, args);
			Console.ResetColor();
		}
		#endregion
		
		#region error
		private const ConsoleColor ERROR_COLOR = ConsoleColor.Red;
		private const string ERROR_PREFIX = "ERROR: ";
		
		public static void WriteError(string format, params object[] args)
		{
			Console.ForegroundColor = ERROR_COLOR;
			Console.Write(ERROR_PREFIX + format, args);
			Console.ResetColor();
		}
		
		public static void WriteErrorLine(string format, params object[] args)
		{
			Console.ForegroundColor = ERROR_COLOR;
			Console.WriteLine(ERROR_PREFIX + format, args);
			Console.ResetColor();
		}
		#endregion
		
		#region milestone
		private const ConsoleColor MILESTONE_COLOR = ConsoleColor.Magenta;
		private const string MILESTONE_PREFIX = "> ";
		
		public static void WriteMilestone(string format, params object[] args)
		{
			Console.ForegroundColor = MILESTONE_COLOR;
			Console.Write(MILESTONE_PREFIX + format, args);
			Console.ResetColor();
		}
		
		public static void WriteMilestoneLine(string format, params object[] args)
		{
			Console.ForegroundColor = MILESTONE_COLOR;
			Console.WriteLine(MILESTONE_PREFIX + format, args);
			Console.ResetColor();
		}
		#endregion
		
		#region success
		private const ConsoleColor SUCCESS_COLOR = ConsoleColor.Green;
		private const string SUCCESS_PREFIX = "OK: ";
		
		public static void WriteSuccess(string format, params object[] args)
		{
			Console.ForegroundColor = SUCCESS_COLOR;
			Console.Write(SUCCESS_PREFIX + format, args);
			Console.ResetColor();
		}
		
		public static void WriteSuccessLine(string format, params object[] args)
		{
			Console.ForegroundColor = SUCCESS_COLOR;
			Console.WriteLine(SUCCESS_PREFIX + format, args);
			Console.ResetColor();
		}
		#endregion
	}
}
