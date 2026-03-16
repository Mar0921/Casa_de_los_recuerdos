using UnityEngine;
using UnityEngine.InputSystem;

public class MovimientoPJ : MonoBehaviour
{
    public float velocidadMovimiento = 5f;
    public float velocidadRotacion = 200f;
    public float fuerzaDeSalto = 8f;
    public bool puedoSaltar;

    private Rigidbody rb;
    private Animator anim;
    private float x, y;

    void Start()
    {
        puedoSaltar = false;
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
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
    }
}