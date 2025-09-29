// src/GradoCerrado.Api/Controllers/DocumentController.cs
using GradoCerrado.Application.Interfaces;
using GradoCerrado.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text.RegularExpressions;

namespace GradoCerrado.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentController : ControllerBase
{
    private readonly IDocumentProcessingService _documentProcessing;
    private readonly IVectorService _vectorService;
    private readonly IQuestionGenerationService _questionGeneration;
    private readonly ILogger<DocumentController> _logger;

    public DocumentController(
        IDocumentProcessingService documentProcessing,
        IVectorService vectorService,
        IQuestionGenerationService questionGeneration,
        ILogger<DocumentController> logger)
    {
        _documentProcessing = documentProcessing;
        _vectorService = vectorService;
        _questionGeneration = questionGeneration;
        _logger = logger;
    }

    // ENDPOINTS PÚBLICOS

    [HttpPost("upload")]
    public async Task<ActionResult<EnhancedDocumentUploadResponse>> UploadDocument(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No se ha enviado ningún archivo");
            }

            var allowedTypes = new[] { ".txt", ".pdf", ".docx", ".md" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedTypes.Contains(fileExtension))
            {
                return BadRequest($"Tipo de archivo no soportado. Tipos permitidos: {string.Join(", ", allowedTypes)}");
            }

