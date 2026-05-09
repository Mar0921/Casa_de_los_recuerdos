using UnityEngine;

public class ScreamController : MonoBehaviour
{
    Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        bool caminando = h != 0 || v != 0;

        // "isWalking" con i minuscula — igual que en el Animator
        anim.SetBool("isWalking", caminando);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            anim.SetTrigger("Scream");
        }
    }

    // Llamado por el monstruo al atrapar al jugador
    public void ForzarScream()
    {
        if (anim != null)
        {
            anim.SetBool("isWalking", false);
            anim.SetTrigger("Scream");
        }
    }
}