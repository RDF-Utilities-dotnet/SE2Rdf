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

using VDS.RDF;

namespace SE2Rdf
{
	partial class GlobalInformationConverter
	{
		private static void WriteConstants(GeneralUris generalUris, string destDir, VDS.RDF.INamespaceMapper nsMapper)
		{
			ConsoleHelper.WriteMilestone("Writing constant definitions ...");
			using (var destWriter = new SequentialTurtleWriter(File.CreateText(Path.Combine(destDir, "_constants.ttl")), nsMapper)) {
				WriteCloseReasons(generalUris, destWriter);
				
				GlobalData.UpdateStats(destWriter);
			}
			Console.WriteLine(" done.");
		}
		
		private static void WriteCloseReasons(GeneralUris generalUris, SequentialTurtleWriter w)
		{
			WriteCloseReason(generalUris, w, generalUris.DuplicateCloseReason, "Duplicate");
			WriteCloseReason(generalUris, w, generalUris.OffTopicCloseReason, "Off-topic");
			WriteCloseReason(generalUris, w, generalUris.SubjectiveCloseReason, "Opinion-based");
			WriteCloseReason(generalUris, w, generalUris.NotAQuestionCloseReason, "Not a real question");
			WriteCloseReason(generalUris, w, generalUris.TooLocalizedCloseReason, "Too localized");
			WriteCloseReason(generalUris, w, generalUris.GeneralReferenceCloseReason, "General reference");
			WriteCloseReason(generalUris, w, generalUris.NoiseCloseReason, "Pointless/Noise");
			WriteCloseReason(generalUris, w, generalUris.UnclearCloseReason, "Unclear what you're asking");
			WriteCloseReason(generalUris, w, generalUris.TooBroadCloseReason, "Too broad");
		}
		
		private static void WriteCloseReason(GeneralUris generalUris, SequentialTurtleWriter w, Uri reason, string name)
		{
			w.StartTriple(reason);
			w.AddToTriple(generalUris.TypeProperty, generalUris.CloseReasonType);
			w.AddToTriple(generalUris.LabelProperty, name);
		}
	}
}
