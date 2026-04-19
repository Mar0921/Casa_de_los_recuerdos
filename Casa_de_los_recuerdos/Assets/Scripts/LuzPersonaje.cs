using UnityEngine;
using UnityEngine.InputSystem;

public class LuzPersonaje : MonoBehaviour
{
    public Light luz;
    public Animator animator;
    public AudioSource audioSource;
    public AudioClip sonidoLuz;
    public float intensidadMax = 3f;
    public float duracionLuz = 6f;
    public float cooldown = 8f;
    public float velocidadTransicion = 2f;
    public float tiempoAnimacion = 1.5f;

    private float tiempoRestante = 0f;
    private float tiempoCooldown = 0f;
    private float tiempoAnim = 0f;
    private bool luzActiva = false;
    private bool animActiva = false;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        luz.intensity = 0f;
        luz.enabled = false;

        if (animator != null)
            animator.SetBool("ilumine", false);
    }

    void Update()
    {
        // Activar
        if (Keyboard.current.lKey.wasPressedThisFrame && tiempoCooldown <= 0f && !luzActiva)
        {
            luzActiva = true;
            animActiva = true;
            tiempoRestante = duracionLuz;
            tiempoAnim = tiempoAnimacion;
            luz.enabled = true;

            if (audioSource != null && sonidoLuz != null)
                audioSource.PlayOneShot(sonidoLuz);

            if (animator != null)
                animator.SetBool("ilumine", true);
        }

        // Control animación
        if (animActiva)
        {
            tiempoAnim -= Time.deltaTime;
            if (tiempoAnim <= 0f)
            {
                animActiva = false;
                if (animator != null)
                    animator.SetBool("ilumine", false);
            }
        }

        // Control luz
        if (luzActiva)
        {
            luz.intensity = Mathf.MoveTowards(
                luz.intensity,
                intensidadMax,
                velocidadTransicion * Time.deltaTime
            );

            tiempoRestante -= Time.deltaTime;
            if (tiempoRestante <= 0f)
            {
                luzActiva = false;
                tiempoCooldown = cooldown;
            }
        }
        else
        {
            luz.intensity = Mathf.MoveTowards(
                luz.intensity,
                0f,
                velocidadTransicion * Time.deltaTime
            );

            if (luz.intensity <= 0f)
                luz.enabled = false;

            if (tiempoCooldown > 0f)
                tiempoCooldown -= Time.deltaTime;
        }
    }
}