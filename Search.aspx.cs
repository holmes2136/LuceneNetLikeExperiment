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

using System;
using System.Data;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Search.Highlight;
using Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;

namespace Searcher
{
	/// <summary>
	/// Summary description for WebForm1.
	/// </summary>
	public partial class Search : System.Web.UI.Page
	{
		/// <summary>
		/// Search results.
		/// </summary>
		protected DataTable Results = new DataTable();

		/// <summary>
		/// First item on page (index format).
		/// </summary>
		private int startAt;

		/// <summary>
		/// First item on page (user format).
		/// </summary>
		private int fromItem;

		/// <summary>
		/// Last item on page (user format).
		/// </summary>
		private int toItem;

		/// <summary>
		/// Total items returned by search.
		/// </summary>
		private int total;

		/// <summary>
		/// Time it took to make the search.
		/// </summary>
		private TimeSpan duration;

		/// <summary>
		/// How many items can be showed on one page.
		/// </summary>
		private readonly int maxResults = 10;

		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Press ButtonSearch on enter
			Page.RegisterHiddenField("__EVENTTARGET", "ButtonSearch");
            
			if (!IsPostBack)
			{
				if (this.Query != null) 
				{
					search();
					DataBind();
				}
                else
				{
				    Response.Redirect("~/");
				}
			}
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.ButtonSearch.Click += new System.EventHandler(this.ButtonSearch_Click);
			this.Load += new System.EventHandler(this.Page_Load);

		}
		#endregion

		/// <summary>
		/// Does the search and stores the information about the results.
		/// </summary>
		private void search()
		{
			DateTime start = DateTime.Now;

			// create the searcher
			// index is placed in "index" subdirectory
			string indexDirectory = Server.MapPath("~/App_Data/index");

		    //var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            var analyzer = new Lucene.Net.Analysis.PanGu.PanGuAnalyzer();
            
			IndexSearcher searcher = new IndexSearcher(FSDirectory.Open(indexDirectory)); 

			// parse the query, "text" is the default field to search
            var parser = new QueryParser(Version.LUCENE_30, "name", analyzer);
            
            parser.AllowLeadingWildcard = true;
			
            Query query = parser.Parse(this.Query); 

			// create the result DataTable
			this.Results.Columns.Add("title", typeof(string));
            this.Results.Columns.Add("sample", typeof(string));
            this.Results.Columns.Add("path", typeof(string));
            this.Results.Columns.Add("url", typeof(string));

			// search
		    TopDocs hits = searcher.Search(query, 200);

		    this.total = hits.TotalHits;

            // create highlighter
            IFormatter formatter = new SimpleHTMLFormatter("<span style=\"font-weight:bold;\">", "</span>");
            SimpleFragmenter fragmenter = new SimpleFragmenter(80);
            QueryScorer scorer = new QueryScorer(query);
            Highlighter highlighter = new Highlighter(formatter, scorer);
            highlighter.TextFragmenter = fragmenter;

			// initialize startAt
			this.startAt = InitStartAt();

			// how many items we should show - less than defined at the end of the results
			int resultsCount = Math.Min(total, this.maxResults + this.startAt);


			for (int i = startAt; i < resultsCount; i++) 
			{
				// get the document from index
                Document doc = searcher.Doc(hits.ScoreDocs[i].Doc);

                TokenStream stream = analyzer.TokenStream("", new StringReader(doc.Get("name")));
                String sample= highlighter.GetBestFragments(stream, doc.Get("name"), 2, "...");

			    String path = doc.Get("path");

				// create a new row with the result data
				DataRow row = this.Results.NewRow();
				row["title"] = doc.Get("name");
                //row["path"] = "api/" + path;
                //row["url"] = "www.dotlucene.net/documentation/api/" + path;
                //row["sample"] = sample;

				this.Results.Rows.Add(row);
			} 
			searcher.Dispose();

			// result information
			this.duration = DateTime.Now - start;
			this.fromItem = startAt + 1;
			this.toItem = Math.Min(startAt + maxResults, total);
		}

		/// <summary>
		/// Page links. DataTable might be overhead but there used to be more fields in previous version so I'm keeping it for now.
		/// </summary>
		protected DataTable Paging
		{
			get
			{
				// pageNumber starts at 1
				int pageNumber = (startAt + maxResults - 1) / maxResults;

				DataTable dt = new DataTable();
				dt.Columns.Add("html", typeof(string));

				DataRow ar = dt.NewRow();
				ar["html"] = PagingItemHtml(startAt, pageNumber + 1, false);
				dt.Rows.Add(ar);

				int previousPagesCount = 4;
				for (int i = pageNumber - 1; i >= 0 && i >= pageNumber - previousPagesCount; i--)
				{
					int step = i - pageNumber;
					DataRow r = dt.NewRow();
					r["html"] = PagingItemHtml(startAt + (maxResults * step), i + 1, true);

					dt.Rows.InsertAt(r, 0);
				}

				int nextPagesCount = 4;
				for (int i = pageNumber + 1; i <= PageCount && i <= pageNumber + nextPagesCount; i++)
				{
					int step = i - pageNumber;
					DataRow r = dt.NewRow();
					r["html"] = PagingItemHtml(startAt + (maxResults * step), i + 1, true);

					dt.Rows.Add(r);
				}
				return dt;
			}
		}

		/// <summary>
		/// Prepares HTML of a paging item (bold number for current page, links for others).
		/// </summary>
		/// <param name="start"></param>
		/// <param name="number"></param>
		/// <param name="active"></param>
		/// <returns></returns>
		private string PagingItemHtml(int start, int number, bool active)
		{

			if (active)
				return "<a href=\"Search.aspx?q=" + this.Query + "&start=" + start + "\">" + number + "</a>";
			else
				return "<b>" + number + "</b>";
		}

		/// <summary>
		/// Prepares the string with seach summary information.
		/// </summary>
		protected string Summary
		{
			get
			{
				if (total > 0)
					return "Results <b>" + this.fromItem + " - " + this.toItem + "</b> of <b>" + this.total + "</b> for <b>" + this.Query + "</b>. (" + this.duration.TotalSeconds + " seconds)";
				return "No results found";
			}
		}

		/// <summary>
		/// Return search query or null if not provided.
		/// </summary>
		protected string Query
		{
			get 
			{
				string query = this.Request.Params["q"];
				if (query == String.Empty)
					return null;
				return query;
			}
		}

		/// <summary>
		/// Initializes startAt value. Checks for bad values.
		/// </summary>
		/// <returns></returns>
		private int InitStartAt()
		{
			try
			{
				int sa = Convert.ToInt32(this.Request.Params["start"]);

				// too small starting item, return first page
				if (sa < 0)
					return 0;

				// too big starting item, return last page
				if (sa >= total - 1)
				{
					return LastPageStartsAt;
				}

				return sa;
			}
			catch
			{
				return 0;
			}		
		}

		/// <summary>
		/// How many pages are there in the results.
		/// </summary>
		private int PageCount
		{
			get 
			{
				return (total - 1) / maxResults; // floor
			}
		}

		/// <summary>
		/// First item of the last page
		/// </summary>
		private int LastPageStartsAt
		{
			get
			{
				return PageCount * maxResults;
			}
		}


		/// <summary>
		/// This should be replaced with a direct client-side get
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void ButtonSearch_Click(object sender, System.EventArgs e)
		{
			this.Response.Redirect("Search.aspx?q=" + this.TextBoxQuery.Text);
		}


	}
}
