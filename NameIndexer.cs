/*
 * Copyright 2012 dotlucene.net
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.IO;
using System.Text.RegularExpressions;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;
using PanGu;

namespace Indexer
{
	/// <summary>
	/// Summary description for Indexer.
	/// </summary>
	public class NameIndexer
	{
		private IndexWriter writer;
		//private string docRootDirectory;
		//private string pattern;

		/// <summary>
		/// Creates a new index in <c>directory</c>. Overwrites the existing index in that directory.
		/// </summary>
		/// <param name="directory">Path to index (will be created if not existing).</param>
        public NameIndexer(string directory)
		{
            writer = new IndexWriter(FSDirectory.Open(directory), new Lucene.Net.Analysis.PanGu.PanGuAnalyzer(), true, IndexWriter.MaxFieldLength.LIMITED);
			writer.UseCompoundFile = true;
		}

		/// <summary>
		/// Add HTML files from <c>directory</c> and its subdirectories that match <c>pattern</c>.
		/// </summary>
		/// <param name="directory">Directory with the HTML files.</param>
		/// <param name="pattern">Search pattern, e.g. <c>"*.html"</c></param>
		public void Addfile(string filename)
		{

            AddHtmlDocument(filename);
		}


		/// <summary>
		/// Loads, parses and indexes an HTML file.
		/// </summary>
		/// <param name="path"></param>
		public void AddHtmlDocument(string path)
		{
		

			string text;
			using (StreamReader sr = new StreamReader(path, System.Text.Encoding.Default))
			{
                while (sr.Peek() >=0) {
                    Document doc = new Document();
                    text = sr.ReadLine();
                    doc.Add(new Field("name", text, Field.Store.YES, Field.Index.ANALYZED));
                    writer.AddDocument(doc);
                }
                
			}

            //int relativePathStartsAt = this.docRootDirectory.EndsWith("\\") ? this.docRootDirectory.Length : this.docRootDirectory.Length + 1;
            //string relativePath = path.Substring(relativePathStartsAt);

            //Close();
			
		} 

		/// <summary>
		/// Optimizes and save the index.
		/// </summary>
		public void Close()
		{
			writer.Optimize();
			writer.Dispose();
		}


	}
}
