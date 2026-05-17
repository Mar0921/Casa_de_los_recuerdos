using UnityEngine;
using System.Collections;

/// <summary>
/// Versión con FÍSICA REAL - La tabla se vuelca usando Rigidbody y torque
/// Más realista pero menos predecible
/// </summary>
public class TablasCaida : MonoBehaviour
{
    public float distanciaActivacion = 3f;
    public string tagJugador = "Player";
    public float tiempoEspera = 0.3f;

    [Header("Configuración de Caída con Física")]
    public float fuerzaTorque = 500f;
    public float gravedadExtra = 2f;
    public float empujeAdelante = 3f;

    [Header("Configuración de NavMesh")]
    public bool hacerNavegable = true;
    public LayerMask layerNavegable;
    private Rigidbody rb;
    private Collider miCollider;
    private Transform jugador;
    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;
    public bool haCaido = false;
    private bool cayendo = false;
    private Vector3 direccionCaida;
    private bool torqueAplicado = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        miCollider = GetComponent<Collider>();

        // Configurar Rigidbody
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.mass = 10f; // Masa decente para física
        rb.angularDamping = 0.5f; // Un poco de resistencia angular

        posicionInicial = transform.position;
        rotacionInicial = transform.rotation;

        GameObject jugadorObj = GameObject.FindGameObjectWithTag(tagJugador);
        if (jugadorObj != null) jugador = jugadorObj.transform;

        if (gameObject.tag != "tabla") gameObject.tag = "tabla";
    }

    void Update()
    {
        if (haCaido || cayendo || jugador == null) return;

        float distancia = Vector3.Distance(transform.position, jugador.position);
        if (distancia <= distanciaActivacion)
        {
            // Dirección hacia el jugador (solo horizontal)
            direccionCaida = (jugador.position - transform.position).normalized;
            direccionCaida.y = 0;
            direccionCaida.Normalize();

            StartCoroutine(IniciarCaidaFisica());
        }
    }

    void FixedUpdate()
    {
        // Aplicar gravedad extra mientras cae
        if (cayendo && !haCaido)
        {
            rb.AddForce(Vector3.down * gravedadExtra, ForceMode.Acceleration);
        }
    }

    IEnumerator IniciarCaidaFisica()
    {
        cayendo = true;
        Debug.Log($"🎯 {gameObject.name}: Volcándose con física real");

        yield return new WaitForSeconds(tiempoEspera);

        // Activar física
        rb.isKinematic = false;
        rb.useGravity = true;

        // Calcular eje de rotación perpendicular a la dirección de caída
        Vector3 ejeRotacion = Vector3.Cross(direccionCaida, Vector3.up).normalized;

        if (ejeRotacion.magnitude < 0.1f)
        {
            ejeRotacion = transform.right;
        }

        // Aplicar torque para volcar la tabla
        rb.AddTorque(ejeRotacion * fuerzaTorque, ForceMode.Impulse);

        // Pequeño empuje hacia adelante
        rb.AddForce(direccionCaida * empujeAdelante, ForceMode.Impulse);

        torqueAplicado = true;

        // Esperar a que se estabilice
        StartCoroutine(VerificarEstabilizacion());
    }

    IEnumerator VerificarEstabilizacion()
    {
        yield return new WaitForSeconds(1.5f); // Dar tiempo a que caiga

        // Verificar si está quieta
        float tiempoEspera = 0f;
        float tiempoMaximo = 5f; // Máximo 5 segundos esperando

        while (tiempoEspera < tiempoMaximo)
        {
            // Si la velocidad es muy baja, está estabilizada
            if (rb.linearVelocity.magnitude < 0.1f && rb.angularVelocity.magnitude < 0.1f)
            {
                yield return new WaitForSeconds(0.3f); // Pequeña pausa adicional
                FinalizarCaida();
                yield break;
            }

            tiempoEspera += Time.deltaTime;
            yield return null;
        }

        // Si pasó mucho tiempo, forzar finalización
        Debug.LogWarning($"{gameObject.name}: Tiempo máximo alcanzado, finalizando caída");
        FinalizarCaida();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!cayendo || haCaido) return;

        Debug.Log($"🔄 {gameObject.name}: Colisión con {collision.gameObject.name}");

        // Empujar al jugador si lo golpea
        if (collision.gameObject.CompareTag(tagJugador))
        {
            EmpujarJugador(collision.gameObject, collision);
        }
    }

    void EmpujarJugador(GameObject jugadorObj, Collision collision)
    {
        // Calcular dirección del impacto basada en el punto de contacto
        Vector3 puntoContacto = collision.contacts[0].point;
        Vector3 direccionEmpuje = (jugadorObj.transform.position - puntoContacto).normalized;
        direccionEmpuje.y = 0;

        CharacterController controller = jugadorObj.GetComponent<CharacterController>();
        Rigidbody jugadorRb = jugadorObj.GetComponent<Rigidbody>();

        // Calcular fuerza basada en la velocidad de la tabla
        float fuerzaBase = 12f;
        float factorVelocidad = Mathf.Clamp(rb.linearVelocity.magnitude, 0.5f, 3f);
        float fuerzaEmpuje = fuerzaBase * factorVelocidad;

        if (controller != null)
        {
            StartCoroutine(EmpujarConCharacterController(controller, direccionEmpuje, fuerzaEmpuje));
        }
        else if (jugadorRb != null)
        {
            jugadorRb.AddForce(direccionEmpuje * fuerzaEmpuje, ForceMode.Impulse);
        }

        Debug.Log($"💥 Tabla golpeó al jugador con fuerza {fuerzaEmpuje:F1}!");
    }

    IEnumerator EmpujarConCharacterController(CharacterController controller, Vector3 direccion, float fuerza)
    {
        float distancia = fuerza / 6f; // Distancia proporcional a la fuerza
        float recorrido = 0f;
        float velocidad = fuerza;

        while (recorrido < distancia)
        {
            float movimiento = velocidad * Time.deltaTime;
            controller.Move(direccion * movimiento);
            recorrido += movimiento;
            velocidad *= 0.85f;
            yield return null;
        }
    }

    void FinalizarCaida()
    {
        if (haCaido) return;

        haCaido = true;
        cayendo = false;

        // Congelar la tabla
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Si debe ser navegable
        if (hacerNavegable)
        {
            if (layerNavegable.value != 0)
            {
                gameObject.layer = Mathf.RoundToInt(Mathf.Log(layerNavegable.value, 2));
            }
        }

        Debug.Log($"🏁 {gameObject.name}: Caída completada (física)!");
    }

    public void Resetear()
    {
        haCaido = false;
        cayendo = false;
        torqueAplicado = false;

        transform.position = posicionInicial;
        transform.rotation = rotacionInicial;

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.None;

        if (miCollider != null) miCollider.enabled = true;

        // Volver al layer original
        gameObject.layer = LayerMask.NameToLayer("TablasCaidas");

        Debug.Log($"🔄 {gameObject.name}: Reseteada");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaActivacion);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * 3f);

        // Mostrar dirección de caída
        if (Application.isPlaying && (cayendo || haCaido))
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + direccionCaida * 3f);
        }
    }
}