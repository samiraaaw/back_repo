using Microsoft.AspNetCore.Mvc;
using GradoCerrado.Application.Interfaces;
using GradoCerrado.Application.DTOs;

namespace GradoCerrado.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpeechController : ControllerBase
{
    private readonly ISpeechService _speechService;
    private readonly ILogger<SpeechController> _logger;

    public SpeechController(ISpeechService speechService, ILogger<SpeechController> logger)
    {
        _speechService = speechService;
        _logger = logger;
    }


    [HttpPost("analyze-audio")]
    public async Task<ActionResult> AnalyzeAudio(IFormFile audioFile)
    {
        try
        {
            using var stream = audioFile.OpenReadStream();
            var audioData = new byte[audioFile.Length];
            await stream.ReadAsync(audioData, 0, audioData.Length);

            // Analizar header WAV
            if (audioData.Length < 44)
            {
                return BadRequest("Archivo muy pequeño para ser WAV válido");
            }

            var riff = System.Text.Encoding.ASCII.GetString(audioData, 0, 4);
            var wave = System.Text.Encoding.ASCII.GetString(audioData, 8, 4);
            var fmt = System.Text.Encoding.ASCII.GetString(audioData, 12, 4);

            var audioFormat = BitConverter.ToUInt16(audioData, 20);
            var channels = BitConverter.ToUInt16(audioData, 22);
            var sampleRate = BitConverter.ToUInt32(audioData, 24);
            var bitsPerSample = BitConverter.ToUInt16(audioData, 34);

            return Ok(new
            {
                fileName = audioFile.FileName,
                fileSize = audioFile.Length,
                header = new
                {
                    riff,
                    wave,
                    fmt,
                    audioFormat,
                    channels,
                    sampleRate,
                    bitsPerSample
                },
                isValidWav = riff == "RIFF" && wave == "WAVE",
                azureCompatible = audioFormat == 1 && channels == 1 && sampleRate == 16000 && bitsPerSample == 16
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    //ENDPOINT: Test de conexión
    [HttpGet("test")]
    public async Task<ActionResult> TestConnection()
    {
        try
        {
            var isWorking = await _speechService.TestConnectionAsync();

            return Ok(new
            {
                success = isWorking,
                message = isWorking ? "Conexión exitosa con Azure Speech" : "Error de conexión",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en test de conexión");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    //ENDPOINT: Texto a voz
    [HttpPost("text-to-speech")]
    public async Task<ActionResult> TextToSpeech([FromBody] TtsRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return BadRequest("El texto no puede estar vacío");
            }

            var audioData = await _speechService.TextToSpeechAsync(request.Text, request.Voice);
            return File(audioData, "audio/mpeg", "speech.mp3");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en text-to-speech");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // 🎤 ENDPOINT: Voz a texto
[HttpPost("speech-to-text")]
public async Task<ActionResult> SpeechToText(IFormFile audioFile)
{
    try
    {
        if (audioFile == null || audioFile.Length == 0)
        {
            return BadRequest("Debe proporcionar un archivo de audio");
        }

        // Validar tipo de archivo
        var allowedTypes = new[] { ".wav", ".mp3", ".m4a", ".ogg" };
        var fileExtension = Path.GetExtension(audioFile.FileName).ToLower();
        
        if (!allowedTypes.Contains(fileExtension))
        {
            return BadRequest($"Tipo de archivo no soportado. Use: {string.Join(", ", allowedTypes)}");
        }

        // Leer bytes del archivo
        using var stream = audioFile.OpenReadStream();
        var audioData = new byte[audioFile.Length];
        await stream.ReadAsync(audioData, 0, audioData.Length);

        // Transcribir con Azure Speech
        var transcription = await _speechService.SpeechToTextAsync(audioData);

        _logger.LogInformation("Audio transcrito exitosamente: {Transcription}", transcription);

        return Ok(new 
        { 
            transcription,
            success = !string.IsNullOrEmpty(transcription),
            confidence = transcription.Length > 0 ? 0.95 : 0.0, // Estimado
            audioSize = audioFile.Length,
            fileName = audioFile.FileName
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error en speech-to-text");
        return StatusCode(500, new { error = "Error interno del servidor", details = ex.Message });
    }
}
}