// src/GradoCerrado.Infrastructure/Services/QuestionGenerationService.cs
using GradoCerrado.Application.Interfaces;
using GradoCerrado.Domain.Entities;
using GradoCerrado.Infrastructure.Configuration;
using GradoCerrado.Infrastructure.DTOs; // ✅ Usar DTOs compartidos
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System.Text.Json;

namespace GradoCerrado.Infrastructure.Services;

public class QuestionGenerationService : IQuestionGenerationService
{
    private readonly ChatClient _chatClient;
    private readonly IVectorService _vectorService;
    private readonly OpenAISettings _settings;

    public QuestionGenerationService(
        IOptions<OpenAISettings> settings,
        IVectorService vectorService)
    {
        _settings = settings.Value;
        var client = new OpenAIClient(_settings.ApiKey);
        _chatClient = client.GetChatClient(_settings.Model);
        _vectorService = vectorService;
    }

    public async Task<List<StudyQuestion>> GenerateQuestionsFromDocument(LegalDocument document, int count = 10)
    {
        var questions = new List<StudyQuestion>();

        // Dividir el documento en chunks más pequeños
        var chunks = SplitContentIntoChunks(document.Content, 1000);

        foreach (var chunk in chunks.Take(Math.Min(chunks.Count, 3))) // Máximo 3 chunks
        {
            try
            {
                var chunkQuestions = await GenerateQuestionsFromChunk(chunk, document, count / chunks.Count + 1);
                questions.AddRange(chunkQuestions);

                if (questions.Count >= count) break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generando preguntas del chunk: {ex.Message}");
            }
        }

        return questions.Take(count).ToList();
    }

    private async Task<List<StudyQuestion>> GenerateQuestionsFromChunk(
        string content,
        LegalDocument document,
        int count)
    {
        var questions = new List<StudyQuestion>();

        // Generar preguntas de selección múltiple (70% del total)
        var multipleChoiceCount = Math.Max(1, (int)Math.Ceiling(count * 0.7));
        var mcQuestions = await GenerateMultipleChoiceQuestions(content, document, multipleChoiceCount);
        questions.AddRange(mcQuestions);

        // Generar preguntas verdadero/falso (30% del total)
        var trueFalseCount = count - mcQuestions.Count;
        if (trueFalseCount > 0)
        {
            var tfQuestions = await GenerateTrueFalseQuestions(content, document, trueFalseCount);
            questions.AddRange(tfQuestions);
        }

        return questions;
    }

    // ✅ IMPLEMENTACIÓN: satisface la interfaz
    public async Task<List<StudyQuestion>> GenerateAndSaveQuestionsAsync(
        string sourceText,
        List<string> legalAreas,
        DifficultyLevel difficulty,
        int count)
    {
        // 1) Armamos un "documento" temporal con el texto fuente
        var tempDoc = new LegalDocument
        {
            Id = Guid.NewGuid(),
            Title = $"Generado desde texto libre ({string.Join(", ", legalAreas)})",
            Content = sourceText,
            LegalAreas = legalAreas,
            Difficulty = difficulty,
            DocumentType = LegalDocumentType.StudyMaterial,
            CreatedAt = DateTime.UtcNow
        };

        // 2) Reutilizamos tu pipeline existente
        var questions = await GenerateQuestionsFromDocument(tempDoc, count);

        // 3) (Opcional) Persistir: descomenta/ajusta según tu infra
        // await _questionRepository.BulkInsertAsync(questions);
        // await _unitOfWork.SaveChangesAsync();

        return questions;
    }

    private async Task<List<StudyQuestion>> GenerateMultipleChoiceQuestions(
        string content,
        LegalDocument document,
        int count)
    {
        var prompt = $@"TAREA: Genera exactamente {count} preguntas de selección múltiple para examen de grado de derecho chileno.

CONTENIDO A ANALIZAR:
{content}

CONTEXTO:
- Título: {document.Title}
- Áreas legales: {string.Join(", ", document.LegalAreas)}
- Dificultad: {document.Difficulty}

INSTRUCCIONES ESPECÍFICAS:
1. Cada pregunta debe tener exactamente 4 opciones (A, B, C, D)
2. Solo UNA opción es correcta
3. Las opciones incorrectas deben ser plausibles pero claramente erróneas
4. Incluye explicación clara de por qué la respuesta es correcta
5. Enfócate en conceptos, aplicaciones y análisis del contenido dado

FORMATO DE RESPUESTA - DEVUELVE SOLO JSON VÁLIDO:
{{
  ""questions"": [
    {{
      ""questionText"": ""¿Cuál de las siguientes afirmaciones sobre [concepto] es correcta?"",
      ""options"": [
        {{""id"": ""A"", ""text"": ""Primera opción"", ""isCorrect"": false}},
        {{""id"": ""B"", ""text"": ""Segunda opción (correcta)"", ""isCorrect"": true}},
        {{""id"": ""C"", ""text"": ""Tercera opción"", ""isCorrect"": false}},
        {{""id"": ""D"", ""text"": ""Cuarta opción"", ""isCorrect"": false}}
      ],
      ""explanation"": ""La respuesta correcta es B porque..."",
      ""relatedConcepts"": [""concepto1"", ""concepto2""],
      ""difficulty"": ""{document.Difficulty}""
    }}
  ]
}}";

        try
        {
            var response = await _chatClient.CompleteChatAsync(prompt);
            var jsonText = ExtractJsonFromResponse(response.Value.Content[0].Text);
            return ParseMultipleChoiceResponse(jsonText, document);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generando preguntas múltiple choice: {ex.Message}");
            return new List<StudyQuestion>();
        }
    }

