using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class MonstruoController : MonoBehaviour
{
    [Header("Referencias")]
    public NavMeshAgent agent;
    public Animator animator;

    [Header("Jugador")]
    public ScreamController screamControllerJugador;

    [Header("Estado visual")]
    public GameObject modeloVisual;

    [Header("Configuracion")]
    public bool ocultoAlIniciar = true;
    public float distanciaMinimaActualizacion = 0.2f;
    public float baseOffset = 0f;

    [Header("Zona Segura")]
    public bool enZonaSegura = false;

    [Header("Ataque")]
    public LayerMask playerLayer;
    public string playerTag = "Player";

    private Transform objetivo;
    private bool persiguiendo = false;
    private bool haAtacado = false;
    private Renderer[] renderersMonstruo;
    private Collider[] collidersMonstruo;
    private Vector3 ultimoDestino;

    void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        GameObject raizVisual = modeloVisual != null ? modeloVisual : gameObject;
        renderersMonstruo = raizVisual.GetComponentsInChildren<Renderer>(true);
        collidersMonstruo = GetComponentsInChildren<Collider>(true);
    }

    void Start()
    {
        if (agent != null)
            agent.baseOffset = baseOffset;

        if (ocultoAlIniciar)
            DesactivarYDesaparecer();

        ActualizarEstadoAnimacion();
    }

    void Update()
    {
        if (!persiguiendo || objetivo == null || agent == null || !agent.enabled || haAtacado)
        {
            ActualizarEstadoAnimacion();
            return;
        }

        float distancia = Vector3.Distance(ultimoDestino, objetivo.position);
        if (distancia >= distanciaMinimaActualizacion)
        {
            agent.SetDestination(objetivo.position);
            ultimoDestino = objetivo.position;
        }

        ActualizarEstadoAnimacion();
    }

    private void ActualizarEstadoAnimacion()
    {
        if (animator == null) return;

        animator.SetBool("isInSafeZone", enZonaSegura);

        if (enZonaSegura)
        {
            animator.SetBool("isWalking", false);
        }
        else if (persiguiendo && !haAtacado)
        {
            bool estaCaminando = agent != null && agent.enabled && agent.velocity.magnitude > 0.1f;
            animator.SetBool("isWalking", estaCaminando);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (haAtacado || enZonaSegura) return;
        if (EsJugador(collision.gameObject)) AtacarJugador();
    }

    void OnTriggerEnter(Collider other)
    {
        if (haAtacado || enZonaSegura) return;
        if (EsJugador(other.gameObject)) AtacarJugador();
    }

    private bool EsJugador(GameObject obj)
    {
        return obj.CompareTag(playerTag) || ((playerLayer.value & (1 << obj.layer)) != 0);
    }

    private void AtacarJugador()
    {
        haAtacado = true;
        persiguiendo = false;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetTrigger("doScream");
        }

        if (screamControllerJugador != null)
        {
            screamControllerJugador.ForzarScream();
        }
        else
        {
            GameObject jugador = GameObject.FindGameObjectWithTag(playerTag);
            if (jugador != null)
            {
                ScreamController sc = jugador.GetComponent<ScreamController>();
                if (sc != null) sc.ForzarScream();
            }
        }

        Debug.Log("[MonstruoController] Ataque al jugador.");
    }

    public void ActivarPersecucion(Transform objetivoNuevo, Transform puntoAparicion)
    {
        if (objetivoNuevo == null)
        {
            Debug.LogWarning("[MonstruoController] No se recibió objetivo.");
            return;
        }

        haAtacado = false;
        objetivo = objetivoNuevo;
        persiguiendo = true;
        enZonaSegura = false;

        MostrarMonstruo(true);

        if (agent != null && !agent.enabled)
            agent.enabled = true;

        if (agent != null)
            agent.baseOffset = baseOffset;

        Vector3 posicionSpawn = puntoAparicion != null ? puntoAparicion.position : transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(posicionSpawn, out hit, 2f, NavMesh.AllAreas))
            agent.Warp(hit.position);
        else
        {
            transform.position = posicionSpawn;
            Debug.LogWarning("[MonstruoController] Punto de aparición fuera del NavMesh.");
        }

        agent.isStopped = false;
        agent.ResetPath();
        agent.SetDestination(objetivo.position);
        ultimoDestino = objetivo.position;

        ActualizarEstadoAnimacion();

        Debug.Log("[MonstruoController] Monstruo activado, persiguiendo.");
    }

    public void DesactivarYDesaparecer()
    {
        persiguiendo = false;
        objetivo = null;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;
        }

        ActualizarEstadoAnimacion();
        MostrarMonstruo(false);
        Debug.Log("[MonstruoController] Monstruo desactivado.");
    }

    public void EstablecerZonaSegura(bool estaEnZonaSegura)
    {
        enZonaSegura = estaEnZonaSegura;
        ActualizarEstadoAnimacion();
        Debug.Log("[MonstruoController] Zona segura: " + estaEnZonaSegura);
    }

    void MostrarMonstruo(bool mostrar)
    {
        foreach (Renderer rend in renderersMonstruo)
            rend.enabled = mostrar;
        foreach (Collider col in collidersMonstruo)
            col.enabled = mostrar;
    }
}