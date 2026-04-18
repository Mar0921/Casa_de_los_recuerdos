using UnityEngine;
using UnityEngine.InputSystem;

public class VyreController : MonoBehaviour
{
    private MovimientoPJ movimiento;
    private Rigidbody rb;

    [Header("Vyre")]
    public float velocidadCorrer = 9f;
    public int maxSaltos = 2;

    private int saltosRestantes;

    void Start()
    {
        movimiento = GetComponent<MovimientoPJ>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 🏃 CORRER
        if (Keyboard.current.leftShiftKey.isPressed)
        {
            movimiento.velocidadMovimiento = velocidadCorrer;
        }
        else
        {
            movimiento.velocidadMovimiento = movimiento.velocidadNormal;
        }

        // 🦘 DOBLE SALTO (sin romper el salto base)
        if (Keyboard.current.spaceKey.wasPressedThisFrame && saltosRestantes > 0)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * movimiento.fuerzaDeSalto, ForceMode.Impulse);

            saltosRestantes--;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Reset saltos
        if (((1 << collision.gameObject.layer) & movimiento.capaSuelo) != 0)
        {
            saltosRestantes = maxSaltos;
        }

        // 💥 ROMPER
        if (collision.gameObject.CompareTag("Rompible"))
        {
            Destroy(collision.gameObject);
        }
    }
}