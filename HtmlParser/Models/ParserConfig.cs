using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlParser.Models
{
	public class ParserConfig
	{
		public string BaseUrl { get; set; } = "https://brokennews.net";
		public string DateFormat { get; set; } = "yyyy-MM-dd";
	}
}
