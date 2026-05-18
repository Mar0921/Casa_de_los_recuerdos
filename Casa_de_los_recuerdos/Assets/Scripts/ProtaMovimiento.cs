using UnityEngine;
using UnityEngine.InputSystem;

public class ProtaMovimiento : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadMovimiento = 5f;
    public float velocidadRotacion = 200f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip sonidoPaso;
    public float intervaloEntrepasos = 0.5f;
    private float tiempoUltimoPaso;

    private Animator anim;
    private float inputHorizontal;
    private float inputVertical;
    private bool puedeMover = true;

    void Start()
    {
        anim = GetComponent<Animator>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        // Obtener inputs
        inputHorizontal = 0f;
        inputVertical = 0f;

        // Movimiento horizontal (izquierda/derecha)
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            inputHorizontal = -1f;
        else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            inputHorizontal = 1f;

        // SOLO hacia adelante (sin retroceso)
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            inputVertical = 1f;

        // Animar
        if (anim != null)
        {
            anim.SetFloat("VelX", inputHorizontal);
            anim.SetFloat("VelY", inputVertical);
        }

        // Sonido de pasos
        bool seMueve = (inputHorizontal != 0f || inputVertical != 0f);
        if (seMueve)
        {
            if (Time.time - tiempoUltimoPaso >= intervaloEntrepasos)
            {
                if (sonidoPaso != null && audioSource != null)
                {
                    audioSource.PlayOneShot(sonidoPaso);
                    tiempoUltimoPaso = Time.time;
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (!puedeMover) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 forward = cam.transform.forward;
        Vector3 right = cam.transform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        // Dirección de movimiento
        Vector3 direccionMovimiento = (right * inputHorizontal + forward * inputVertical).normalized;

        if (direccionMovimiento != Vector3.zero)
        {
            // MOVER DIRECTAMENTE EL TRANSFORM (más simple y confiable)
            transform.Translate(direccionMovimiento * velocidadMovimiento * Time.deltaTime, Space.World);

            // Rotación suave
            Quaternion targetRotation = Quaternion.LookRotation(direccionMovimiento);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * velocidadRotacion);
        }
    }

    public void SetPuedeMover(bool estado)
    {
        puedeMover = estado;
    }
}