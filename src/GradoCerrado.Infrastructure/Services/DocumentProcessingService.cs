// src/GradoCerrado.Infrastructure/Services/DocumentProcessingService.cs
using GradoCerrado.Application.Interfaces;
using GradoCerrado.Domain.Entities;
using GradoCerrado.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GradoCerrado.Infrastructure.Services;

public class DocumentProcessingService : IDocumentProcessingService
{
	private readonly ChatClient _chatClient;
	private readonly OpenAISettings _settings;

	public DocumentProcessingService(IOptions<OpenAISettings> settings)
	{
		_settings = settings.Value;
		var client = new OpenAIClient(_settings.ApiKey);
		_chatClient = client.GetChatClient(_settings.Model);
	}

	public async Task<LegalDocument> ProcessDocumentAsync(string content, string fileName, LegalDocumentType? suggestedType = null)
	{
		var document = new LegalDocument
		{
			Id = Guid.NewGuid(),
			OriginalFileName = fileName,
			Content = content,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			Source = "Manual"
		};

		try
		{
			// Procesar en paralelo para eficiencia
			var tasks = new Task[]
			{
				ExtractTitleAndType(content, fileName, suggestedType).ContinueWith(t => {
					var result = t.Result;
					document.Title = result.title;
					document.DocumentType = result.type;
				}),
				ExtractKeyConcepts(content).ContinueWith(t => document.KeyConcepts = t.Result),
				IdentifyLegalAreas(content).ContinueWith(t => document.LegalAreas = t.Result),
				AssessDifficulty(content).ContinueWith(t => document.Difficulty = t.Result),
				ExtractArticleReferences(content).ContinueWith(t => document.Articles = t.Result),
				ExtractCaseReferences(content).ContinueWith(t => document.Cases = t.Result)
			};

			await Task.WhenAll(tasks);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error procesando documento: {ex.Message}");
			// Valores por defecto si falla el procesamiento
			document.Title = fileName;
			document.DocumentType = suggestedType ?? LegalDocumentType.StudyMaterial;
			document.Difficulty = DifficultyLevel.Intermediate;
		}

		return document;
	}

	private async Task<(string title, LegalDocumentType type)> ExtractTitleAndType(string content, string fileName, LegalDocumentType? suggestedType)
	{
		var prompt = $@"Analiza el siguiente documento jurídico chileno y extrae:
1. Un título descriptivo apropiado
2. El tipo de documento legal

Contenido: {content.Substring(0, Math.Min(content.Length, 1500))}...

Responde en formato JSON:
{{
    ""title"": ""título extraído o generado"",
    ""type"": ""Law|Decree|Jurisprudence|Doctrine|Constitution|Code|StudyMaterial|CaseStudy|Regulation""
}}";

		try
		{
			var response = await _chatClient.CompleteChatAsync(prompt);
			var jsonResponse = ExtractJsonFromResponse(response.Value.Content[0].Text);
			var result = JsonSerializer.Deserialize<TitleTypeResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

			var documentType = Enum.TryParse<LegalDocumentType>(result?.Type, out var parsedType)
				? parsedType
				: suggestedType ?? LegalDocumentType.StudyMaterial;

			return (result?.Title ?? fileName, documentType);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error extrayendo título y tipo: {ex.Message}");
			return (fileName, suggestedType ?? LegalDocumentType.StudyMaterial);
		}
	}

	public async Task<List<string>> ExtractKeyConcepts(string content)
	{
		var prompt = $@"Identifica los conceptos jurídicos clave en este documento de derecho chileno.
Enfócate en términos técnicos, instituciones, principios legales y conceptos fundamentales.

Contenido: {content.Substring(0, Math.Min(content.Length, 2000))}...

Responde con una lista JSON de conceptos:
[""concepto1"", ""concepto2"", ""concepto3""]";

		try
		{
			var response = await _chatClient.CompleteChatAsync(prompt);
			return ParseJsonArray(response.Value.Content[0].Text);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error extrayendo conceptos clave: {ex.Message}");
			return new List<string>();
		}
	}

	public async Task<List<string>> IdentifyLegalAreas(string content)
	{
		var prompt = $@"Identifica las áreas del derecho chileno que abarca este documento.
Considera: Derecho Civil, Penal, Constitucional, Administrativo, Laboral, Comercial, Procesal, etc.

Contenido: {content.Substring(0, Math.Min(content.Length, 1500))}...

Responde con una lista JSON de áreas:
[""area1"", ""area2"", ""area3""]";

		try
		{
			var response = await _chatClient.CompleteChatAsync(prompt);
			return ParseJsonArray(response.Value.Content[0].Text);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error identificando áreas legales: {ex.Message}");
			return new List<string> { "General" };
		}
	}

