using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlParser.Services
{
	public interface ITextCleaner
	{
		string CleanText(string text);
	}
}
