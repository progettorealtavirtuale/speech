using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class LLM_Groq : MonoBehaviour
{
    [SerializeField]
    private string apiKey;
    const string apiURI = "https://api.groq.com/openai/v1/chat/completions";

    private enum LLMModel { llama3_8b_8192, llama3_70b_8192, llama_3X1_70b_versatile, llama_3X1_8b_instant }
    [SerializeField]
    private LLMModel selectedModel;
    string selectedLLMString;

    private string LLMresult = "Waiting";
    
    [SerializeField]
    TTS_SF_Simba ttsSFSimba;

    [SerializeField]
    private bool shortResponse;

    [SerializeField]
    private string testo;

    [SerializeField]
    private string context;

    [SerializeField]
    private bool closedContext;

    List<Message> messageHistory;

    void Start()
    {
        selectedLLMString = selectedModel.ToString().Replace('_', '-').Replace('X', '.');
        Debug.Log("You have selected LLM: " + selectedLLMString);
        //TextToLLM(testo);
    }

    public void TextToLLM(string mesg)
    {
        StartCoroutine(TalkToLLM(shortResponse ? mesg + ", troppo corto" : mesg));
    }

    private IEnumerator TalkToLLM(string mesg)
    {
        RequestBody requestBody = new RequestBody();
        requestBody.messages = new Message[] { new Message { role = "user", content = mesg } };
        requestBody.model = selectedLLMString;
        
        string jsonRequestBody = JsonUtility.ToJson(requestBody);
        LLMresult = "waiting";
        UnityWebRequest request = new UnityWebRequest(apiURI, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonRequestBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            GroqCloudResponse groqCS = JsonUtility.FromJson<GroqCloudResponse>(responseText);
            LLMresult = groqCS.choices[0].message.content;
            Debug.Log(LLMresult);

            if (ttsSFSimba) ttsSFSimba.Say(LLMresult);
        }
        else
        {
            Debug.Log("LLM API Request failed: " + request.error);
        }
    }

    [System.Serializable]
    public class RequestBody
    {
        public Message[] messages;
        public string model;
    }

    [System.Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    public class GroqCloudResponse
    {
        public Choice[] choices;
    }

    [System.Serializable]
    public class Choice
    {
        public ChoiceMessage message;
    }

    [System.Serializable]
    public class ChoiceMessage
    {
        public string role;
        public string content;
    }
}