    private async Task<List<StudyQuestion>> GenerateTrueFalseQuestions(
        string content,
        LegalDocument document,
        int count)
    {
        var prompt = $@"TAREA: Genera exactamente {count} preguntas de verdadero/falso para examen de grado de derecho chileno.

CONTENIDO A ANALIZAR:
{content}

CONTEXTO:
- Título: {document.Title}  
- Áreas legales: {string.Join(", ", document.LegalAreas)}
- Dificultad: {document.Difficulty}

INSTRUCCIONES:
1. Crea afirmaciones que sean claramente verdaderas o falsas según el contenido
2. Evita ambigüedades
3. Incluye explicación detallada
4. Varía entre afirmaciones verdaderas y falsas

FORMATO DE RESPUESTA - DEVUELVE SOLO JSON VÁLIDO:
{{
  ""questions"": [
    {{
      ""questionText"": ""El artículo X establece que..."",
      ""isTrue"": false,
      ""explanation"": ""Falso. El artículo X en realidad establece que..."",
      ""relatedConcepts"": [""concepto1"", ""concepto2""],
      ""difficulty"": ""{document.Difficulty}""
    }}
  ]
}}";

        try
        {
            var response = await _chatClient.CompleteChatAsync(prompt);
            var jsonText = ExtractJsonFromResponse(response.Value.Content[0].Text);
            return ParseTrueFalseResponse(jsonText, document);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generando preguntas verdadero/falso: {ex.Message}");
            return new List<StudyQuestion>();
        }
    }

