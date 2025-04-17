using UnityEngine;

public class ClickToAnimate : MonoBehaviour
{
    private Animator animator;
    private bool isTalking = true;  // Variabile per verificare se l'animazione è in corso

    void Start()
    {
        animator = GetComponent<Animator>();  // Trova l'Animator nell'oggetto
    }

    void Update()
    {
        // Se viene cliccato il tasto sinistro del mouse
        if (Input.GetMouseButtonDown(0))
        {
            // Alterna lo stato dell'animazione
            isTalking = !isTalking;

            if (isTalking)
            {
                animator.SetBool("isTalking", true);  // Avvia l'animazione
            }
            else
            {
                animator.SetBool("isTalking", false);  // Ferma l'animazione
            }
        }
    }
}
