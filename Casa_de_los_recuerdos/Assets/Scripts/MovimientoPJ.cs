using UnityEngine;
using UnityEngine.InputSystem;

public class MovimientoPJ : MonoBehaviour
{
    public float velocidadMovimiento = 5f;
    public float velocidadRotacion = 200f;
    public float fuerzaDeSalto = 8f;
    public float fuerzaEmpuje = 5f;

    public float velocidadNormal = 5f;
    public float velocidadEnSombra = 1f;
    public float velocidadAgachado;
    public bool puedoSaltar;

    private float velocidadInicial;
    private bool estaAgachado = false;
    private bool enSombra = false;

    private Rigidbody rb;
    private Animator anim;
    private float x, y;
    private bool estaEmpujando = false;
    private Rigidbody objetoEmpujado = null;
    public LayerMask capaSuelo;

    private float umbralColisionVertical = 0.7f;
    private float offsetAlturaMinima = 0.3f;

    void Start()
    {
        puedoSaltar = false;
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        velocidadInicial = velocidadMovimiento;

        if (velocidadNormal <= 0f)
            velocidadNormal = velocidadInicial;

        velocidadAgachado = velocidadNormal * 0.5f;

        ActualizarVelocidad();
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
        anim.SetBool("estaEmpujando", estaEmpujando);

        if (puedoSaltar)
        {
            estaAgachado = Keyboard.current.rKey.isPressed;
        }
        else
        {
            estaAgachado = false;
        }

        anim.SetBool("agachado", estaAgachado);
        ActualizarVelocidad();

        if (puedoSaltar && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Saltar();
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

        if (estaEmpujando && objetoEmpujado != null && movimiento != Vector3.zero)
        {
            Vector3 direccionEmpuje = movimiento.normalized;
            objetoEmpujado.AddForce(direccionEmpuje * fuerzaEmpuje, ForceMode.Force);
        }
    }

    void Saltar()
    {
        puedoSaltar = false;
        anim.SetBool("salte", true);
        anim.SetBool("tocoSuelo", false);
        rb.AddForce(Vector3.up * fuerzaDeSalto, ForceMode.Impulse);
    }

    bool EstaEncimaDelObjeto(Collision collision)
    {
        Bounds cajaBounds = collision.collider.bounds;
        float topeDeCaja = cajaBounds.max.y;
        float basePJ = transform.position.y - GetComponent<Collider>().bounds.extents.y;

        if (basePJ >= topeDeCaja - offsetAlturaMinima)
        {
            return true;
        }

        foreach (ContactPoint contacto in collision.contacts)
        {
            if (contacto.normal.y > umbralColisionVertical)
            {
                return true;
            }
        }

        return false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & capaSuelo) != 0)
        {
            puedoSaltar = true;
            anim.SetBool("salte", false);
            anim.SetBool("tocoSuelo", true);
        }

        if (collision.gameObject.CompareTag("Empujable"))
        {
            if (!EstaEncimaDelObjeto(collision))
            {
                estaEmpujando = true;
                objetoEmpujado = collision.gameObject.GetComponent<Rigidbody>();
            }
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
                estaEmpujando = true;
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

    void ActualizarVelocidad()
    {
        float velocidadBase = enSombra ? velocidadEnSombra : velocidadNormal;

        if (estaAgachado)
            velocidadMovimiento = velocidadBase * 0.5f;
        else
            velocidadMovimiento = velocidadBase;
    }

    public void EnSombra(bool activo)
    {
        enSombra = activo;
        ActualizarVelocidad();

        Debug.Log("EnSombra: " + activo + " | Velocidad: " + velocidadMovimiento);
    }
}