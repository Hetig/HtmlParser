using HtmlParser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlParser.Services
{
	public interface INewsParser
	{
		(List<NewsItem> news, List<string> errors) Parse(string htmlContent);
	}
}
