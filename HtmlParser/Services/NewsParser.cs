using HtmlAgilityPack;
using HtmlParser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HtmlParser.Services
{
	public class NewsParser : INewsParser
	{
		private readonly ITextCleaner _textCleaner;
		private readonly ParserConfig _config;

		public NewsParser(ITextCleaner textCleaner, ParserConfig config)
		{
			_textCleaner = textCleaner;
			_config = config;
		}

		public (List<NewsItem> news, List<string> errors) Parse(string htmlContent)
		{
			var newsItems = new List<NewsItem>();
			var errors = new List<string>();

			var doc = new HtmlDocument();
			doc.LoadHtml(htmlContent);

			var newsNodes = doc.DocumentNode.SelectNodes("//li[contains(@class, 'news-item')]");
			if (newsNodes == null)
			{
				errors.Add("No news items found in the HTML file.");
				return (newsItems, errors);
			}

			foreach (var node in newsNodes)
			{
				try
				{
					if (IsNonNewsItem(node, out var error))
					{
						errors.Add(error);
						continue;
					}

					var newsBody = node.SelectSingleNode(".//div[contains(@class, 'news-body')]");
					if (newsBody == null)
					{
						errors.Add($"Skipped item without news-body: {GetShortText(node.InnerHtml)}...");
						continue;
					}

					if (!TryParseNewsItem(newsBody, out var newsItem, out var parseError))
					{
						errors.Add(parseError);
						continue;
					}

					newsItems.Add(newsItem);
				}
				catch (Exception ex)
				{
					errors.Add($"Error processing news item: {ex.Message}");
				}
			}

			return (newsItems, errors);
		}

		private bool IsNonNewsItem(HtmlNode node, out string error)
		{
			if (node.SelectSingleNode(".//div[contains(@class, 'ad-banner')]") != null)
			{
				error = "Skipped ad banner";
				return true;
			}

			if (node.SelectSingleNode(".//div[contains(@class, 'footer')]") != null)
			{
				error = "Skipped footer content";
				return true;
			}

			error = null;
			return false;
		}

		private bool TryParseNewsItem(HtmlNode newsBody, out NewsItem newsItem, out string error)
		{
			newsItem = null;

			if (!TryParseTitle(newsBody, out var title, out error))
				return false;

			if (!TryParseUrl(newsBody, out var url, out error))
				return false;

			if (!TryParseDate(newsBody, out var date, out error))
				return false;

			newsItem = new NewsItem
			{
				Title = title,
				Url = url,
				Date = date
			};

			return true;
		}

		private bool TryParseTitle(HtmlNode newsBody, out string title, out string error)
		{
			title = null;
			error = null;

			var titleNode = newsBody.SelectSingleNode(".//h4[contains(@class, 'news-title')]//a")
						   ?? newsBody.SelectSingleNode(".//h4//a");

			if (titleNode == null || string.IsNullOrWhiteSpace(titleNode.InnerHtml))
			{
				error = "Skipped item without title";
				return false;
			}

			title = _textCleaner.CleanText(titleNode.InnerText);
			return true;
		}

		private bool TryParseUrl(HtmlNode newsBody, out string url, out string error)
		{
			url = null;
			error = null;

			var titleNode = newsBody.SelectSingleNode(".//h4[contains(@class, 'news-title')]//a")
						   ?? newsBody.SelectSingleNode(".//h4//a");

			var relativeUrl = titleNode?.GetAttributeValue("href", "");
			if (string.IsNullOrWhiteSpace(relativeUrl))
			{
				error = $"Skipped item without URL";
				return false;
			}

			url = _config.BaseUrl + relativeUrl;
			return true;
		}

		private bool TryParseDate(HtmlNode newsBody, out string date, out string error)
		{
			date = null;
			error = null;

			var dateNode = newsBody.SelectSingleNode(".//time[@datetime]");
			if (dateNode == null)
			{
				error = "Skipped item without valid date";
				return false;
			}

			var dateStr = dateNode.GetAttributeValue("datetime", "");
			if (!DateTime.TryParse(dateStr, out var parsedDate))
			{
				error = $"Skipped item with invalid date format: {dateStr}";
				return false;
			}

			date = parsedDate.ToString(_config.DateFormat);
			return true;
		}

		private string GetShortText(string text)
		{
			text = text.Trim();
			return text.Substring(0, Math.Min(50, text.Length));
		}
	}
}
