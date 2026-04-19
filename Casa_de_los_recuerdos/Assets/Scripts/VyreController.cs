using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class VyreController : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadNormal = 5f;
    public float velocidadCorrer = 9f;
    public float velocidadRotacion = 200f;

    [Header("Salto")]
    public float fuerzaDeSalto = 8f;
    public int maxSaltos = 2;
    public bool puedoSaltar;

    [Header("Empujar")]
    public float fuerzaEmpuje = 5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip sonidoPaso;
    public AudioClip sonidoSalto;
    public float intervaloEntrepasos = 0.5f;
    private float tiempoUltimoPaso;

    [Header("Configuración")]
    public LayerMask capaSuelo;

    // Referencias
    private Rigidbody rb;
    private Animator anim;

    // Variables privadas
    private float x, y;
    private int saltosRestantes;
    private float velocidadMovimiento;
    private bool estaEmpujando = false;
    private bool estaRompiendo = false;
    private Rigidbody objetoEmpujado = null;

    private float umbralColisionVertical = 0.7f;
    private float offsetAlturaMinima = 0.3f;

    void Start()
    {
        puedoSaltar = false;
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        velocidadMovimiento = velocidadNormal;
        saltosRestantes = maxSaltos;

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.loop = false;
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        // INPUT
        x = 0f;
        y = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x = -1f;
        else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x = 1f;

        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) y = -1f;
        else if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) y = 1f;

        // ANIMACIONES
        anim.SetFloat("VelX", x);
        anim.SetFloat("VelY", y);
        anim.SetBool("estaEmpujando", estaEmpujando);
        anim.SetBool("romper", estaRompiendo);

        // 🏃 CORRER
        if (Keyboard.current.leftShiftKey.isPressed)
            velocidadMovimiento = velocidadCorrer;
        else
            velocidadMovimiento = velocidadNormal;

        // 👊 EMPUJAR con V
        if (Keyboard.current.vKey.wasPressedThisFrame && objetoEmpujado != null)
        {
            estaEmpujando = true;
        }
        else if (Keyboard.current.vKey.wasReleasedThisFrame)
        {
            estaEmpujando = false;
        }

        // 💥 PUÑO/ROMPER con B
        if (Keyboard.current.bKey.wasPressedThisFrame && !estaRompiendo)
        {
            estaRompiendo = true;
            StartCoroutine(ResetRomper());
        }

        // 🦘 SALTO
        if (puedoSaltar && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Saltar();
        }
        // 🦘 DOBLE SALTO
        else if (!puedoSaltar && Keyboard.current.spaceKey.wasPressedThisFrame && saltosRestantes > 0)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * fuerzaDeSalto, ForceMode.Impulse);
            saltosRestantes--;

            if (sonidoSalto != null && audioSource != null)
                audioSource.PlayOneShot(sonidoSalto);
        }

        // SONIDO DE PASOS
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
            Vector3 nuevaPosicion = rb.position + movimiento;
            rb.MovePosition(nuevaPosicion);

            Quaternion targetRotation = Quaternion.LookRotation(movimiento);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // EMPUJAR OBJETOS
        if (estaEmpujando && objetoEmpujado != null && movimiento != Vector3.zero)
        {
            Vector3 direccionEmpuje = movimiento.normalized;
            objetoEmpujado.AddForce(direccionEmpuje * fuerzaEmpuje, ForceMode.Force);
        }
    }

    void Saltar()
    {
        puedoSaltar = false;
        saltosRestantes = maxSaltos - 1;
        anim.SetBool("salte", true);
        anim.SetBool("tocoSuelo", false);
        rb.AddForce(Vector3.up * fuerzaDeSalto, ForceMode.Impulse);

        if (sonidoSalto != null && audioSource != null)
            audioSource.PlayOneShot(sonidoSalto);
    }

    // ← Fuera del OnCollisionEnter, al mismo nivel que los demás métodos
    IEnumerator ResetRomper()
    {
        yield return new WaitForSeconds(0.5f); // ← Ajusta según dure tu animación
        estaRompiendo = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        // DETECCIÓN DE SUELO
        if (((1 << collision.gameObject.layer) & capaSuelo) != 0)
        {
            puedoSaltar = true;
            saltosRestantes = maxSaltos;
            anim.SetBool("salte", false);
            anim.SetBool("tocoSuelo", true);
        }

        // 💥 ROMPER OBJETOS (solo si está presionando B)
        if (collision.gameObject.CompareTag("Rompible") && estaRompiendo)
        {
            Destroy(collision.gameObject, 0.5f);
        }

        // 📦 REGISTRAR OBJETO EMPUJABLE
        if (collision.gameObject.CompareTag("Empujable"))
        {
            if (!EstaEncimaDelObjeto(collision))
            {
                objetoEmpujado = collision.gameObject.GetComponent<Rigidbody>();
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & capaSuelo) != 0)
        {
            puedoSaltar = false;
        }

        if (collision.gameObject.CompareTag("Empujable"))
        {
            estaEmpujando = false;
            objetoEmpujado = null;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Empujable"))
        {
            bool estaEncima = EstaEncimaDelObjeto(collision);

            if (estaEncima && estaEmpujando)
            {
                estaEmpujando = false;
                objetoEmpujado = null;
            }
            else if (!estaEncima && !estaEmpujando)
            {
                objetoEmpujado = collision.gameObject.GetComponent<Rigidbody>();
            }
        }
    }

    bool EstaEncimaDelObjeto(Collision collision)
    {
        Bounds cajaBounds = collision.collider.bounds;
        float topeDeCaja = cajaBounds.max.y;
        float basePJ = transform.position.y - GetComponent<Collider>().bounds.extents.y;

        if (basePJ >= topeDeCaja - offsetAlturaMinima)
            return true;

        foreach (ContactPoint contacto in collision.contacts)
        {
            if (contacto.normal.y > umbralColisionVertical)
                return true;
        }

        return false;
    }
}