            string content = await ExtractContentFromFile(file, fileExtension);

            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest("El archivo está vacío o no se pudo extraer el contenido");
            }

            _logger.LogInformation($"Procesando documento: {file.FileName}");
            var document = await _documentProcessing.ProcessDocumentAsync(content, file.FileName);



            var chunks = await CreateDocumentChunks(content, 500, 100);
            var chunkIds = new List<string>();
            var baseMetadata = CreateBaseMetadata(document, file);

            for (int i = 0; i < chunks.Count; i++)
            {
                var chunkMetadata = new Dictionary<string, object>(baseMetadata)
                {
                    ["chunk_index"] = i,
                    ["chunk_id"] = $"{document.Id}_chunk_{i}",
                    ["total_chunks"] = chunks.Count
                };

                var vectorId = await _vectorService.AddDocumentAsync(chunks[i], chunkMetadata);
                chunkIds.Add(vectorId);
                _logger.LogInformation($"Chunk {i + 1}/{chunks.Count} almacenado con ID: {vectorId}");
            }

            var sampleQuestions = await _questionGeneration.GenerateQuestionsFromDocument(document, 5);

            _logger.LogInformation($"Documento procesado: {chunks.Count} chunks, {sampleQuestions.Count} preguntas generadas");

            return Ok(new EnhancedDocumentUploadResponse
            {
                Success = true,
                Document = document,
                ChunkIds = chunkIds,
                ChunksCreated = chunks.Count,
                TotalCharacters = content.Length,
                SampleQuestions = sampleQuestions,
                ProcessingTimeMs = 0,
                Message = $"Documento procesado exitosamente en {chunks.Count} fragmentos"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando documento: {FileName}", file?.FileName);
            return StatusCode(500, new EnhancedDocumentUploadResponse
            {
                Success = false,
                Message = $"Error procesando documento: {ex.Message}"
            });
        }
    }

    [HttpPost("upload-text")]
    public async Task<ActionResult<EnhancedDocumentUploadResponse>> UploadTextContent([FromBody] TextUploadRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest("El contenido no puede estar vacío");
            }

            var fileName = request.Title ?? "documento_manual.txt";
            var document = await _documentProcessing.ProcessDocumentAsync(request.Content, fileName, request.DocumentType);

            var chunks = await CreateDocumentChunks(request.Content, 1000, 200);
            var chunkIds = new List<string>();

            var baseMetadata = new Dictionary<string, object>
            {
                ["title"] = document.Title,
                ["document_type"] = document.DocumentType.ToString(),
                ["legal_areas"] = string.Join(",", document.LegalAreas),
                ["key_concepts"] = string.Join(",", document.KeyConcepts),
                ["difficulty"] = document.Difficulty.ToString(),
                ["articles"] = string.Join(",", document.Articles),
                ["cases"] = string.Join(",", document.Cases),
                ["source"] = "Manual",
                ["created_at"] = document.CreatedAt.ToString("O"),
                ["document_id"] = document.Id.ToString(),
                ["file_name"] = fileName,
                ["file_size"] = request.Content.Length,
                ["mime_type"] = "text/plain"
            };

            for (int i = 0; i < chunks.Count; i++)
            {
                var chunkMetadata = new Dictionary<string, object>(baseMetadata)
                {
                    ["chunk_index"] = i,
                    ["chunk_id"] = $"{document.Id}_chunk_{i}",
                    ["total_chunks"] = chunks.Count
                };

                var vectorId = await _vectorService.AddDocumentAsync(chunks[i], chunkMetadata);
                chunkIds.Add(vectorId);
            }

            var questionCount = request.GenerateQuestions ? (request.QuestionCount ?? 5) : 0;
            var sampleQuestions = questionCount > 0
                ? await _questionGeneration.GenerateQuestionsFromDocument(document, questionCount)
                : new List<StudyQuestion>();

            return Ok(new EnhancedDocumentUploadResponse
            {
                Success = true,
                Document = document,
                ChunkIds = chunkIds,
                ChunksCreated = chunks.Count,
                TotalCharacters = request.Content.Length,
                SampleQuestions = sampleQuestions,
                Message = $"Documento procesado exitosamente en {chunks.Count} fragmentos"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando contenido de texto");
            return StatusCode(500, new EnhancedDocumentUploadResponse
            {
                Success = false,
                Message = $"Error procesando documento: {ex.Message}"
            });
        }
    }

    [HttpGet("status")]
    public async Task<ActionResult<DocumentSystemStatus>> GetStatus()
    {
        try
        {
            var collectionExists = await _vectorService.CollectionExistsAsync();

            return Ok(new DocumentSystemStatus
            {
                VectorDatabaseReady = collectionExists,
                Message = collectionExists ? "Sistema listo para recibir documentos" : "Base de datos vectorial no inicializada"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando estado del sistema");
            return StatusCode(500, new DocumentSystemStatus
            {
                VectorDatabaseReady = false,
                Message = $"Error verificando estado: {ex.Message}"
            });
        }
    }

    [HttpPost("generate-questions")]
    public async Task<ActionResult<List<StudyQuestion>>> GenerateQuestionsFromVector([FromBody] GenerateQuestionsRequest request)
    {
        try
        {
            if (request.LegalAreas == null || !request.LegalAreas.Any())
            {
                return BadRequest("Debe especificar al menos un área legal");
            }

            List<StudyQuestion> questions;

            if (!string.IsNullOrWhiteSpace(request.SearchQuery))
            {
                var relevantDocs = await _vectorService.SearchSimilarAsync(request.SearchQuery, 3);
                questions = await GenerateQuestionsFromSearchResults(relevantDocs, request);
            }
            else
            {
                questions = await _questionGeneration.GenerateRandomQuestions(
                    request.LegalAreas,
                    request.Difficulty,
                    request.Count);
            }

            return Ok(questions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando preguntas");
            return StatusCode(500, $"Error generando preguntas: {ex.Message}");
        }
    }

    [HttpPost("test-document")]
    public async Task<ActionResult<EnhancedDocumentUploadResponse>> TestWithSampleDocument()
    {
        try
        {
            var sampleContent = @"
ARTÍCULO 19 DE LA CONSTITUCIÓN POLÍTICA DE CHILE

El artículo 19 de la Constitución Política de la República de Chile garantiza a todas las personas los siguientes derechos:

1. El derecho a la vida y a la integridad física y psíquica de la persona.
2. La igualdad ante la ley, estableciendo que ni la ley ni autoridad alguna podrán establecer diferencias arbitrarias.
3. La igual protección de la ley en el ejercicio de sus derechos.
4. El respeto y protección a la vida privada y a la honra de la persona y su familia.
5. La inviolabilidad del hogar y de toda forma de comunicación privada.
6. La libertad de conciencia, la manifestación de todas las creencias y el ejercicio libre de todos los cultos.
7. El derecho a la libertad personal y a la seguridad individual.
8. El derecho a vivir en un medio ambiente libre de contaminación.

PRINCIPIOS FUNDAMENTALES:

- Supremacía Constitucional: La Constitución es la norma suprema del ordenamiento jurídico.
- Estado de Derecho: Todas las personas e instituciones están sujetas a la ley.
- Separación de Poderes: División del poder público en Ejecutivo, Legislativo y Judicial.

RECURSOS DE PROTECCIÓN:

El recurso de protección procede cuando se vulneran los derechos establecidos en el artículo 19, específicamente los números 1, 2, 3, 4, 5, 6, 9, 11, 12, 15, 16, 19, 21, 22, 23, 24 y 25.

JURISPRUDENCIA RELEVANTE:

- Caso Palamara Iribarne vs. Chile (2005): Sobre libertad de expresión y debido proceso.
- Caso Atala Riffo vs. Chile (2012): Sobre discriminación y vida privada.
";

            var document = await _documentProcessing.ProcessDocumentAsync(
                sampleContent,
                "articulo_19_constitucion.txt",
                LegalDocumentType.Constitution);

            var chunks = await CreateDocumentChunks(sampleContent, 1000, 200);
            var chunkIds = new List<string>();

            var baseMetadata = new Dictionary<string, object>
            {
                ["title"] = document.Title,
                ["document_type"] = document.DocumentType.ToString(),
                ["legal_areas"] = string.Join(",", document.LegalAreas),
                ["key_concepts"] = string.Join(",", document.KeyConcepts),
                ["difficulty"] = document.Difficulty.ToString(),
                ["source"] = "Sample",
                ["document_id"] = document.Id.ToString(),
                ["file_name"] = "articulo_19_constitucion.txt",
                ["file_size"] = sampleContent.Length,
                ["mime_type"] = "text/plain"
            };

            for (int i = 0; i < chunks.Count; i++)
            {
                var chunkMetadata = new Dictionary<string, object>(baseMetadata)
                {
                    ["chunk_index"] = i,
                    ["chunk_id"] = $"{document.Id}_chunk_{i}",
                    ["total_chunks"] = chunks.Count
                };

                var vectorId = await _vectorService.AddDocumentAsync(chunks[i], chunkMetadata);
                chunkIds.Add(vectorId);
            }

            var questions = await _questionGeneration.GenerateQuestionsFromDocument(document, 8);

            return Ok(new EnhancedDocumentUploadResponse
            {
                Success = true,
                Document = document,
                ChunkIds = chunkIds,
                ChunksCreated = chunks.Count,
                TotalCharacters = sampleContent.Length,
                SampleQuestions = questions,
                Message = "Documento de prueba procesado exitosamente"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error con documento de prueba");
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    [HttpGet("list")]
    public async Task<ActionResult<List<DocumentSummary>>> ListDocuments(
        [FromQuery] LegalDocumentType? documentType = null,
        [FromQuery] string? legalArea = null,
        [FromQuery] DifficultyLevel? difficulty = null)
    {
        try
        {
            var allResults = await _vectorService.SearchSimilarAsync("documento", 1000);

            var documentGroups = allResults
                .Where(doc => doc.Metadata.ContainsKey("document_id"))
                .GroupBy(doc => doc.Metadata["document_id"].ToString())
                .Select(group => CreateDocumentSummary(group))
                .Where(doc => doc != null)
                .Cast<DocumentSummary>()
                .ToList();

            if (documentType.HasValue)
                documentGroups = documentGroups.Where(d => d.DocumentType == documentType.Value).ToList();

            if (!string.IsNullOrEmpty(legalArea))
                documentGroups = documentGroups.Where(d => d.LegalAreas.Contains(legalArea)).ToList();

            if (difficulty.HasValue)
                documentGroups = documentGroups.Where(d => d.Difficulty == difficulty.Value).ToList();

            return Ok(documentGroups.OrderByDescending(d => d.CreatedAt).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar documentos");
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    [HttpPost("search")]
    public async Task<ActionResult> SearchDocuments([FromBody] DocumentSearchRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest("La consulta no puede estar vacía");
            }

            var results = await _vectorService.SearchSimilarAsync(request.Query, request.Limit);

            return Ok(new
            {
                query = request.Query,
                results = results.Select(r => new
                {
                    id = r.Id,
                    content = r.Content,
                    score = r.Score,
                    metadata = r.Metadata
                }),
                count = results.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar documentos");
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    [HttpDelete("{documentId}")]
    public async Task<ActionResult> DeleteDocument(Guid documentId)
    {
        try
        {
            var allResults = await _vectorService.SearchSimilarAsync("", 1000);
            var documentChunks = allResults
                .Where(doc => doc.Metadata.ContainsKey("document_id") &&
                             doc.Metadata["document_id"].ToString() == documentId.ToString())
                .ToList();

            if (!documentChunks.Any())
            {
                return NotFound("Documento no encontrado");
            }

            int deletedCount = 0;
            foreach (var chunk in documentChunks)
            {
                var result = await _vectorService.DeleteDocumentAsync(chunk.Id);
                if (result) deletedCount++;
            }

            return Ok(new
            {
                message = $"Documento eliminado: {deletedCount}/{documentChunks.Count} chunks eliminados",
                success = deletedCount == documentChunks.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar documento");
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

   

    // AGREGAR este método helper al DocumentController
    private async Task<LegalDocument?> GetDocumentByIdAsync(Guid documentId)
    {
        try
        {
            var allResults = await _vectorService.SearchSimilarAsync("documento", 1000);
            var documentChunks = allResults
                .Where(doc => doc.Metadata.ContainsKey("document_id") &&
                             doc.Metadata["document_id"].ToString() == documentId.ToString())
                .ToList();

            if (!documentChunks.Any())
                return null;

            var firstChunk = documentChunks.First();
            var metadata = firstChunk.Metadata;

            return new LegalDocument
            {
                Id = documentId,
                Title = metadata.GetValueOrDefault("title", "").ToString()!,
                DocumentType = Enum.Parse<LegalDocumentType>(metadata.GetValueOrDefault("document_type", "StudyMaterial").ToString()!),
                LegalAreas = metadata.GetValueOrDefault("legal_areas", "").ToString()!.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                KeyConcepts = metadata.GetValueOrDefault("key_concepts", "").ToString()!.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                Difficulty = Enum.Parse<DifficultyLevel>(metadata.GetValueOrDefault("difficulty", "Intermediate").ToString()!),
                Source = metadata.GetValueOrDefault("source", "").ToString()!,
                CreatedAt = DateTime.Parse(metadata.GetValueOrDefault("created_at", DateTime.MinValue.ToString("O")).ToString()!),
                Content = string.Join("\n", documentChunks.Select(c => c.Content))
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error obteniendo documento {documentId}");
            return null;
        }
    }

    public class StudyQuestionsResponse
    {
        public Guid DocumentId { get; set; }
        public string DocumentTitle { get; set; } = string.Empty;
        public Guid StudentId { get; set; }
        public Guid? SessionId { get; set; }
        public List<QuestionForStudent> Questions { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    public class QuestionForStudent
    {
        public Guid Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public List<QuestionOptionDto> Options { get; set; } = new();
        public string LegalArea { get; set; } = string.Empty;
        public DifficultyLevel Difficulty { get; set; }
        public List<string> RelatedConcepts { get; set; } = new();
    }

    public class QuestionOptionDto
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }

    // MÉTODOS PRIVADOS DE EXTRACCIÓN

    private async Task<string> ExtractContentFromFile(IFormFile file, string fileExtension)
    {
        return fileExtension switch
        {
            ".txt" or ".md" => await ExtractTextFromPlainFile(file),
            ".pdf" => await ExtractTextFromPdfWithFallback(file),
            ".docx" => await ExtractTextFromDocx(file),
            _ => throw new NotSupportedException($"Tipo de archivo no soportado: {fileExtension}")
        };
    }

    private async Task<string> ExtractTextFromPlainFile(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    private async Task<string> ExtractTextFromPdf(IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            using var pdfReader = new PdfReader(stream);
            using var pdfDocument = new PdfDocument(pdfReader);

            var textBuilder = new StringBuilder();
            var totalPages = pdfDocument.GetNumberOfPages();

            _logger.LogInformation($"Procesando PDF: {file.FileName} con {totalPages} páginas");

            for (int i = 1; i <= totalPages; i++)
            {
                try
                {
                    var page = pdfDocument.GetPage(i);
                    var text = PdfTextExtractor.GetTextFromPage(page);

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        textBuilder.AppendLine(text);
                        _logger.LogDebug($"Página {i}: {text.Length} caracteres extraídos");
                    }
                    else
                    {
                        _logger.LogWarning($"Página {i}: No se pudo extraer texto o está vacía");
                    }
                }
                catch (Exception pageEx)
                {
                    _logger.LogWarning(pageEx, $"Error procesando página {i}, intentando método alternativo");

                    try
                    {
                        var page = pdfDocument.GetPage(i);
                        var strategy = new iText.Kernel.Pdf.Canvas.Parser.Listener.SimpleTextExtractionStrategy();
                        var text = PdfTextExtractor.GetTextFromPage(page, strategy);

                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            textBuilder.AppendLine(text);
                            _logger.LogInformation($"Página {i}: Extraída con método alternativo");
                        }
                    }
                    catch (Exception alternativeEx)
                    {
                        _logger.LogError(alternativeEx, $"No se pudo procesar página {i}, saltando...");
                        textBuilder.AppendLine($"\n[Página {i}: Error de procesamiento - contenido omitido]\n");
                    }
                }
            }

            var extractedText = textBuilder.ToString();

            if (string.IsNullOrWhiteSpace(extractedText))
            {
                throw new InvalidOperationException($"No se pudo extraer texto del PDF {file.FileName}. El archivo podría estar protegido, dañado o ser solo imágenes.");
            }

            _logger.LogInformation($"PDF procesado exitosamente: {extractedText.Length} caracteres extraídos de {totalPages} páginas");
            return CleanExtractedText(extractedText);
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error general extrayendo texto de PDF: {file.FileName}");
            throw new InvalidOperationException($"Error al procesar archivo PDF: {ex.Message}", ex);
        }
    }

    private async Task<string> ExtractTextFromPdfFallback(IFormFile file)
    {
        try
        {
            _logger.LogInformation($"Intentando extracción de fallback para PDF: {file.FileName}");

            using var stream = file.OpenReadStream();
            using var pdfReader = new PdfReader(stream);
            using var pdfDocument = new PdfDocument(pdfReader);

            var textBuilder = new StringBuilder();
            var totalPages = pdfDocument.GetNumberOfPages();

            for (int i = 1; i <= totalPages; i++)
            {
                try
                {
                    var page = pdfDocument.GetPage(i);
                    string pageText = null;

                    try
                    {
                        var locationStrategy = new iText.Kernel.Pdf.Canvas.Parser.Listener.LocationTextExtractionStrategy();
                        pageText = PdfTextExtractor.GetTextFromPage(page, locationStrategy);
                    }
                    catch
                    {
                        var simpleStrategy = new iText.Kernel.Pdf.Canvas.Parser.Listener.SimpleTextExtractionStrategy();
                        pageText = PdfTextExtractor.GetTextFromPage(page, simpleStrategy);
                    }

                    if (!string.IsNullOrWhiteSpace(pageText))
                    {
                        textBuilder.AppendLine(pageText);
                    }
                }
                catch (Exception pageEx)
                {
                    _logger.LogWarning(pageEx, $"Página {i} omitida debido a errores de procesamiento");
                    textBuilder.AppendLine($"\n[Página {i}: Contenido no disponible]\n");
                }
            }

            return CleanExtractedText(textBuilder.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en extracción de fallback");
            throw new InvalidOperationException("No se pudo procesar el PDF con ningún método disponible", ex);
        }
    }

    private async Task<string> ExtractTextFromPdfWithFallback(IFormFile file)
    {
        try
        {
            return await ExtractTextFromPdf(file);
        }
        catch (Exception primaryEx)
        {
            _logger.LogWarning(primaryEx, $"Método principal falló para {file.FileName}, intentando fallback...");

            try
            {
                var fallbackResult = await ExtractTextFromPdfFallback(file);

                if (string.IsNullOrWhiteSpace(fallbackResult))
                {
                    throw new InvalidOperationException($"Ningún método pudo extraer texto del PDF {file.FileName}");
                }

                _logger.LogInformation($"PDF procesado exitosamente con método de fallback: {file.FileName}");
                return fallbackResult;
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, $"Ambos métodos fallaron para {file.FileName}");

                throw new InvalidOperationException(
                    $"No se pudo procesar el PDF '{file.FileName}'. " +
                    $"Posibles causas: archivo protegido, dañado, o contiene solo imágenes. " +
                    $"Error principal: {primaryEx.Message}. " +
                    $"Error fallback: {fallbackEx.Message}",
                    primaryEx);
            }
        }
    }

    private async Task<string> ExtractTextFromDocx(IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            using var wordDocument = WordprocessingDocument.Open(stream, false);

            var body = wordDocument.MainDocumentPart?.Document?.Body;
            if (body == null)
                throw new InvalidOperationException("No se pudo acceder al contenido del documento DOCX");

            var textBuilder = new StringBuilder();

            foreach (var paragraph in body.Elements<Paragraph>())
            {
                var paragraphText = GetParagraphText(paragraph);
                if (!string.IsNullOrWhiteSpace(paragraphText))
                {
                    textBuilder.AppendLine(paragraphText);
                }
            }

            foreach (var table in body.Elements<Table>())
            {
                var tableText = GetTableText(table);
                if (!string.IsNullOrWhiteSpace(tableText))
                {
                    textBuilder.AppendLine(tableText);
                }
            }

            var extractedText = textBuilder.ToString();
            _logger.LogInformation($"Texto extraído de DOCX: {extractedText.Length} caracteres");

            return CleanExtractedText(extractedText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extrayendo texto de DOCX");
            throw new InvalidOperationException("Error al procesar archivo DOCX", ex);
        }
    }

    // MÉTODOS HELPER

    private string GetParagraphText(Paragraph paragraph)
    {
        var textBuilder = new StringBuilder();
        foreach (var run in paragraph.Elements<Run>())
        {
            foreach (var text in run.Elements<Text>())
            {
                textBuilder.Append(text.Text);
            }
        }
        return textBuilder.ToString();
    }

    private string GetTableText(Table table)
    {
        var textBuilder = new StringBuilder();

        foreach (var row in table.Elements<TableRow>())
        {
            var rowTexts = new List<string>();

            foreach (var cell in row.Elements<TableCell>())
            {
                var cellText = new StringBuilder();
                foreach (var paragraph in cell.Elements<Paragraph>())
                {
                    cellText.Append(GetParagraphText(paragraph));
                }
                rowTexts.Add(cellText.ToString().Trim());
            }

            if (rowTexts.Any(t => !string.IsNullOrWhiteSpace(t)))
            {
                textBuilder.AppendLine(string.Join(" | ", rowTexts));
            }
        }

        return textBuilder.ToString();
    }

    private string CleanExtractedText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
        text = System.Text.RegularExpressions.Regex.Replace(text, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", "");
        return text.Trim();
    }

    // REEMPLAZAR el método CreateDocumentChunks en tu DocumentController.cs

    private async Task<List<string>> CreateDocumentChunks(string text, int maxChunkSize = 500, int overlap = 100)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<string>();

        var chunks = new List<string>();

        // 🚨 CHUNKS MÁS PEQUEÑOS para evitar límite de OpenAI
        // Cambio: maxChunkSize de 1000 → 500 caracteres
        // Esto es ~125 tokens aprox (1 token ≈ 4 caracteres)

        // Estrategia 1: Dividir por párrafos primero
        var paragraphs = text.Split(new[] { "\n\n", "\r\n\r\n", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        var currentChunk = new StringBuilder();

        foreach (var paragraph in paragraphs)
        {
            var cleanParagraph = paragraph.Trim();

            if (string.IsNullOrWhiteSpace(cleanParagraph))
                continue;

            // Si agregar este párrafo excedería el límite
            if (currentChunk.Length + cleanParagraph.Length > maxChunkSize && currentChunk.Length > 0)
            {
                // Guardar chunk actual
                var chunkText = currentChunk.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(chunkText))
                {
                    chunks.Add(chunkText);
                }

                // Nuevo chunk con overlap
                var overlapText = GetLastWords(chunkText, overlap);
                currentChunk.Clear();
                if (!string.IsNullOrWhiteSpace(overlapText))
                {
                    currentChunk.Append(overlapText + " ");
                }
            }

            // Si el párrafo mismo es muy largo, dividirlo por oraciones
            if (cleanParagraph.Length > maxChunkSize)
            {
                var sentences = SplitIntoSentences(cleanParagraph);
                foreach (var sentence in sentences)
                {
                    if (currentChunk.Length + sentence.Length > maxChunkSize && currentChunk.Length > 0)
                    {
                        var chunkText = currentChunk.ToString().Trim();
                        if (!string.IsNullOrWhiteSpace(chunkText))
                        {
                            chunks.Add(chunkText);
                        }

                        var overlapText = GetLastWords(chunkText, overlap);
                        currentChunk.Clear();
                        if (!string.IsNullOrWhiteSpace(overlapText))
                        {
                            currentChunk.Append(overlapText + " ");
                        }
                    }

                    currentChunk.Append(sentence + " ");
                }
            }
            else
            {
                currentChunk.Append(cleanParagraph + " ");
            }
        }

        // Agregar último chunk si no está vacío
        if (currentChunk.Length > 0)
        {
            var finalChunk = currentChunk.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(finalChunk))
            {
                chunks.Add(finalChunk);
            }
        }

        // 🛡️ VALIDACIÓN FINAL: Asegurar que ningún chunk exceda el límite
        var validatedChunks = new List<string>();
        foreach (var chunk in chunks)
        {
            if (chunk.Length <= maxChunkSize)
            {
                validatedChunks.Add(chunk);
            }
            else
            {
                // Si un chunk todavía es muy largo, dividirlo más
                var subChunks = ForceChunkSplit(chunk, maxChunkSize, overlap);
                validatedChunks.AddRange(subChunks);
            }
        }

        _logger.LogInformation($"Documento dividido en {validatedChunks.Count} chunks. Tamaño promedio: {validatedChunks.Average(c => c.Length):F0} caracteres");

        // Log de chunks muy grandes para debugging
        var largeChunks = validatedChunks.Where(c => c.Length > 400).ToList();
        if (largeChunks.Any())
        {
            _logger.LogWarning($"{largeChunks.Count} chunks son grandes (>400 chars). Máximo: {largeChunks.Max(c => c.Length)} chars");
        }

        return validatedChunks;
    }

    // 🆕 MÉTODO NUEVO: Dividir por oraciones
    private List<string> SplitIntoSentences(string text)
    {
        // Dividir por puntos, signos de exclamación, interrogación
        var sentences = Regex.Split(text, @"(?<=[.!?])\s+")
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Where(s => s.Length > 10) // Filtrar fragmentos muy cortos
            .ToList();

        return sentences.Any() ? sentences : new List<string> { text };
    }

    // 🆕 MÉTODO NUEVO: Forzar división de chunks muy grandes
    private List<string> ForceChunkSplit(string text, int maxSize, int overlap)
    {
        var chunks = new List<string>();
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var currentChunk = new StringBuilder();

        foreach (var word in words)
        {
            if (currentChunk.Length + word.Length + 1 > maxSize && currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString().Trim());

                // Crear overlap con las últimas palabras
                var chunkText = currentChunk.ToString();
                var overlapText = GetLastWords(chunkText, overlap);
                currentChunk.Clear();
                if (!string.IsNullOrWhiteSpace(overlapText))
                {
                    currentChunk.Append(overlapText + " ");
                }
            }

            currentChunk.Append(word + " ");
        }

        if (currentChunk.Length > 0)
        {
            chunks.Add(currentChunk.ToString().Trim());
        }

        return chunks;
    }

    private string GetLastWords(string text, int maxLength)
    {
        if (text.Length <= maxLength) return text;

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var lastWords = words.TakeLast(Math.Min(20, words.Length / 4));
        return string.Join(" ", lastWords);
    }

    private Dictionary<string, object> CreateBaseMetadata(LegalDocument document, IFormFile file)
    {
        return new Dictionary<string, object>
        {
            ["title"] = document.Title,
            ["document_type"] = document.DocumentType.ToString(),
            ["legal_areas"] = string.Join(",", document.LegalAreas),
            ["key_concepts"] = string.Join(",", document.KeyConcepts),
            ["difficulty"] = document.Difficulty.ToString(),
            ["articles"] = string.Join(",", document.Articles),
            ["cases"] = string.Join(",", document.Cases),
            ["source"] = document.Source,
            ["created_at"] = document.CreatedAt.ToString("O"),
            ["document_id"] = document.Id.ToString(),
            ["file_name"] = file.FileName,
            ["file_size"] = file.Length,
            ["mime_type"] = file.ContentType
        };
    }

    // COMPLETAR el método CreateDocumentSummary (reemplazar desde la línea 774 hasta el final)
    private DocumentSummary? CreateDocumentSummary(IGrouping<string?, SearchResult> group)
    {
        try
        {
            var firstDoc = group.First();
            var metadata = firstDoc.Metadata;

            return new DocumentSummary
            {
                DocumentId = Guid.Parse(metadata["document_id"].ToString()!),
                Title = metadata.GetValueOrDefault("title", "").ToString()!,
                DocumentType = Enum.Parse<LegalDocumentType>(metadata.GetValueOrDefault("document_type", "StudyMaterial").ToString()!),
                LegalAreas = metadata.GetValueOrDefault("legal_areas", "").ToString()!.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                Difficulty = Enum.Parse<DifficultyLevel>(metadata.GetValueOrDefault("difficulty", "Intermediate").ToString()!),
                CreatedAt = DateTime.Parse(metadata.GetValueOrDefault("created_at", DateTime.MinValue.ToString("O")).ToString()!),
                FileName = metadata.GetValueOrDefault("file_name", "").ToString()!,
                ChunkCount = group.Count(),
                FileSize = long.Parse(metadata.GetValueOrDefault("file_size", "0").ToString()!)
            };
        }
        catch
        {
            return null;
        }
    }

    private async Task<List<StudyQuestion>> GenerateQuestionsFromSearchResults(
        List<SearchResult> searchResults,
        GenerateQuestionsRequest request)
    {
        if (!searchResults.Any())
        {
            return new List<StudyQuestion>();
        }

        var combinedContent = string.Join("\n\n", searchResults.Select(r => r.Content));
        var tempDocument = new LegalDocument
        {
            Id = Guid.NewGuid(),
            Title = $"Resultados para: {request.SearchQuery}",
            Content = combinedContent,
            LegalAreas = request.LegalAreas,
            Difficulty = request.Difficulty,
            DocumentType = LegalDocumentType.StudyMaterial,
            CreatedAt = DateTime.UtcNow
        };

        return await _questionGeneration.GenerateQuestionsFromDocument(tempDocument, request.Count);
    }


// AGREGAR este endpoint al DocumentController existente

[HttpPost("generate-questions-for-student/{documentId}")]
    public async Task<ActionResult<StudyQuestionsResponse>> GenerateQuestionsForStudent(
    Guid documentId,
    [FromBody] GenerateQuestionsForStudentRequest request)
    {
        try
        {
            // Obtener el documento
            var document = await GetDocumentByIdAsync(documentId);
            if (document == null)
            {
                return NotFound("Documento no encontrado");
            }

            // Generar preguntas usando tu servicio existente
            var questions = await _questionGeneration.GenerateQuestionsFromDocument(document, request.QuestionCount);

            // Preparar respuesta con formato para el frontend
            var response = new StudyQuestionsResponse
            {
                DocumentId = documentId,
                DocumentTitle = document.Title,
                StudentId = request.StudentId,
                SessionId = request.SessionId,
                Questions = questions.Select(q => new QuestionForStudent
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    Options = q.Options?.Select(o => new QuestionOptionDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        Text = o.Text,
                        IsCorrect = o.IsCorrect
                    }).ToList() ?? new List<QuestionOptionDto>(),
                    LegalArea = q.LegalArea,
                    Difficulty = q.Difficulty,
                    RelatedConcepts = q.RelatedConcepts
                }).ToList(),
                GeneratedAt = DateTime.UtcNow
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando preguntas para estudiante");
            return StatusCode(500, "Error interno del servidor");
        }
    }

 
}
// DTOs adicionales
public class GenerateQuestionsForStudentRequest
{
    public Guid StudentId { get; set; }
    public Guid? SessionId { get; set; }
    public int QuestionCount { get; set; } = 5;
}

public class StudyQuestionsResponse
{
    public Guid DocumentId { get; set; }
    public string DocumentTitle { get; set; } = string.Empty;
    public Guid StudentId { get; set; }
    public Guid? SessionId { get; set; }
    public List<QuestionForStudent> Questions { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

public class QuestionForStudent
{
    public Guid Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public List<QuestionOptionDto> Options { get; set; } = new();
    public string LegalArea { get; set; } = string.Empty;
    public DifficultyLevel Difficulty { get; set; }
    public List<string> RelatedConcepts { get; set; } = new();
}

public class QuestionOptionDto
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

public class EnhancedDocumentUploadResponse : DocumentUploadResponse
{
    public List<string> ChunkIds { get; set; } = new();
    public int ChunksCreated { get; set; }
    public int TotalCharacters { get; set; }
    public int ProcessingTimeMs { get; set; }
}

public class DocumentSummary
{
    public Guid DocumentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public LegalDocumentType DocumentType { get; set; }
    public List<string> LegalAreas { get; set; } = new();
    public DifficultyLevel Difficulty { get; set; }
    public DateTime CreatedAt { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int ChunkCount { get; set; }
    public long FileSize { get; set; }
}

public class DocumentSearchRequest
{
    public string Query { get; set; } = string.Empty;
    public int Limit { get; set; } = 10;
}

public class DocumentUploadResponse
{
    public bool Success { get; set; }
    public LegalDocument? Document { get; set; }
    public string VectorId { get; set; } = string.Empty;
    public List<StudyQuestion> SampleQuestions { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}

public class TextUploadRequest
{
    public string Content { get; set; } = string.Empty;
    public string? Title { get; set; }
    public LegalDocumentType? DocumentType { get; set; }
    public bool GenerateQuestions { get; set; } = true;
    public int? QuestionCount { get; set; } = 5;
}

public class DocumentSystemStatus
{
    public bool VectorDatabaseReady { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class GenerateQuestionsRequest
{
    public List<string> LegalAreas { get; set; } = new();
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Intermediate;
    public int Count { get; set; } = 5;
    public string? SearchQuery { get; set; }
}