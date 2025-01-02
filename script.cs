using UnityEngine;

public class LipSync : MonoBehaviour
{
    public AudioSource audioSource;
    public SkinnedMeshRenderer skinnedMeshRenderer;
    public int[] phonemeBlendShapeIndices; // Indici delle blend shapes
    public float[] phonemeTimings; // Tempi per ciascun fonema (in secondi)
    
    private int currentPhoneme = 0;

    void Start()
    {
        audioSource.Play();
    }

    void Update()
    {
        if (currentPhoneme < phonemeTimings.Length && 
            audioSource.time >= phonemeTimings[currentPhoneme])
        {
            SetBlendShape(currentPhoneme);
            currentPhoneme++;
        }
    }

    void SetBlendShape(int phonemeIndex)
    {
        // Resetta tutte le blend shapes
        for (int i = 0; i < skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
        {
            skinnedMeshRenderer.SetBlendShapeWeight(i, 0);
        }

        // Attiva il fonema corrente
        if (phonemeIndex < phonemeBlendShapeIndices.Length)
        {
            int blendShapeIndex = phonemeBlendShapeIndices[phonemeIndex];
            skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, 100);
        }
    }
}
