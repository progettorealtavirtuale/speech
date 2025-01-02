using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class SpeechRecognitionTest : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private LLM_Groq llmGroq;  // Riferimento allo script LLM_Groq
    [SerializeField] private TTS_SF_Simba ttsSFSimba;  // Riferimento allo script TTS_SF_Simba

    private AudioClip clip;
    private byte[] bytes;
    private bool recording;

    // Questo è il metodo che viene chiamato quando il gioco inizia
    private void Start()
    {
        startButton.onClick.AddListener(StartRecording);
        stopButton.onClick.AddListener(StopRecording);
        stopButton.interactable = false;
    }

    // Questo metodo viene chiamato ogni frame, per fermare la registrazione automaticamente dopo 10 secondi
    private void Update()
    {
        if (recording && Microphone.GetPosition(null) >= clip.samples)
        {
            StopRecording();
        }
    }

    // Inizia la registrazione dal microfono
    private void StartRecording()
    {
        text.color = Color.white;
        text.text = "Recording...";
        startButton.interactable = false;
        stopButton.interactable = true;
        clip = Microphone.Start(null, false, 10, 44100);  // Registra per 10 secondi con frequenza di 44100 Hz
        recording = true;
    }

    // Ferma la registrazione e invia i dati
    private void StopRecording()
    {
        var position = Microphone.GetPosition(null);
        Microphone.End(null);  // Ferma la registrazione
        var samples = new float[position * clip.channels];
        clip.GetData(samples, 0);  // Estrai i dati registrati
        bytes = EncodeAsWAV(samples, clip.frequency, clip.channels);  // Codifica in formato WAV
        recording = false;
        SendRecording();  // Invia i dati
    }

    // Metodo per inviare i dati audio all'API
    private void SendRecording()
    {
        text.color = Color.yellow;
        text.text = "Sending...";
        stopButton.interactable = false;

        // Avvia la coroutine per inviare i dati audio
        StartCoroutine(SendToAPI(bytes));
    }

    // Coroutine che invia i dati audio a Hugging Face (o altro servizio API)
    private IEnumerator SendToAPI(byte[] audioBytes)
    {
        string url = "https://api-inference.huggingface.co/models/openai/whisper-large-v3-turbo";  // Cambia con l'URL del tuo modello
        UnityWebRequest request = new UnityWebRequest(url, "POST");

        // Crea la richiesta con i dati audio
        request.uploadHandler = new UploadHandlerRaw(audioBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", "Bearer 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");  // Sostituisci con la tua chiave API
        request.SetRequestHeader("Content-Type", "audio/wav");

        // Invia la richiesta e attendi la risposta
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;

            // Estrai il testo dalla risposta (assumendo che la risposta sia un oggetto JSON con un campo "text")
            string transcribedText = ExtractTranscription(responseText);
            text.color = Color.white;
            text.text = "Transcription: " + transcribedText;  // Mostra la trascrizione

            // Invia il testo trascritto all'LLM per ricevere una risposta
            // Verifica che llmGroq non sia nullo
            if (llmGroq != null)
            {
                // Invia il testo trascritto a LLM per la risposta
                llmGroq.TextToLLM(transcribedText);  // Invia il testo trascritto a LLM per la risposta
            }
            else
            {
                Debug.LogError("llmGroq is not assigned in the inspector.");
            }
            startButton.interactable = true;
        }
        else
        {
            text.color = Color.red;
            text.text = "Error: " + request.error;  // Mostra l'errore in caso di fallimento
            startButton.interactable = true;
        }
    }

    // Estrai il testo dalla risposta (assumendo che la risposta contenga il campo "text")
    private string ExtractTranscription(string response)
    {
        // Assumendo che la risposta sia del tipo: {"text": "Stella"}
        int startIndex = response.IndexOf("\"text\":\"") + 8;
        int endIndex = response.IndexOf("\"", startIndex);
        return response.Substring(startIndex, endIndex - startIndex);
    }

    // Codifica i dati audio in formato WAV
    private byte[] EncodeAsWAV(float[] samples, int frequency, int channels)
    {
        using (var memoryStream = new System.IO.MemoryStream(44 + samples.Length * 2))
        {
            using (var writer = new System.IO.BinaryWriter(memoryStream))
            {
                writer.Write("RIFF".ToCharArray());
                writer.Write(36 + samples.Length * 2);
                writer.Write("WAVE".ToCharArray());
                writer.Write("fmt ".ToCharArray());
                writer.Write(16); // PCM format size
                writer.Write((ushort)1);  // PCM format
                writer.Write((ushort)channels);
                writer.Write(frequency);
                writer.Write(frequency * channels * 2);  // Byte rate
                writer.Write((ushort)(channels * 2));  // Block align
                writer.Write((ushort)16);  // Bits per sample (16-bit)
                writer.Write("data".ToCharArray());
                writer.Write(samples.Length * 2);  // Data chunk size

                // Scrive i dati audio come valori a 16 bit
                foreach (var sample in samples)
                {
                    writer.Write((short)(sample * short.MaxValue));  // Converte i dati audio a short
                }
            }
            return memoryStream.ToArray();  // Restituisce il byte array WAV
        }
    }
}
