// src/GradoCerrado.Infrastructure/DTOs/QuestionResponseDTOs.cs

using System.Collections.Generic;

namespace GradoCerrado.Infrastructure.DTOs
{
    // DTOs para OpenAI responses
    public class MultipleChoiceResponse
    {
        public List<MultipleChoiceQuestionDto> Questions { get; set; } = new();
    }

    public class TrueFalseResponse
    {
        public List<TrueFalseQuestionDto> Questions { get; set; } = new();
    }

    public class MultipleChoiceQuestionDto
    {
        public string QuestionText { get; set; } = "";
        public List<QuestionOptionDto> Options { get; set; } = new();
        public string Explanation { get; set; } = "";
        public List<string> RelatedConcepts { get; set; } = new();
        public string Difficulty { get; set; } = "";
    }

    public class TrueFalseQuestionDto
    {
        public string QuestionText { get; set; } = "";
        public bool IsTrue { get; set; }
        public string Explanation { get; set; } = "";
        public List<string> RelatedConcepts { get; set; } = new();
        public string Difficulty { get; set; } = "";
    }

    public class QuestionOptionDto
    {
        public string Id { get; set; } = "";
        public string Text { get; set; } = "";
        public bool IsCorrect { get; set; }
    }

    // DTOs específicos para QuestionGenerationService
    public class OptionDto
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }

    public class SingleQuestionResponse
    {
        public string QuestionText { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public List<OptionDto>? Options { get; set; }
        public string CorrectAnswer { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
    }
}