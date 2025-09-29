// src/GradoCerrado.Application/Interfaces/IAIService.cs
using GradoCerrado.Domain.Entities;

namespace GradoCerrado.Application.Interfaces;

public interface IAIService
{
    // ✅ ACTUALIZADO - Usar el ChatMessage del dominio explícitamente
    Task<string> GenerateResponseAsync(string userMessage, List<Domain.Entities.ChatMessage>? conversationHistory = null);
    
    Task<string> GenerateStudyPlanAsync(string topic, string difficulty = "intermediate");
    
    Task<string> ExplainLegalConceptAsync(string concept);
    
    // ✅ ACTUALIZADO - Cambiar tipo de retorno a List<string>
    Task<List<string>> GeneratePracticeQuestionsAsync(string topic, int count = 5);

    // 🆕 AGREGAR - Para generar preguntas estructuradas con opciones
    Task<string> GenerateStructuredQuestionsAsync(string topic, string context, QuestionType type, DifficultyLevel difficulty, int count = 5);

    // 🆕 AGREGAR - Para generar explicaciones de respuestas
    Task<string> GenerateAnswerExplanationAsync(string question, string correctAnswer, string userAnswer, bool isCorrect);
}