	public async Task<DifficultyLevel> AssessDifficulty(string content)
	{
		var prompt = $@"Evalúa la dificultad de este contenido jurídico para estudiantes de derecho:
- Basic: Conceptos fundamentales, definiciones básicas
- Intermediate: Aplicación de conceptos, casos prácticos
- Advanced: Análisis complejo, jurisprudencia especializada

Contenido: {content.Substring(0, Math.Min(content.Length, 1000))}...

Responde solo con: Basic, Intermediate o Advanced";

		try
		{
			var response = await _chatClient.CompleteChatAsync(prompt);
			var difficultyText = response.Value.Content[0].Text.Trim();

			return difficultyText.ToLower() switch
			{
				"basic" => DifficultyLevel.Basic,
				"intermediate" => DifficultyLevel.Intermediate,
				"advanced" => DifficultyLevel.Advanced,
				_ => DifficultyLevel.Intermediate
			};
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error evaluando dificultad: {ex.Message}");
			return DifficultyLevel.Intermediate;
		}
	}

	public async Task<List<string>> ExtractArticleReferences(string content)
	{
		try
		{
			// Usar regex para encontrar referencias a artículos
			var articlePattern = @"art(?:ículo)?\.?\s*(\d+(?:\.\d+)*(?:\s*(?:bis|ter|quater|quinquies))?(?:\s*[a-z])?)\s*(?:del?\s+(.+?))?(?:\.|,|;|$)";
			var matches = Regex.Matches(content, articlePattern, RegexOptions.IgnoreCase);

			var articles = matches
				.Cast<Match>()
				.Select(m => m.Groups[1].Value.Trim())
				.Distinct()
				.Take(20) // Limitar para evitar listas muy largas
				.ToList();

			// Complementar con AI si es necesario
			if (articles.Count < 3)
			{
				var aiArticles = await ExtractArticlesWithAI(content);
				articles.AddRange(aiArticles.Where(a => !articles.Contains(a)));
			}

			return articles;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error extrayendo referencias de artículos: {ex.Message}");
			return new List<string>();
		}
	}

	private async Task<List<string>> ExtractArticlesWithAI(string content)
	{
		var prompt = $@"Identifica todas las referencias a artículos de leyes, códigos o reglamentos en este texto jurídico chileno.

Contenido: {content.Substring(0, Math.Min(content.Length, 1500))}...

Responde con una lista JSON de números de artículos:
[""1"", ""25"", ""156 bis"", ""300 ter""]";

		try
		{
			var response = await _chatClient.CompleteChatAsync(prompt);
			return ParseJsonArray(response.Value.Content[0].Text);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error extrayendo artículos con AI: {ex.Message}");
			return new List<string>();
		}
	}

	public async Task<List<string>> ExtractCaseReferences(string content)
	{
		var prompt = $@"Identifica referencias a casos judiciales, sentencias o jurisprudencia en este documento jurídico chileno.
Busca nombres de casos, números de rol, referencias a tribunales.

Contenido: {content.Substring(0, Math.Min(content.Length, 1500))}...

Responde con una lista JSON de referencias:
[""caso1"", ""sentencia2"", ""rol3""]";

		try
		{
			var response = await _chatClient.CompleteChatAsync(prompt);
			return ParseJsonArray(response.Value.Content[0].Text);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error extrayendo referencias de casos: {ex.Message}");
			return new List<string>();
		}
	}

	// Métodos auxiliares
	private string ExtractJsonFromResponse(string response)
	{
		var startIndex = response.IndexOf('{');
		var endIndex = response.LastIndexOf('}');

		if (startIndex >= 0 && endIndex > startIndex)
		{
			return response.Substring(startIndex, endIndex - startIndex + 1);
		}

		return response;
	}

	private List<string> ParseJsonArray(string jsonText)
	{
		try
		{
			// Limpiar la respuesta para extraer solo el JSON
			var startIndex = jsonText.IndexOf('[');
			var endIndex = jsonText.LastIndexOf(']');

			if (startIndex >= 0 && endIndex > startIndex)
			{
				var jsonPart = jsonText.Substring(startIndex, endIndex - startIndex + 1);
				return JsonSerializer.Deserialize<List<string>>(jsonPart) ?? new List<string>();
			}

			return new List<string>();
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error parseando JSON array: {ex.Message}");
			return new List<string>();
		}
	}

	private class TitleTypeResponse
	{
		public string Title { get; set; } = string.Empty;
		public string Type { get; set; } = string.Empty;
	}
}