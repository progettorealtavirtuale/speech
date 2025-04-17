
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class TTS_SF_Simba : MonoBehaviour
{
    [Header("ElevenLabs Settings")]
    [SerializeField] private string elevenLabsAPIKey = "sk_3499e022aa394f8a795fff98b2f9e1a71ec047acde1b6f31";
    [SerializeField] private string voiceID = "pwvkOXKI34DbjtR6yUk5"; // voce ID

    private const string ELEVEN_TTS_BASE_URI = "https://api.elevenlabs.io/v1/text-to-speech/";

    public void Say(string textInput)
    {
        StartCoroutine(PlayTTS(textInput));
    }

    IEnumerator PlayTTS(string message)
    {
        string url = ELEVEN_TTS_BASE_URI + voiceID + "/stream";

        // JSON payload
        ElevenLabsTTSRequest payload = new ElevenLabsTTSRequest
        {
            text = message,
            model_id = "eleven_multilingual_v1",
            voice_settings = new VoiceSettings
            {
                stability = 0.5f,
                similarity_boost = 0.5f
            }
        };

        string jsonData = JsonUtility.ToJson(payload);
        Debug.Log("Sending to ElevenLabs: " + jsonData);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));
        request.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);

        request.SetRequestHeader("xi-api-key", elevenLabsAPIKey);
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Accept", "audio/mpeg");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
            GetComponent<AudioSource>().PlayOneShot(clip);
        }
        else
        {
            Debug.Log("TTS API Request failed: " + request.error);
            Debug.Log("Response Code: " + request.responseCode);
        }
    }

    [Serializable]
    public class ElevenLabsTTSRequest
    {
        public string text;
        public string model_id;
        public VoiceSettings voice_settings;
    }

    [Serializable]
    public class VoiceSettings
    {
        public float stability;
        public float similarity_boost;
    }
} 
/*
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
public class TTS_SF_Simba : MonoBehaviour
{
    [Header("VoiceRSS Settings")]
    [SerializeField] private string voiceRSSApiKey = "42bf7fae2faa4a68a63b9d0690403de6"; 
    [SerializeField] private string language = "en-en"; 

    private const string VOICERSS_URL = "https://api.voicerss.org/";
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Say(string textInput)
    {
        StartCoroutine(PlayTTS(textInput));
    }

    IEnumerator PlayTTS(string message)
    {
        string url = $"{VOICERSS_URL}?key={voiceRSSApiKey}&hl={language}&src={UnityWebRequest.EscapeURL(message)}&c=MP3&f=44khz_16bit_stereo";

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
                audioSource.Play(); // <-- questo attiva il LipSync
            }
            else
            {
                Debug.LogError("TTS Request failed: " + www.error);
            }
        }
    }
}
*/