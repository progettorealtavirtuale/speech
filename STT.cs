using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class SpeechRecognitionTest : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private LLM_Groq llmGroq;
    [SerializeField] private TTS_SF_Simba ttsSFSimba;

    private AudioClip clip;
    private byte[] bytes;
    private bool recording;

    private float silenceTimer = 0f;
    private float silenceThreshold = 0.01f;
    private float silenceDurationToStop = 2f;

    private int clickCount = 0;  // Contatore 

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Ogni click del tasto sinistro
        {
            clickCount++; //variabile che tenga conto del numero di click

            if (clickCount % 2 != 0) // Se il numero di clic è dispari, per far si che si stoppi/parte l'animazione
            {
                if (!recording)
                {
                    // Avvia la registrazione se non è già attiva
                    StartRecording();
                }
            }
        }

        // Se è in corso la registrazione, gestisci la fine quando c'è silenzio
        if (recording)
        {
            if (Microphone.GetPosition(null) >= clip.samples)
            {
                StopRecording();
                return;
            }

            float volume = GetMicVolume();
            if (volume < silenceThreshold)
            {
                silenceTimer += Time.deltaTime;
                if (silenceTimer >= silenceDurationToStop)
                {
                    StopRecording();
                }
            }
            else
            {
                silenceTimer = 0f;
            }
        }
    }

    private void StartRecording()
    {
        text.color = Color.white;
        text.text = "Talk to me...";
        clip = Microphone.Start(null, false, 10, 44100);
        recording = true;
        silenceTimer = 0f;
    }

    private void StopRecording()
{
    int position = Microphone.GetPosition(null);

    // Controllo di sicurezza
    if (position <= 0 || clip == null)
    {
        Debug.LogWarning("Registrazione non valida o troppo breve.");
        Microphone.End(null);
        recording = false;
        return;
    }

    Microphone.End(null);

    try
    {
        float[] samples = new float[position * clip.channels];
        bool success = clip.GetData(samples, 0);

        if (!success)
        {
            Debug.LogError("GetData fallito.");
            return;
        }

        bytes = EncodeAsWAV(samples, clip.frequency, clip.channels);
        recording = false;
        SendRecording();
    }
    catch (System.Exception e)
    {
        Debug.LogError("Errore durante GetData: " + e.Message);
    }
}


    private float GetMicVolume()
    {
        int micPosition = Microphone.GetPosition(null);
        int start = Mathf.Max(0, micPosition - 128);
        float[] samples = new float[128];
        clip.GetData(samples, start);

        float sum = 0f;
        foreach (var sample in samples)
        {
            sum += Mathf.Abs(sample);
        }
        return sum / samples.Length;
    }

    private void SendRecording()
    {
        text.color = Color.yellow;
        text.text = "Analizzo...";
        StartCoroutine(SendToAPI(bytes));
    }

    private IEnumerator SendToAPI(byte[] audioBytes)
    {
        string url = "https://api-inference.huggingface.co/models/openai/whisper-large-v3-turbo";
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(audioBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", "Bearer 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");
        request.SetRequestHeader("Content-Type", "audio/wav");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            string transcribedText = ExtractTranscription(responseText);
            text.color = Color.white;
            text.text = "Hai detto: " + transcribedText;

            if (llmGroq != null)
                llmGroq.TextToLLM(transcribedText);
            else
                Debug.LogError("llmGroq non assegnato.");
        }
        else
        {
            text.color = Color.red;
            text.text = "Errore: " + request.error;
        }
    }

    private string ExtractTranscription(string response)
    {
        int startIndex = response.IndexOf("\"text\":\"") + 8;
        int endIndex = response.IndexOf("\"", startIndex);
        return response.Substring(startIndex, endIndex - startIndex);
    }

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
                writer.Write(16);
                writer.Write((ushort)1);
                writer.Write((ushort)channels);
                writer.Write(frequency);
                writer.Write(frequency * channels * 2);
                writer.Write((ushort)(channels * 2));
                writer.Write((ushort)16);
                writer.Write("data".ToCharArray());
                writer.Write(samples.Length * 2);

                foreach (var sample in samples)
                    writer.Write((short)(sample * short.MaxValue));
            }
            return memoryStream.ToArray();
        }
    }
}
