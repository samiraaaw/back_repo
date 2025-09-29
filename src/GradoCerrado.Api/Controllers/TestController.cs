// src/GradoCerrado.Api/Controllers/TestController.cs
using GradoCerrado.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using GradoCerrado.Application.Interfaces;

namespace GradoCerrado.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

    // 🧪 PRUEBA 1: Verificar que las entidades se crean correctamente
    [HttpGet("test-entities")]
    public ActionResult TestEntities()
    {
        try
        {
            // Crear un estudiante de prueba
            var student = new Student
            {
                Id = Guid.NewGuid(),
                Name = "Juan Pérez Test",
                Email = "test@example.com",
                CurrentLevel = DifficultyLevel.Basic,
                RegistrationDate = DateTime.UtcNow,
                LastAccess = DateTime.UtcNow,
                IsActive = true
            };

            // Crear una pregunta de prueba
            var question = new StudyQuestion
            {
                Id = Guid.NewGuid(),
                QuestionText = "¿Cuál es la mayoría de edad en Chile?",
                Type = QuestionType.MultipleChoice,
                CorrectAnswer = "18 años",
                Explanation = "Según el Código Civil chileno, la mayoría de edad se alcanza a los 18 años.",
                LegalArea = "Civil",
                Difficulty = DifficultyLevel.Basic,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test",
                IsActive = true,
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Id = Guid.NewGuid(), Text = "16 años", IsCorrect = false },
                    new QuestionOption { Id = Guid.NewGuid(), Text = "18 años", IsCorrect = true },
                    new QuestionOption { Id = Guid.NewGuid(), Text = "21 años", IsCorrect = false },
                    new QuestionOption { Id = Guid.NewGuid(), Text = "25 años", IsCorrect = false }
                }
            };

            // Crear una sesión de estudio
            var session = new UserStudySession
            {
                Id = Guid.NewGuid(),
                UserId = student.Id,
                StartTime = DateTime.UtcNow,
                SelectedLegalAreas = new List<string> { "Civil" },
                Difficulty = DifficultyLevel.Basic,
                QuestionAttempts = new List<QuestionAttempt>()
            };

            // Crear un intento de respuesta
            var attempt = new QuestionAttempt
            {
                Id = Guid.NewGuid(),
                QuestionId = question.Id,
                UserId = student.Id,
                SessionId = session.Id,
                UserAnswer = "18 años",
                IsCorrect = true,
                AnsweredAt = DateTime.UtcNow,
                TimeSpent = TimeSpan.FromSeconds(30),
                ViewedExplanation = false
            };

            _logger.LogInformation("✅ Todas las entidades se crearon correctamente");

            return Ok(new
            {
                message = "✅ Entidades creadas correctamente",
                entities = new
                {
                    student = new { student.Id, student.Name, student.Email },
                    question = new { question.Id, question.QuestionText, question.LegalArea },
                    session = new { session.Id, session.Difficulty, AreaCount = session.SelectedLegalAreas.Count },
                    attempt = new { attempt.Id, attempt.UserAnswer, attempt.IsCorrect }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creando entidades de prueba");
            return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    // 🧪 PRUEBA 2: Probar integración con tus servicios existentes
    [HttpPost("test-ai-integration")]
    public async Task<ActionResult> TestAIIntegration([FromServices] IAIService? aiService)
    {
        try
        {
            if (aiService == null)
            {
                return Ok(new { message = "⚠️ IAIService no está configurado, pero las entidades funcionan" });
            }

            // Probar tu servicio de IA existente
            var questions = await aiService.GeneratePracticeQuestionsAsync("Derecho Civil", 3);

            _logger.LogInformation($"✅ IA generó {questions.Count} preguntas");

            return Ok(new
            {
                message = "✅ Integración con IA funcionando",
                questionsGenerated = questions.Count,
                questions = questions
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error probando integración con IA");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // 🧪 PRUEBA 3: Probar vector service
    [HttpPost("test-vector-integration")]
    public async Task<ActionResult> TestVectorIntegration([FromServices] IVectorService? vectorService)
    {
        try
        {
            if (vectorService == null)
            {
                return Ok(new { message = "⚠️ IVectorService no está configurado" });
            }

            // Probar búsqueda vectorial
            var results = await vectorService.SearchSimilarAsync("derecho civil", 3);

            _logger.LogInformation($"✅ Vector search retornó {results.Count} resultados");

            return Ok(new
            {
                message = "✅ Integración con vector service funcionando",
                resultsFound = results.Count,
                results = results.Select(r => new { r.Id, r.Score, Content = r.Content.Substring(0, Math.Min(100, r.Content.Length)) })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error probando vector service");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // 🧪 PRUEBA 4: Simular flujo completo
    [HttpPost("test-full-flow")]
    public async Task<ActionResult> TestFullFlow([FromServices] IAIService? aiService, [FromServices] IVectorService? vectorService)
    {
        try
        {
            var results = new List<string>();

            // 1. Crear estudiante
            var student = new Student
            {
                Id = Guid.NewGuid(),
                Name = "Usuario Prueba",
                Email = "prueba@test.com",
                CurrentLevel = DifficultyLevel.Intermediate
            };
            results.Add("✅ Estudiante creado");

            // 2. Generar preguntas con IA (si está disponible)
            List<string> aiQuestions = new();
            if (aiService != null)
            {
                aiQuestions = await aiService.GeneratePracticeQuestionsAsync("Derecho Penal", 2);
                results.Add($"✅ IA generó {aiQuestions.Count} preguntas");
            }
            else
            {
                aiQuestions = new List<string> { "¿Qué es el delito?", "¿Cuáles son los elementos del delito?" };
                results.Add("⚠️ Usando preguntas predefinidas (IA no disponible)");
            }

            // 3. Convertir a entidades StudyQuestion
            var studyQuestions = aiQuestions.Select(q => new StudyQuestion
            {
                Id = Guid.NewGuid(),
                QuestionText = q,
                Type = QuestionType.MultipleChoice,
                CorrectAnswer = "Respuesta simulada",
                LegalArea = "Penal",
                Difficulty = DifficultyLevel.Intermediate,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "AI-Test"
            }).ToList();
            results.Add($"✅ Convertidas a {studyQuestions.Count} entidades StudyQuestion");

            // 4. Crear sesión de estudio
            var session = new UserStudySession
            {
                Id = Guid.NewGuid(),
                UserId = student.Id,
                StartTime = DateTime.UtcNow,
                SelectedLegalAreas = new List<string> { "Penal" },
                Difficulty = DifficultyLevel.Intermediate
            };
            results.Add("✅ Sesión de estudio creada");

            // 5. Simular respuestas
            var attempts = studyQuestions.Select(q => new QuestionAttempt
            {
                Id = Guid.NewGuid(),
                QuestionId = q.Id,
                UserId = student.Id,
                SessionId = session.Id,
                UserAnswer = "Respuesta del usuario",
                IsCorrect = Random.Shared.NextDouble() > 0.5, // 50% de probabilidad
                AnsweredAt = DateTime.UtcNow,
                TimeSpent = TimeSpan.FromSeconds(Random.Shared.Next(15, 120))
            }).ToList();
            results.Add($"✅ Creados {attempts.Count} intentos de respuesta");

            // 6. Calcular estadísticas
            var correctAnswers = attempts.Count(a => a.IsCorrect);
            var successRate = attempts.Count > 0 ? (double)correctAnswers / attempts.Count * 100 : 0;
            results.Add($"✅ Estadísticas: {correctAnswers}/{attempts.Count} correctas ({successRate:F1}%)");

            _logger.LogInformation("✅ Flujo completo simulado exitosamente");

            return Ok(new
            {
                message = "✅ Flujo completo funcionando",
                student = new { student.Id, student.Name },
                session = new { session.Id, session.Difficulty },
                questions = studyQuestions.Count,
                attempts = attempts.Count,
                successRate = $"{successRate:F1}%",
                steps = results
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error en flujo completo");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}