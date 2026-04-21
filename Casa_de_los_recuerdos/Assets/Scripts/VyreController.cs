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
    public bool puedoSaltar;
    private int saltosRealizados = 0;
    public int maxSaltos = 2; 

    [Header("Empujar")]
    public float fuerzaEmpuje = 30f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip sonidoPaso;
    public AudioClip sonidoSalto;
    public float intervaloEntrepasos = 0.5f;
    private float tiempoUltimoPaso;

    [Header("Animación")]
    public float duracionAnimacionPuno = 0.8f;

    public LayerMask capaSuelo;
    public ColisionPuno colisionPuno;

    private Rigidbody rb;
    private Animator anim;

    private float x, y;
    private float velocidadMovimiento;
    private bool estaEmpujando = false;
    private bool estaRompiendo = false;
    private Rigidbody objetoEmpujado = null;
    private GameObject objetoRompibleCercano = null;

    private float umbralColisionVertical = 0.7f;
    private float offsetAlturaMinima = 0.3f;

    void Start()
    {
        puedoSaltar = false;
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        velocidadMovimiento = velocidadNormal;

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.loop = true;
        audioSource.playOnAwake = true;
    }

    void Update()
    {
        x = 0f;
        y = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x = -1f;
        else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x = 1f;

        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) y = -1f;
        else if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) y = 1f;

        if (anim != null)
        {
            anim.SetFloat("VelX", x);
            anim.SetFloat("VelY", y);
            anim.SetBool("estaEmpujando", estaEmpujando);
            anim.SetBool("romper", estaRompiendo);
        }

        if (Keyboard.current.leftShiftKey.isPressed)
            velocidadMovimiento = velocidadCorrer;
        else
            velocidadMovimiento = velocidadNormal;

        // romper
        if (Keyboard.current.bKey.wasPressedThisFrame && !estaRompiendo)
        {
            estaRompiendo = true;
            if (colisionPuno != null) colisionPuno.ActivarPuno(); 
            StartCoroutine(ResetRomper());
        }

        // dobleSalto
        if (Keyboard.current.spaceKey.wasPressedThisFrame && saltosRealizados < maxSaltos)
            Saltar();

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
            rb.MovePosition(rb.position + movimiento);
            Quaternion targetRotation = Quaternion.LookRotation(movimiento);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        if (estaEmpujando && objetoEmpujado != null && movimiento != Vector3.zero)
            objetoEmpujado.AddForce(movimiento.normalized * fuerzaEmpuje, ForceMode.Force);
    }

    void Saltar()
    {
        saltosRealizados++;

        if (anim != null)
        {
            anim.SetBool("salte", true);
            anim.SetBool("tocoSuelo", false);
        }

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * fuerzaDeSalto, ForceMode.Impulse);

        if (sonidoSalto != null && audioSource != null)
            audioSource.PlayOneShot(sonidoSalto);
    }

    IEnumerator ResetRomper()
    {
        yield return new WaitForSeconds(duracionAnimacionPuno);
        estaRompiendo = false;
        if (colisionPuno != null) colisionPuno.DesactivarPuno(); 
    }

    void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & capaSuelo) != 0)
        {
            puedoSaltar = true;
            saltosRealizados = 0; 

            if (anim != null)
            {
                anim.SetBool("salte", false);
                anim.SetBool("tocoSuelo", true);
            }
        }

        if (collision.gameObject.CompareTag("Rompible") && estaRompiendo)
            objetoRompibleCercano = collision.gameObject;

        if (collision.gameObject.CompareTag("Empujable"))
        {
            if (!EstaEncimaDelObjeto(collision))
            {
                estaEmpujando = true;
                objetoEmpujado = collision.gameObject.GetComponent<Rigidbody>();
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & capaSuelo) != 0)
            puedoSaltar = false;

        if (collision.gameObject.CompareTag("Empujable"))
        {
            estaEmpujando = false;
            objetoEmpujado = null;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Rompible") && estaRompiendo)
            objetoRompibleCercano = collision.gameObject;

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
                estaEmpujando = true;
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = puedoSaltar ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}