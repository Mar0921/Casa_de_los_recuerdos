using UnityEngine;
using UnityEngine.InputSystem;

public class MovimientoPJ : MonoBehaviour
{
    public float velocidadMovimiento = 5f;
    public float velocidadRotacion = 200f;
    public float fuerzaDeSalto = 8f;
    public float fuerzaEmpuje = 5f;
    public bool puedoSaltar;

    private float velocidadInicial;
    public float velocidadAgachado;
    private bool estaAgachado = false;

    private Rigidbody rb;
    private Animator anim;
    private float x, y;
    private bool estaEmpujando = false;
    private Rigidbody objetoEmpujado = null;

    void Start()
    {
        puedoSaltar = false;
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        velocidadInicial = velocidadMovimiento;
        velocidadAgachado = velocidadMovimiento * 0.5f;
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

        //Si el personaje esta agachado:
        if (puedoSaltar)
        {
            if (Keyboard.current.rKey.isPressed)
            {
                estaAgachado = true;
                velocidadMovimiento = velocidadAgachado;
            }
            else
            {
                estaAgachado = false;
                velocidadMovimiento = velocidadInicial;
            }
        }
        else
        {
            // Si está en el aire, cancelar agachado
            estaAgachado = false;
            velocidadMovimiento = velocidadInicial;
        }

        anim.SetBool("agachado", estaAgachado);

        if (puedoSaltar && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Saltar();
        }

    }

    void FixedUpdate()
    {
        Camera cam = Camera.main;
        Vector3 forward = cam.transform.forward;
        Vector3 right = cam.transform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 movimiento = (right * x + forward * y) * velocidadMovimiento * Time.deltaTime;
        transform.position += movimiento;

        if (movimiento != Vector3.zero)
        {
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

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Suelo"))
        {
            puedoSaltar = true;
            anim.SetBool("salte", false);
            anim.SetBool("tocoSuelo", true);
        }
        if (collision.gameObject.CompareTag("Empujable"))
        {
            estaEmpujando = true;
            objetoEmpujado = collision.gameObject.GetComponent<Rigidbody>();
        }
    }
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Empujable"))
        {
            estaEmpujando = false;
            objetoEmpujado = null;
        }
    }
}