using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class TTS_SF_Simba : MonoBehaviour
{
    //Variables
    [SerializeField]
    private string SPEECHIFY_API_KEY;

    private enum SelectVoice
    {
        US_Henry, US_Carly, US_Kyle, US_Kristy, US_Oliver, US_Tasha, US_Joe, US_Lisa,
        US_George, US_Emily, US_Rob, GB_Russell, GB_Benjamin, GB_Michael, AU_KIM, IN_Ankit, IN_Arun,
        GB_Carol, GB_Helen, US_Julie, AU_Linda, US_Mark, US_Nick, NG_Elijah, GB_Beverly, GB_Collin,
        US_Erin, US_Jack, US_Jesse, US_Keenan, US_Lindsey, US_Monica, GB_Phil, GB_Declan, US_Stacy,
        GB_Archie, US_Evelyn, GB_Freddy, GB_Harper, US_Jacob, US_James, US_Mason, US_Victoria
    }

    [SerializeField]
    private SelectVoice selectVoice;

    const string TTS_API_URI = "https://api.sws.speechify.com/v1/audio/stream";      //POST URI, streaming API
    private string sfVoice;

    // Start is called before the first frame update
    void Start()
    {
        sfVoice = selectVoice.ToString().Substring(3);
        Debug.Log("You have selected voice " + sfVoice);
    }

    public void Say(string textInput)
    {
        StartCoroutine(PlayTTS(textInput));
    }

    IEnumerator PlayTTS(string mesg)
    {
        //JSON
        TextToSpeechData ttsData = new TextToSpeechData();
        ttsData.input = SimpleCleanText(mesg);
        ttsData.voice_id = sfVoice;
        string jsonPrompt = JsonUtility.ToJson(ttsData);

        //WebRequest
        UnityWebRequest request = new UnityWebRequest(TTS_API_URI, "POST");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonPrompt));
        request.downloadHandler = new DownloadHandlerAudioClip(TTS_API_URI, AudioType.MPEG);

        //Headers
        request.SetRequestHeader("content-type", "application/json");
        request.SetRequestHeader("accept", "audio/mpeg");
        request.SetRequestHeader("Authorization", SPEECHIFY_API_KEY);

        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
            GetComponent<AudioSource>().PlayOneShot(clip);
            StartCoroutine(WaitForTalkingFinished());
        }
        else Debug.Log("TTS API Request failed: " + request.error);
    }

    IEnumerator WaitForTalkingFinished()
    {
        yield return new WaitForSeconds(1.5f); // Attendi prima di restituire
    }

    string SimpleCleanText(string inputText)
    {
        return inputText.Trim();
    }

    [Serializable]
    public class TextToSpeechData
    {
        public string input;
        public string voice_id;
    }
}
