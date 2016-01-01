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
	internal static partial class GlobalInformationConverter
	{
		public static void Convert(GeneralUris generalUris, string tempDir, Uri baseUri, string destDir)
		{
			if (tempDir == null) {
				throw new ArgumentNullException("tempDir");
			}
			if (baseUri == null) {
				throw new ArgumentNullException("baseUri");
			}
			if (destDir == null) {
				throw new ArgumentNullException("destDir");
			}
			
			var nsMapper = generalUris.CreateNamespaceMapper();
			
			WriteOntology(generalUris, destDir, nsMapper);
			if (!GlobalData.Options.OntologyOnly) {
				ConvertSiteList(generalUris, tempDir, baseUri, destDir, nsMapper);
				if (!GlobalData.Options.SiteListOnly) {
					WriteAccountList(generalUris, destDir, nsMapper);
					WriteBadgesLists(generalUris, destDir, nsMapper);
					WriteConstants(generalUris, destDir, nsMapper);
				}
			}
		}
	}
}
