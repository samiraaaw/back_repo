using Microsoft.AspNetCore.Mvc;
using GradoCerrado.Application.Interfaces;
using GradoCerrado.Domain.Entities;
using GradoCerrado.Infrastructure.DTOs;


namespace GradoCerrado.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudyController : ControllerBase
{
    private readonly ILogger<StudyController> _logger;
    private readonly IAIService _aiService;
    private readonly IVectorService _vectorService;

    public StudyController(
        ILogger<StudyController> logger,
        IAIService aiService,
        IVectorService vectorService)
    {
        _logger = logger;
        _aiService = aiService;
        _vectorService = vectorService;
    }

    // GET: api/study/registered-users
    [HttpGet("registered-users")]
    public ActionResult GetRegisteredUsers()
    {
        try
        {
            var users = new[]
            {
                new
                {
                    id = "2a5f109f-37da-41a6-91f1-d8df4b7ba02a",
                    name = "Coni",
                    email = "coni@gmail.com",
                    createdAt = "2025-09-25T03:00:10.427677Z"
                },
                new
                {
                    id = "9971d353-41e7-4a5c-a7c5-a6f620386ed5",
                    name = "alumno1",
                    email = "alumno1@gmail.com", 
                    createdAt = "2025-09-24T23:26:28.101947Z"
                }
            };

            return Ok(new
            {
                success = true,
                totalUsers = users.Length,
                users
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo usuarios registrados");
            return StatusCode(500, new { success = false, message = "Error consultando usuarios" });
        }
    }

    // POST: api/study/login
    [HttpPost("login")]
    public ActionResult Login([FromBody] LoginRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { success = false, message = "Email es obligatorio" });

            var user = new
            {
                id = Guid.NewGuid(),
                name = "Usuario de prueba",
                email = request.Email.Trim()
            };

            return Ok(new
            {
                success = true,
                message = "Login exitoso",
                user
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en login: {Email}", request.Email);
            return StatusCode(500, new { success = false, message = "Error interno del servidor" });
        }
    }

    // 🔥 NUEVO: POST: api/study/start-session (CON IA REAL)
    [HttpPost("start-session")]
    public async Task<ActionResult> StartSession([FromBody] StartSessionRequest request)
    {
        try
        {
            if (request.StudentId == Guid.Empty)
                return BadRequest(new { success = false, message = "StudentId es obligatorio" });

            _logger.LogInformation("Iniciando sesión con IA para estudiante: {StudentId}", request.StudentId);

            // Datos de sesión
            var sessionData = new
            {
                sessionId = Guid.NewGuid(),
                studentId = request.StudentId,
                startTime = DateTime.UtcNow,
                difficulty = request.Difficulty.ToString(),
                legalAreas = request.LegalAreas,
                status = "Active"
            };

            // 🧠 GENERAR PREGUNTAS CON IA
            var aiQuestions = await GenerateQuestionsWithAI(
                legalAreas: request.LegalAreas, 
                difficulty: request.Difficulty,
                count: 5
            );

            // Convertir formato IA a formato frontend
            var questions = aiQuestions.Select((q, index) => new
            {
                id = index + 1,
                questionText = q.QuestionText,
                type = q.Type.ToString().ToLower(),
                level = request.Difficulty.ToLower(),
                tema = request.LegalAreas.FirstOrDefault() ?? "Derecho General",
                options = q.Type == QuestionType.MultipleChoice ? 
                    q.Options?.Select(o => new { 
                        id = o.Id.ToString(), 
                        text = o.Text,
                        isCorrect = o.IsCorrect 
                    }).ToArray() : null,
                correctAnswer = q.Type == QuestionType.MultipleChoice ?
                q.Options?.FirstOrDefault(o => o.IsCorrect)?.Id.ToString() : 
                (q.IsTrue?.ToString().ToLower() ?? "true"),
                explanation = q.Explanation
            }).ToArray();

            _logger.LogInformation("Generadas {Count} preguntas con IA exitosamente", questions.Length);

            return Ok(new
            {
                success = true,
                session = sessionData,
                questions = questions,
                totalQuestions = questions.Length,
                generatedWithAI = true,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando sesión con IA");
            
            // 🛡️ FALLBACK: Si falla la IA, usar preguntas de ejemplo
            return await GetFallbackQuestions(request);
        }
    }

    // 🧠 MÉTODO PRINCIPAL: Generar preguntas con IA
    private async Task<List<StudyQuestion>> GenerateQuestionsWithAI(
        List<string> legalAreas, 
        string difficulty, 
        int count)
    {
        try
        {
            // 1. Buscar contenido relevante en Qdrant
            var searchQuery = $"derecho {string.Join(" ", legalAreas)}";
            var searchResults = await _vectorService.SearchSimilarAsync(searchQuery, limit: 3);

            // 2. Preparar contenido base
            string sourceContent;
            if (searchResults.Any())
            {
                sourceContent = string.Join("\n\n", searchResults.Select(r => r.Content));
                _logger.LogInformation("Usando {Count} documentos de Qdrant para generar preguntas", searchResults.Count);
            }
            else
            {
                // Contenido base si no hay documentos en Qdrant
                sourceContent = GetBaseLegalContent(legalAreas.FirstOrDefault() ?? "Derecho Civil");
                _logger.LogInformation("Usando contenido base predefinido para {Area}", legalAreas.FirstOrDefault());
            }

            // 3. Generar preguntas con OpenAI
            DifficultyLevel difficultyLevel;
            if (!Enum.TryParse<DifficultyLevel>(difficulty, true, out difficultyLevel))
            {
                difficultyLevel = DifficultyLevel.Intermediate;
            }

            var questionsJson = await _aiService.GenerateStructuredQuestionsAsync(
                topic: legalAreas.FirstOrDefault() ?? "Derecho General",
                context: sourceContent,
                type: QuestionType.MultipleChoice,
                difficulty: difficultyLevel,
                count: count
            );

            // 4. Parsear respuesta JSON
            var parsedQuestions = ParseAIQuestions(questionsJson);
            
            _logger.LogInformation("IA generó {Count} preguntas exitosamente", parsedQuestions.Count);
            return parsedQuestions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en generación IA");
            throw;
        }
    }

    // 📝 Contenido base si no hay documentos vectorizados
    private string GetBaseLegalContent(string area)
    {
        return area.ToLower() switch
        {
            "derecho civil" => @"
                El Derecho Civil chileno se basa en el Código Civil de Andrés Bello. 
                La capacidad jurídica se divide en capacidad de goce y capacidad de ejercicio.
                La capacidad de goce se adquiere desde el nacimiento y permite ser titular de derechos.
                La capacidad de ejercicio se adquiere con la mayoría de edad (18 años) y permite ejercer derechos.
                El patrimonio es el conjunto de derechos y obligaciones de una persona, evaluables en dinero.
                Los bienes se clasifican en muebles e inmuebles, corporales e incorporales.",
                
            "derecho penal" => @"
                El Derecho Penal chileno establece los delitos, faltas y sus penas correspondientes.
                Los delitos se clasifican según su gravedad en crímenes, simples delitos y faltas.
                La imputabilidad requiere capacidad de entender y querer en el momento del hecho.
                Las circunstancias modificatorias pueden agravar, atenuar o eximir de responsabilidad.",
                
            _ => @"
                El ordenamiento jurídico chileno se estructura en distintas ramas del derecho.
                La Constitución es la norma suprema del ordenamiento jurídico nacional.
                Las leyes se clasifican según su jerarquía y ámbito de aplicación.
                Los principios generales del derecho informan todo el sistema jurídico."
        };
    }

    // 🔧 Parsear respuesta JSON de OpenAI
    private List<StudyQuestion> ParseAIQuestions(string jsonResponse)
    {
        try
        {
            using var document = System.Text.Json.JsonDocument.Parse(jsonResponse);
            var questionsArray = document.RootElement.GetProperty("questions");
            
            var questions = new List<StudyQuestion>();
            
            foreach (var questionElement in questionsArray.EnumerateArray())
            {
                var question = new StudyQuestion
                {
                    Id = Guid.NewGuid(),
                    QuestionText = questionElement.GetProperty("questionText").GetString() ?? "",
                    Type = QuestionType.MultipleChoice,
                    Explanation = questionElement.TryGetProperty("explanation", out var exp) ? exp.GetString() : "",
                    CreatedAt = DateTime.UtcNow
                };

                // Parsear opciones si es selección múltiple
                if (questionElement.TryGetProperty("options", out var optionsElement))
                {
                    question.Options = new List<QuestionOption>();
                    foreach (var option in optionsElement.EnumerateArray())
                    {
                        question.Options.Add(new QuestionOption
                        {
                            Id = Guid.NewGuid(),
                            Text = option.GetProperty("text").GetString() ?? "",
                            IsCorrect = option.GetProperty("isCorrect").GetBoolean()
                        });
                    }
                }

                questions.Add(question);
            }

            return questions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parseando respuesta IA: {Response}", jsonResponse);
            throw new InvalidOperationException("Error parseando respuesta de IA", ex);
        }
    }

    // 🛡️ FALLBACK: Preguntas de ejemplo si falla la IA
    private async Task<ActionResult> GetFallbackQuestions(StartSessionRequest request)
    {
        _logger.LogWarning("Usando preguntas de fallback debido a error en IA");

        var sessionData = new
        {
            sessionId = Guid.NewGuid(),
            studentId = request.StudentId,
            startTime = DateTime.UtcNow,
            difficulty = request.Difficulty,
            legalAreas = request.LegalAreas,
            status = "Active"
        };

        var questions = new[]
        {
            new
            {
                id = 1,
                questionText = "¿Cuál de las siguientes afirmaciones sobre la capacidad jurídica es correcta?",
                type = "seleccion_multiple",
                level = request.Difficulty.ToLower(),
                tema = request.LegalAreas.FirstOrDefault() ?? "Derecho Civil",
                options = new[]
                {
                    new { id = "A", text = "La capacidad de goce se adquiere al cumplir la mayoría de edad" },
                    new { id = "B", text = "La capacidad de goce se adquiere desde el nacimiento" },
                    new { id = "C", text = "La capacidad de ejercicio se adquiere desde la concepción" },
                    new { id = "D", text = "Ambas capacidades se adquieren al mismo tiempo" }
                },
                correctAnswer = "B",
                explanation = "La capacidad de goce se adquiere desde el nacimiento y permite ser titular de derechos."
            }
        };

        return Ok(new
        {
            success = true,
            session = sessionData,
            questions = questions,
            totalQuestions = questions.Length,
            generatedWithAI = false,
            fallbackMode = true,
            timestamp = DateTime.UtcNow
        });
    }

    // POST: api/study/submit-answer
    [HttpPost("submit-answer")]
    public ActionResult SubmitAnswer([FromBody] SubmitAnswerRequest request)
    {
        try
        {
            var isCorrect = !string.IsNullOrWhiteSpace(request.UserAnswer) &&
                           !string.IsNullOrWhiteSpace(request.CorrectAnswer) &&
                           request.UserAnswer.Trim().Equals(request.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase);

            return Ok(new
            {
                success = true,
                isCorrect,
                correctAnswer = request.CorrectAnswer,
                explanation = string.IsNullOrWhiteSpace(request.Explanation)
                    ? "Explicación no disponible"
                    : request.Explanation,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando respuesta");
            return StatusCode(500, new { success = false, message = "Error procesando respuesta" });
        }
    }
}

// DTOs
public class RegisterStudentRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
}

public class StartSessionRequest
{
    public Guid StudentId { get; set; } = Guid.NewGuid();
    public string Difficulty { get; set; } = "basico";
    public List<string> LegalAreas { get; set; } = new();
}

public class SubmitAnswerRequest
{
    public string UserAnswer { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public string? Explanation { get; set; }
}