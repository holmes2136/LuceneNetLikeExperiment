<%@ Page language="c#" CodeFile="Search.aspx.cs" AutoEventWireup="false" Inherits="Searcher.Search" EnableViewState="False" %>
<!DOCTYPE html>
<html>
	<head>
        <meta charset="UTF-8">
		<title>Experiment</title>
		<link href="main.css" type="text/css" rel="stylesheet" />
	</head>
	<body>
		<form id="Form1" method="post" runat="server">
			<table id="Table1" cellspacing="0" cellpadding="2" border="0">
				<tr>
					<td>
						<p><a class="homepage" href="./">Search Demo</a>&nbsp;</p>
					</td>
					<td><asp:textbox id="TextBoxQuery" runat="server" Width="312px" Text="<%# Query %>"></asp:textbox>&nbsp;
						<asp:button id="ButtonSearch" runat="server" Text="Search" OnClick="ButtonSearch_Click"></asp:button></td>
				</tr>
			</table>
			<table class="header" cellspacing="0" cellpadding="0">
				<tr>
					<td>
						<div class="target">API Documentation</div>
					</td>
					<td>
						<div class="summary"><asp:label id="LabelSummary" runat="server" Text="<%# Summary %>"></asp:label></div>
					</td>
				</tr>
			</table>
			<div class="tip"><img src="ico/bulb16.png" width="16" height="16" alt="Tip" />&nbsp;<a href="http://www.dotlucene.net/30648/lucene-net-api-search-demo" class="source">Want the source code of this search application?</a></div>
			<div>
                <asp:repeater id="Repeater1" runat="server" DataSource="<%# Results %>">
					<ItemTemplate>
						<p><a href='<%# DataBinder.Eval(Container.DataItem, "path")  %>' class="link"><%# DataBinder.Eval(Container.DataItem, "title")  %></a><br/>
							<span class="sample">
								<%# DataBinder.Eval(Container.DataItem, "sample")  %>
							</span>
							<br>
							<span class="path">
								<%# DataBinder.Eval(Container.DataItem, "url")  %>
							</span>
						</p>
					</ItemTemplate>
				</asp:repeater>
             </div>
			<div class="paging">Result page:
				<asp:repeater id="Repeater2" runat="server" DataSource="<%# Paging %>">
					<ItemTemplate>
						<%# DataBinder.Eval(Container.DataItem, "html") %>
					</ItemTemplate>
				</asp:repeater></div>
			<div class="footer">See <a href="http://www.dotlucene.net">Lucene.Net Tutorials</a> at dotlucene.net</div>
		</form>
	</body>
</html>
