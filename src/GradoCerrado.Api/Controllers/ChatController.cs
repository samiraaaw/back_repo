using GradoCerrado.Application.DTOs;
using GradoCerrado.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GradoCerrado.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IAIService _aiService;
    private readonly IVectorService _vectorService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IAIService aiService, IVectorService vectorService, ILogger<ChatController> logger)
    {
        _aiService = aiService;
        _vectorService = vectorService;
        _logger = logger;
    }

    [HttpPost("message")]
    public async Task<ActionResult<ChatResponse>> SendMessage([FromBody] ChatRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("El mensaje no puede estar vacío");
            }

            // 1. Buscar documentos relevantes usando búsqueda vectorial
            var relevantDocs = await _vectorService.SearchSimilarAsync(request.Message, 3);
            
            // 2. Construir contexto con documentos encontrados
            var context = BuildContextFromDocuments(relevantDocs);
            
            // 3. Crear mensaje enriquecido con contexto para el AI
            var enrichedMessage = BuildEnrichedMessage(request.Message, context);
            
            // 4. Generar respuesta usando RAG
            var response = await _aiService.GenerateResponseAsync(enrichedMessage);
            
            _logger.LogInformation($"RAG aplicado: {relevantDocs.Count} documentos encontrados para la consulta");
            
            return Ok(new ChatResponse
            {
                Response = response,
                SessionId = request.SessionId ?? Guid.NewGuid(),
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar mensaje de chat con RAG");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [HttpPost("message-simple")]
    public async Task<ActionResult<ChatResponse>> SendSimpleMessage([FromBody] ChatRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("El mensaje no puede estar vacío");
            }

            // Respuesta simple sin RAG (para comparación)
            var response = await _aiService.GenerateResponseAsync(request.Message);
            
            return Ok(new ChatResponse
            {
                Response = response,
                SessionId = request.SessionId ?? Guid.NewGuid(),
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar mensaje simple");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [HttpPost("study-plan")]
    public async Task<ActionResult<string>> GenerateStudyPlan([FromBody] StudyPlanRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Topic))
            {
                return BadRequest("El tema no puede estar vacío");
            }

            // Buscar material relevante para el plan de estudio
            var relevantDocs = await _vectorService.SearchSimilarAsync(request.Topic, 5);
            var context = BuildContextFromDocuments(relevantDocs);
            
            // Crear prompt enriquecido para plan de estudio
            var enrichedTopic = $"Tema: {request.Topic}\n\nMaterial disponible:\n{context}";
            
            var studyPlan = await _aiService.GenerateStudyPlanAsync(enrichedTopic, request.Difficulty);
            return Ok(studyPlan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar plan de estudio");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [HttpPost("explain-concept")]
    public async Task<ActionResult<string>> ExplainConcept([FromBody] ConceptExplanationRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Concept))
            {
                return BadRequest("El concepto no puede estar vacío");
            }

            // Buscar documentos relacionados con el concepto
            var relevantDocs = await _vectorService.SearchSimilarAsync(request.Concept, 3);
            var context = BuildContextFromDocuments(relevantDocs);
            
            // Crear prompt enriquecido para explicación
            var enrichedConcept = $"Concepto: {request.Concept}\n\nInformación de referencia:\n{context}";
            
            var explanation = await _aiService.ExplainLegalConceptAsync(enrichedConcept);
            return Ok(explanation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al explicar concepto");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [HttpPost("practice-questions")]
    public async Task<ActionResult<List<string>>> GeneratePracticeQuestions([FromBody] PracticeQuestionsRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Topic))
            {
                return BadRequest("El tema no puede estar vacío");
            }

            if (request.Count <= 0 || request.Count > 20)
            {
                return BadRequest("El número de preguntas debe estar entre 1 y 20");
            }

            // Buscar material relevante para generar preguntas
            var relevantDocs = await _vectorService.SearchSimilarAsync(request.Topic, 5);
            var context = BuildContextFromDocuments(relevantDocs);
            
            // Crear prompt enriquecido para preguntas
            var enrichedTopic = $"Tema: {request.Topic}\n\nMaterial de referencia:\n{context}";
            
            var questions = await _aiService.GeneratePracticeQuestionsAsync(enrichedTopic, request.Count);
            return Ok(questions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar preguntas de práctica");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // Métodos auxiliares para RAG

    private string BuildContextFromDocuments(List<SearchResult> documents)
    {
        if (!documents.Any())
        {
            return "No se encontró información específica en la base de datos.";
        }

        var contextBuilder = new System.Text.StringBuilder();
        contextBuilder.AppendLine("INFORMACIÓN DE REFERENCIA:");
        
        for (int i = 0; i < documents.Count; i++)
        {
            var doc = documents[i];
            contextBuilder.AppendLine($"\n[Documento {i + 1}] (Relevancia: {doc.Score:F2})");
            
            if (doc.Metadata.ContainsKey("title"))
            {
                contextBuilder.AppendLine($"Título: {doc.Metadata["title"]}");
            }
            
            if (doc.Metadata.ContainsKey("category"))
            {
                contextBuilder.AppendLine($"Categoría: {doc.Metadata["category"]}");
            }
            
            contextBuilder.AppendLine($"Contenido: {doc.Content}");
            contextBuilder.AppendLine(new string('-', 50));
        }
        
        return contextBuilder.ToString();
    }

    private string BuildEnrichedMessage(string userMessage, string context)
    {
        return $@"CONTEXTO DE REFERENCIA:
{context}

CONSULTA DEL ESTUDIANTE:
{userMessage}

INSTRUCCIONES:
- Responde basándote PRINCIPALMENTE en la información de referencia proporcionada arriba
- Si la información de referencia no es suficiente, indícalo claramente
- Mantén un tono académico apropiado para estudiantes de derecho
- Cita las fuentes cuando sea relevante
- Si hay múltiples documentos, sintetiza la información de manera coherente";
    }
}