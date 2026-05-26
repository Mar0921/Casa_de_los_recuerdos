using UnityEngine;
using UnityEngine.InputSystem;

public class MovimientoPJ : MonoBehaviour
{
    public float velocidadMovimiento = 5f;
    public float velocidadRotacion = 200f;
    public float fuerzaDeSalto = 8f;

    public float velocidadNormal = 5f;
    public float velocidadEnSombra = 1f;
    public float velocidadAgachado;
    public bool puedoSaltar;

    public AudioSource audioSource;
    public AudioClip sonidoPaso;
    public AudioClip sonidoSalto;
    public float intervaloEntrepasos = 0.5f;
    private float tiempoUltimoPaso;

    private float velocidadInicial;
    private bool estaAgachado = false;
    private bool enSombra = false;

    private Rigidbody rb;
    private Animator anim;
    private float x, y;
    public LayerMask capaSuelo;

    // ---- Variables para el collider ----
    private CapsuleCollider capsuleCollider;
    private Vector3 centroOriginal;
    private float alturaOriginal;
    private Vector3 centroAgachado;
    private float alturaAgachado;

    private float umbralColisionVertical = 0.7f;
    private float offsetAlturaMinima = 0.3f;

    void Start()
    {
        puedoSaltar = false;
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // Obtener o añadir el CapsuleCollider
        capsuleCollider = GetComponent<CapsuleCollider>();
        if (capsuleCollider == null)
        {
            capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
            Debug.Log("Se añadió un CapsuleCollider automáticamente.");
        }

        // Guardar valores originales (de pie)
        centroOriginal = capsuleCollider.center;
        alturaOriginal = capsuleCollider.height;

        // Valores agachado (según lo que pide el usuario)
        centroAgachado = new Vector3(centroOriginal.x, 13.84571f, centroOriginal.z);
        alturaAgachado = 28.25344f;

        // Ajustar valores originales si no coinciden con los que tiene el usuario
        // (por si ya los había cambiado manualmente)
        // Pero asumimos que los originales son los que están al inicio.

        velocidadInicial = velocidadMovimiento;

        if (velocidadNormal <= 0f)
            velocidadNormal = velocidadInicial;

        velocidadAgachado = velocidadNormal * 0.5f;
        ActualizarVelocidad();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.loop = false;
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        x = 0f;
        y = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x = -1f;
        else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x = 1f;

        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) y = -1f;
        else if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) y = 1f;

        anim.SetFloat("VelX", x);
        anim.SetFloat("VelY", y);

        // --- Detectar caída por gravedad (no solo por salto) ---
        if (!puedoSaltar)
        {
            // Si está bajando (velocidad Y negativa) activar animación de caída
            if (rb.linearVelocity.y < -0.5f)
            {
                anim.SetBool("salte", true);
                anim.SetBool("tocoSuelo", false);
            }
        }

        // --- Lógica de agachado con R ---
        bool quiereAgachado = false;
        if (puedoSaltar)
            quiereAgachado = Keyboard.current.rKey.isPressed;
        else
            quiereAgachado = false;

        if (quiereAgachado != estaAgachado)
        {
            estaAgachado = quiereAgachado;
            if (estaAgachado)
            {
                capsuleCollider.center = centroAgachado;
                capsuleCollider.height = alturaAgachado;
            }
            else
            {
                capsuleCollider.center = centroOriginal;
                capsuleCollider.height = alturaOriginal;
            }
            anim.SetBool("agachado", estaAgachado);
            ActualizarVelocidad();
        }

        // Salto
        if (puedoSaltar && Keyboard.current.spaceKey.wasPressedThisFrame)
            Saltar();

        // Sonido de pasos
        if (puedoSaltar && (x != 0f || y != 0f))
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
        Debug.Log($"velocidadMovimiento JUSTO ANTES de mover: {velocidadMovimiento}");
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 forward = cam.transform.forward;
        Vector3 right = cam.transform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 movimiento = (right * x + forward * y) * velocidadMovimiento * Time.deltaTime;

        if (movimiento != Vector3.zero)
        {
            rb.MovePosition(rb.position + movimiento);
            Quaternion targetRotation = Quaternion.LookRotation(movimiento);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    void Saltar()
    {
        puedoSaltar = false;
        anim.SetBool("salte", true);
        anim.SetBool("tocoSuelo", false);
        rb.AddForce(Vector3.up * fuerzaDeSalto, ForceMode.Impulse);

        if (sonidoSalto != null && audioSource != null)
            audioSource.PlayOneShot(sonidoSalto);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & capaSuelo) != 0)
        {
            puedoSaltar = true;
            anim.SetBool("salte", false);
            anim.SetBool("tocoSuelo", true);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        Rigidbody rbObjeto = collision.gameObject.GetComponent<Rigidbody>();
        if (rbObjeto != null)
        {
            rbObjeto.linearVelocity = Vector3.zero;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & capaSuelo) != 0)
            puedoSaltar = false;
    }

    void ActualizarVelocidad()
    {
        Debug.Log($"ActualizarVelocidad llamado - enSombra: {enSombra} | estaAgachado: {estaAgachado}");
        float velocidadBase = enSombra ? velocidadEnSombra : velocidadNormal;
        velocidadMovimiento = estaAgachado ? velocidadBase * 0.5f : velocidadBase;
        Debug.Log($"velocidadMovimiento resultado: {velocidadMovimiento}");
    }

    public void EnSombra(bool activo)
    {
        enSombra = activo;
        ActualizarVelocidad();
        Debug.Log("EnSombra: " + activo + " | Velocidad: " + velocidadMovimiento);
    }
}