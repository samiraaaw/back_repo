// src/GradoCerrado.Infrastructure/Services/OpenAIService.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GradoCerrado.Application.Interfaces;
using GradoCerrado.Domain.Entities;
using GradoCerrado.Infrastructure.Configuration;
using GradoCerrado.Infrastructure.DTOs;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace GradoCerrado.Infrastructure.Services
{
    public class OpenAIService : IAIService
    {
        private readonly ChatClient _chatClient;
        private readonly OpenAISettings _settings;

        public OpenAIService(IOptions<OpenAISettings> settings)
        {
            _settings = settings.Value;
            var client = new OpenAIClient(_settings.ApiKey);
            _chatClient = client.GetChatClient(_settings.Model);
        }

        // ✅ IMPLEMENTACIÓN: genera preguntas estructuradas (retorna string JSON)
        public async Task<string> GenerateStructuredQuestionsAsync(
            string sourceText,
            string legalArea,
            QuestionType type,
            DifficultyLevel difficulty,
            int count)
        {
            string formatExample = type == QuestionType.MultipleChoice
                ? @"{
  ""questions"": [
    {
      ""questionText"": ""¿...?"",
      ""options"": [
        { ""id"": ""A"", ""text"": ""opción"", ""isCorrect"": false },
        { ""id"": ""B"", ""text"": ""opción"", ""isCorrect"": true  },
        { ""id"": ""C"", ""text"": ""opción"", ""isCorrect"": false },
        { ""id"": ""D"", ""text"": ""opción"", ""isCorrect"": false }
      ],
      ""explanation"": ""... por qué ..."",
      ""relatedConcepts"": [""..."",""...""],
      ""difficulty"": """ + difficulty + @"""
    }
  ]
}"
                : @"{
  ""questions"": [
    {
      ""questionText"": ""Afirmación..."",
      ""isTrue"": true,
      ""explanation"": ""... por qué ..."",
      ""relatedConcepts"": [""..."",""...""],
      ""difficulty"": """ + difficulty + @"""
    }
  ]
}";

            var prompt = $@"
Eres un generador de preguntas para examen de grado de Derecho chileno.

GENERA EXACTAMENTE {count} preguntas del tipo {type} basadas ÚNICAMENTE en el TEXTO FUENTE.
- Área legal: {legalArea}
- Dificultad: {difficulty}
- Responde SOLO con JSON válido según el formato de ejemplo.

TEXTO FUENTE:
{sourceText}

FORMATO JSON (ejemplo, respétalo):
{formatExample}
";

            var completion = await _chatClient.CompleteChatAsync(prompt);
            var raw = completion.Value.Content[0].Text;

            return ExtractJson(raw);
        }

        // ✅ IMPLEMENTACIÓN: genera explicación para una respuesta
        public async Task<string> GenerateAnswerExplanationAsync(
            string questionText,
            string chosenAnswer,
            string correctAnswer,
            bool wasCorrect)
        {
            var prompt = $@"
Actúa como tutor de Derecho chileno.
Pregunta: {questionText}
Respuesta del estudiante: {chosenAnswer}
Respuesta correcta: {correctAnswer}
El estudiante {(wasCorrect ? "ACERTÓ" : "FALLÓ")}.

Explica de forma breve y clara:
1) Por qué la respuesta {(wasCorrect ? "es correcta" : "no es correcta")}
2) Fundamento conceptual (y, si aplica, referencia normativa del material)
3) Consejo para recordar

