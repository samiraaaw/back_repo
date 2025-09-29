namespace GradoCerrado.Application.DTOs;

public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
    public Guid? SessionId { get; set; }
}

public class ChatResponse
{
    public string Response { get; set; } = string.Empty;
    public Guid SessionId { get; set; }
    public DateTime Timestamp { get; set; }
}

public class StudyPlanRequest
{
    public string Topic { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "intermediate";
}

public class ConceptExplanationRequest
{
    public string Concept { get; set; } = string.Empty;
}

public class PracticeQuestionsRequest
{
    public string Topic { get; set; } = string.Empty;
    public int Count { get; set; } = 5;
}