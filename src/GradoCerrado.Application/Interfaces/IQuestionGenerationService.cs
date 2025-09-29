// src/GradoCerrado.Application/Interfaces/IQuestionGenerationService.cs
using GradoCerrado.Domain.Entities;

namespace GradoCerrado.Application.Interfaces;

public interface IQuestionGenerationService
{
    // ✅ MANTENER - Solo cambiar el tipo de retorno para incluir más detalles
    Task<List<StudyQuestion>> GenerateQuestionsFromDocument(LegalDocument document, int count = 10);

    // ✅ MANTENER - Solo ajustar para usar tu enum DifficultyLevel
    Task<List<StudyQuestion>> GenerateRandomQuestions(List<string> legalAreas, DifficultyLevel difficulty, int count = 5);

    // ✅ MANTENER - Perfecta como está
    Task<StudyQuestion> GenerateFollowUpQuestion(StudyQuestion originalQuestion, bool wasCorrect);

    // 🆕 AGREGAR - Método para guardar preguntas en BD (opcional)
    Task<List<StudyQuestion>> GenerateAndSaveQuestionsAsync(string topic, List<string> legalAreas, DifficultyLevel difficulty, int count = 5);
}