Devuelve SOLO el texto de la explicación (sin JSON).
";

            var completion = await _chatClient.CompleteChatAsync(prompt);
            return completion.Value.Content[0].Text.Trim();
        }

        // ✅ IMPLEMENTACIÓN: respuesta general con historial de chat
        public async Task<string> GenerateResponseAsync(string message, List<Domain.Entities.ChatMessage>? chatHistory = null)
        {
            var messages = new List<OpenAI.Chat.ChatMessage>();

            // Agregar contexto del sistema
            messages.Add(new SystemChatMessage("Eres un asistente especializado en Derecho chileno. Proporciona respuestas precisas y bien fundamentadas."));

            // Agregar historial si existe
            if (chatHistory != null && chatHistory.Any())
            {
                foreach (var msg in chatHistory.TakeLast(10))
                {
                    if (msg.Role == "user")
                        messages.Add(new UserChatMessage(msg.Content));
                    else if (msg.Role == "assistant")
                        messages.Add(new AssistantChatMessage(msg.Content));
                }
            }

            // Agregar mensaje actual
            messages.Add(new UserChatMessage(message));

            var completion = await _chatClient.CompleteChatAsync(messages);
            return completion.Value.Content[0].Text;
        }

        // ✅ IMPLEMENTACIÓN: genera plan de estudio
        public async Task<string> GenerateStudyPlanAsync(string topic, string timeframe)
        {
            var prompt = $@"
Crea un plan de estudio detallado para el tema: {topic}
Plazo disponible: {timeframe}

El plan debe incluir:
1. Objetivos de aprendizaje específicos
2. División temporal por etapas
3. Recursos recomendados
4. Métodos de estudio sugeridos
5. Evaluaciones intermedias
6. Cronograma semanal detallado

Enfócate en Derecho chileno y examen de grado.
Formato de respuesta: texto estructurado (no JSON).
";

            var completion = await _chatClient.CompleteChatAsync(prompt);
            return completion.Value.Content[0].Text.Trim();
        }

        // ✅ IMPLEMENTACIÓN: explica concepto legal
        public async Task<string> ExplainLegalConceptAsync(string concept)
        {
            var prompt = $@"
Explica el concepto jurídico: {concept}

Incluye:
1. Definición clara y precisa
2. Marco normativo aplicable (leyes, códigos relevantes)
3. Aplicación práctica en el derecho chileno
4. Ejemplos concretos
5. Relación con otros conceptos importantes
6. Jurisprudencia relevante si aplica

Respuesta dirigida a estudiantes de derecho chileno.
";

            var completion = await _chatClient.CompleteChatAsync(prompt);
            return completion.Value.Content[0].Text.Trim();
        }

        // ✅ IMPLEMENTACIÓN: genera preguntas de práctica - ARREGLADO EL TIPO DE RETORNO
        public async Task<List<string>> GeneratePracticeQuestionsAsync(string topic, int count = 5)
        {
            var prompt = $@"
Genera {count} preguntas de práctica sobre: {topic}

Incluye:
- Preguntas de selección múltiple (60%)
- Preguntas de verdadero/falso (40%)
- Mezcla diferentes niveles de dificultad
- Respuestas correctas al final
- Breve explicación para cada respuesta

Formato: texto estructurado legible (no JSON).
Enfoque: Derecho chileno, nivel examen de grado.
";

            var completion = await _chatClient.CompleteChatAsync(prompt);
            var response = completion.Value.Content[0].Text.Trim();

            // Dividir la respuesta en preguntas individuales
            var questions = response.Split(new[] { "\n\n", "Pregunta " }, StringSplitOptions.RemoveEmptyEntries)
                .Where(q => !string.IsNullOrWhiteSpace(q) && q.Contains("?"))
                .Take(count)
                .ToList();

            return questions.Any() ? questions : new List<string> { response };
        }

        // ✅ MÉTODO HELPER ADICIONAL: parsea respuesta JSON a entidades
        public async Task<List<StudyQuestion>> ParseStructuredQuestionsAsync(
            string jsonResponse,
            string legalArea,
            QuestionType type,
            DifficultyLevel difficulty)
        {
            try
            {
                if (type == QuestionType.MultipleChoice)
                {
                    var mc = JsonSerializer.Deserialize<MultipleChoiceResponse>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return (mc?.Questions ?? new()).Select(q => new StudyQuestion
                    {
                        Id = Guid.NewGuid(),
                        QuestionText = q.QuestionText,
                        Type = QuestionType.MultipleChoice,
                        Options = q.Options?.Select(o => new Domain.Entities.QuestionOption
                        {
                            Id = Guid.NewGuid(),
                            Text = o.Text,
                            IsCorrect = o.IsCorrect
                        }).ToList() ?? new List<Domain.Entities.QuestionOption>(),
                        CorrectAnswer = q.Options?.FirstOrDefault(o => o.IsCorrect)?.Id ?? "",
                        Explanation = q.Explanation,
                        LegalArea = legalArea,
                        RelatedConcepts = q.RelatedConcepts ?? new List<string>(),
                        Difficulty = difficulty,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();
                }
                else
                {
                    var tf = JsonSerializer.Deserialize<TrueFalseResponse>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return (tf?.Questions ?? new()).Select(q => new StudyQuestion
                    {
                        Id = Guid.NewGuid(),
                        QuestionText = q.QuestionText,
                        Type = QuestionType.TrueFalse,
                        Options = new List<Domain.Entities.QuestionOption>
                        {
                            new() { Id = Guid.NewGuid(), Text = "Verdadero", IsCorrect = q.IsTrue },
                            new() { Id = Guid.NewGuid(), Text = "Falso",     IsCorrect = !q.IsTrue }
                        },
                        CorrectAnswer = q.IsTrue ? "True" : "False",
                        Explanation = q.Explanation,
                        LegalArea = legalArea,
                        RelatedConcepts = q.RelatedConcepts ?? new List<string>(),
                        Difficulty = difficulty,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();
                }
            }
            catch
            {
                return new List<StudyQuestion>();
            }
        }

        // Helper method
        private static string ExtractJson(string text)
        {
            var si = text.IndexOf('{');
            var ei = text.LastIndexOf('}');
            return (si >= 0 && ei > si) ? text.Substring(si, ei - si + 1) : text;
        }
    }
}