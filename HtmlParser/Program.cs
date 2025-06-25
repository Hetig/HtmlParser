using HtmlParser.Models;
using HtmlParser.Services;
using System.Text.Json;

class Program
{
	static async Task Main(string[] args)
	{
		try
		{
			// Конфигурация
			var config = new ParserConfig();
			var textCleaner = new TextCleaner();
			var newsParser = new NewsParser(textCleaner, config);

			// Чтение HTML файла
			var htmlContent = await File.ReadAllTextAsync("corrupted-news.html");

			// Парсинг новостей
			var (news, errors) = newsParser.Parse(htmlContent);

			// Сохранение результатов
			await SaveResultsAsync(news, errors);

			Console.WriteLine("Processing completed successfully.");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error: {ex.Message}");
		}
	}

	private static async Task SaveResultsAsync(List<NewsItem> news, List<string> errors)
	{
		// Сохранение новостей в JSON
		var options = new JsonSerializerOptions
		{
			WriteIndented = true,
			Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};
		await File.WriteAllTextAsync("clean-news.json", JsonSerializer.Serialize(news, options));

		// Сохранение ошибок в лог
		await File.WriteAllLinesAsync("log.txt", errors);
	}
}