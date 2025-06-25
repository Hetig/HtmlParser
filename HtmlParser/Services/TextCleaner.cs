using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlParser.Services
{
	public class TextCleaner : ITextCleaner
	{
		public string CleanText(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return text;

			text = System.Net.WebUtility.HtmlDecode(text);
			text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
			return text.Trim();
		}
	}
}
