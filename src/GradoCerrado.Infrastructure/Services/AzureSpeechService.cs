using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using GradoCerrado.Application.Interfaces;
using GradoCerrado.Infrastructure.Configuration;
using NAudio.Wave;


namespace GradoCerrado.Infrastructure.Services;

public class AzureSpeechService : ISpeechService
{
    private readonly SpeechConfig _speechConfig;
    private readonly AzureSpeechSettings _settings;
    private readonly ILogger<AzureSpeechService> _logger;

    public AzureSpeechService(
        IOptions<AzureSpeechSettings> settings,
        ILogger<AzureSpeechService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        // 🔧 Configurar Azure Speech
        _speechConfig = SpeechConfig.FromSubscription(_settings.ApiKey, _settings.Region);
        _speechConfig.SpeechSynthesisVoiceName = _settings.DefaultVoice;

        // 🎯 Configurar formato de audio
        _speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio16Khz32KBitRateMonoMp3);
    }

    // TEXT-TO-SPEECH
    public async Task<byte[]> TextToSpeechAsync(string text, string? voice = null)
    {
        try
        {
            _logger.LogInformation("Generando audio para texto: {Text}", text[..Math.Min(50, text.Length)]);

            using var synthesizer = new SpeechSynthesizer(_speechConfig);

            //Usar voz específica si se proporciona
            if (!string.IsNullOrEmpty(voice))
            {
                _speechConfig.SpeechSynthesisVoiceName = voice;
            }

            //Sintetizar audio
            using var result = await synthesizer.SpeakTextAsync(text);

            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                _logger.LogInformation("Audio generado exitosamente. Tamaño: {Size} bytes", result.AudioData.Length);
                return result.AudioData;
            }
            else
            {
                var errorMessage = $"Error en síntesis: {result.Reason}";
                _logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando audio para texto");
            throw;
        }
    }

    //SSML para pronunciación avanzada
    public async Task<byte[]> SsmlToSpeechAsync(string ssml)
    {
        try
        {
            _logger.LogInformation("Generando audio desde SSML");

            using var synthesizer = new SpeechSynthesizer(_speechConfig);
            using var result = await synthesizer.SpeakSsmlAsync(ssml);

            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                _logger.LogInformation("Audio SSML generado exitosamente");
                return result.AudioData;
            }
            else
            {
                throw new Exception($"Error en síntesis SSML: {result.Reason}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando audio desde SSML");
            throw;
        }
    }
    public async Task<string> SpeechToTextAsync(byte[] audioData)
    {
        try
        {
            _logger.LogInformation("Transcribiendo audio. Tamaño: {Size} bytes", audioData.Length);

            // Configurar idioma
            _speechConfig.SpeechRecognitionLanguage = "es-ES";

            // Validar que parece ser un WAV
            if (audioData.Length < 44 ||
                System.Text.Encoding.ASCII.GetString(audioData, 0, 4) != "RIFF")
            {
                throw new Exception("El archivo no parece ser un WAV válido");
            }

            // Método alternativo: usar archivo temporal
            var tempFile = Path.GetTempFileName() + ".wav";

            try
            {
                await File.WriteAllBytesAsync(tempFile, audioData);

                using var audioConfig = AudioConfig.FromWavFileInput(tempFile);
                using var recognizer = new SpeechRecognizer(_speechConfig, audioConfig);

                // Configurar propiedades adicionales
                recognizer.Properties.SetProperty(PropertyId.SpeechServiceConnection_InitialSilenceTimeoutMs, "10000");
                recognizer.Properties.SetProperty(PropertyId.SpeechServiceConnection_EndSilenceTimeoutMs, "10000");

                var result = await recognizer.RecognizeOnceAsync();

                _logger.LogInformation("Resultado del reconocimiento: {Reason}", result.Reason);

                switch (result.Reason)
                {
                    case ResultReason.RecognizedSpeech:
                        _logger.LogInformation("Transcripción exitosa: {Text}", result.Text);
                        return result.Text;

                    case ResultReason.NoMatch:
                        _logger.LogWarning("No se detectó habla. Detalles: {NoMatchDetails}", result.Properties.GetProperty(PropertyId.SpeechServiceResponse_JsonResult));
                        return "";

                    case ResultReason.Canceled:
                        var cancellation = CancellationDetails.FromResult(result);
                        _logger.LogError("Cancelado: {Reason} - {ErrorDetails} - {ErrorCode}",
                            cancellation.Reason, cancellation.ErrorDetails, cancellation.ErrorCode);
                        return "";

                    default:
                        _logger.LogWarning("Resultado inesperado: {Reason}", result.Reason);
                        return "";
                }
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transcribiendo audio");
            throw;
        }
    }

    //TEST DE CONEXIÓN
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var testAudio = await TextToSpeechAsync("Test de conexión exitoso");
            return testAudio.Length > 0;
        }
        catch
        {
            return false;
        }
    }
}