    public async Task<List<StudyQuestion>> GenerateRandomQuestions(
        List<string> legalAreas,
        DifficultyLevel difficulty,
        int count = 5)
    {
        var questions = new List<StudyQuestion>();

        foreach (var area in legalAreas)
        {
            try
            {
                var relevantDocs = await _vectorService.SearchSimilarAsync(area, 2);

                if (relevantDocs.Any())
                {
                    var questionsPerArea = Math.Max(1, count / legalAreas.Count);
                    var areaQuestions = await GenerateQuestionsFromSearchResults(
                        relevantDocs, area, difficulty, questionsPerArea);
                    questions.AddRange(areaQuestions);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generando preguntas para área {area}: {ex.Message}");
            }
        }

        return questions.Take(count).ToList();
    }

    public async Task<StudyQuestion> GenerateFollowUpQuestion(StudyQuestion originalQuestion, bool wasCorrect)
    {
        var prompt = $@"El estudiante {(wasCorrect ? "ACERTÓ" : "FALLÓ")} esta pregunta:

PREGUNTA: {originalQuestion.QuestionText}
RESPUESTA CORRECTA: {originalQuestion.CorrectAnswer}
EXPLICACIÓN: {originalQuestion.Explanation}

Genera UNA pregunta de seguimiento que:
{(wasCorrect ?
    "- Profundice en el concepto con mayor complejidad\n- Explore aplicaciones prácticas del concepto" :
    "- Refuerce el concepto básico de manera más simple\n- Aclare posibles confusiones")}

FORMATO - DEVUELVE SOLO JSON:
{{
  ""questionText"": ""Nueva pregunta..."",
  ""type"": ""{originalQuestion.Type}"",
  ""options"": [...],
  ""correctAnswer"": ""respuesta"",
  ""explanation"": ""explicación..."",
  ""difficulty"": ""{originalQuestion.Difficulty}""
}}";

        try
        {
            var response = await _chatClient.CompleteChatAsync(prompt);
            var jsonText = ExtractJsonFromResponse(response.Value.Content[0].Text);
            return ParseSingleQuestionResponse(jsonText, originalQuestion);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generando pregunta de seguimiento: {ex.Message}");
            return originalQuestion; // Devolver la original si hay error
        }
    }

    // Métodos auxiliares
    private List<string> SplitContentIntoChunks(string content, int maxChunkSize)
    {
        var chunks = new List<string>();
        var sentences = content.Split('.', StringSplitOptions.RemoveEmptyEntries);
        var currentChunk = "";

        foreach (var sentence in sentences)
        {
            if (currentChunk.Length + sentence.Length > maxChunkSize && !string.IsNullOrEmpty(currentChunk))
            {
                chunks.Add(currentChunk.Trim());
                currentChunk = "";
            }
            currentChunk += sentence + ". ";
        }

        if (!string.IsNullOrEmpty(currentChunk))
        {
            chunks.Add(currentChunk.Trim());
        }

        return chunks;
    }

    private async Task<List<StudyQuestion>> GenerateQuestionsFromSearchResults(
        List<SearchResult> searchResults,
        string legalArea,
        DifficultyLevel difficulty,
        int count)
    {
        var combinedContent = string.Join("\n\n", searchResults.Take(2).Select(r => r.Content));

        // Crear documento temporal
        var tempDocument = new LegalDocument
        {
            Id = Guid.NewGuid(),
            Title = $"Material sobre {legalArea}",
            Content = combinedContent,
            LegalAreas = new List<string> { legalArea },
            Difficulty = difficulty,
            DocumentType = LegalDocumentType.StudyMaterial,
            CreatedAt = DateTime.UtcNow
        };

        return await GenerateQuestionsFromDocument(tempDocument, count);
    }

    private string ExtractJsonFromResponse(string response)
    {
        // Buscar el JSON en la respuesta
        var startIndex = response.IndexOf('{');
        var endIndex = response.LastIndexOf('}');

        if (startIndex >= 0 && endIndex > startIndex)
        {
            return response.Substring(startIndex, endIndex - startIndex + 1);
        }

        return response;
    }

    private List<StudyQuestion> ParseMultipleChoiceResponse(string jsonText, LegalDocument document)
    {
        try
        {
            var response = JsonSerializer.Deserialize<MultipleChoiceResponse>(jsonText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return response?.Questions?.Select(q => new StudyQuestion
            {
                Id = Guid.NewGuid(),
                QuestionText = q.QuestionText,
                Type = QuestionType.MultipleChoice,
                Options = q.Options?.Select(o => new QuestionOption
                {
                    Id = Guid.NewGuid(),
                    Text = o.Text,
                    IsCorrect = o.IsCorrect
                }).ToList() ?? new List<QuestionOption>(),
                CorrectAnswer = q.Options?.FirstOrDefault(o => o.IsCorrect)?.Id ?? "",
                Explanation = q.Explanation,
                LegalArea = string.Join(", ", document.LegalAreas),
                RelatedConcepts = q.RelatedConcepts ?? new List<string>(),
                Difficulty = document.Difficulty,
                SourceDocumentIds = new List<Guid> { document.Id },
                CreatedAt = DateTime.UtcNow
            }).ToList() ?? new List<StudyQuestion>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parseando respuesta múltiple choice: {ex.Message}");
            return new List<StudyQuestion>();
        }
    }

    private List<StudyQuestion> ParseTrueFalseResponse(string jsonText, LegalDocument document)
    {
        try
        {
            var response = JsonSerializer.Deserialize<TrueFalseResponse>(jsonText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return response?.Questions?.Select(q => new StudyQuestion
            {
                Id = Guid.NewGuid(),
                QuestionText = q.QuestionText,
                Type = QuestionType.TrueFalse,
                Options = new List<QuestionOption>
                {
                    new() { Id = Guid.NewGuid(), Text = "Verdadero", IsCorrect = q.IsTrue },
                    new() { Id = Guid.NewGuid(), Text = "Falso", IsCorrect = !q.IsTrue }
                },
                CorrectAnswer = q.IsTrue ? "True" : "False",
                Explanation = q.Explanation,
                LegalArea = string.Join(", ", document.LegalAreas),
                RelatedConcepts = q.RelatedConcepts ?? new List<string>(),
                Difficulty = document.Difficulty,
                SourceDocumentIds = new List<Guid> { document.Id },
                CreatedAt = DateTime.UtcNow
            }).ToList() ?? new List<StudyQuestion>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parseando respuesta verdadero/falso: {ex.Message}");
            return new List<StudyQuestion>();
        }
    }

    private StudyQuestion ParseSingleQuestionResponse(string jsonText, StudyQuestion originalQuestion)
    {
        try
        {
            var response = JsonSerializer.Deserialize<SingleQuestionResponse>(jsonText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (response == null) return originalQuestion;

            return new StudyQuestion
            {
                Id = Guid.NewGuid(),
                QuestionText = response.QuestionText,
                Type = originalQuestion.Type,
                Options = response.Options?.Select(o => new QuestionOption
                {
                    Id = Guid.NewGuid(),
                    Text = o.Text,
                    IsCorrect = o.IsCorrect
                }).ToList() ?? originalQuestion.Options,
                CorrectAnswer = response.CorrectAnswer,
                Explanation = response.Explanation,
                LegalArea = originalQuestion.LegalArea,
                RelatedConcepts = originalQuestion.RelatedConcepts,
                Difficulty = originalQuestion.Difficulty,
                CreatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parseando pregunta única: {ex.Message}");
            return originalQuestion;
        }
    }
}