using UnityEngine;
using UnityEngine.AI;

public class MonstruoPerseguir : MonoBehaviour
{
    [Header("Referencias")]
    public NavMeshAgent agent;
    public Animator animator;

    [Header("Estado visual")]
    public GameObject modeloVisual; // opcional, si lo dejas vacío usa este mismo GameObject

    [Header("Configuración")]
    public bool ocultoAlIniciar = true;
    public float distanciaMinimaActualizacion = 0.2f;

    private Transform objetivo;
    private bool persiguiendo = false;

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
        if (ocultoAlIniciar)
        {
            DesactivarYDesaparecer();
        }
    }

    void Update()
    {
        if (!persiguiendo || objetivo == null || agent == null || !agent.enabled)
            return;

        float distancia = Vector3.Distance(ultimoDestino, objetivo.position);

        if (distancia >= distanciaMinimaActualizacion)
        {
            agent.SetDestination(objetivo.position);
            ultimoDestino = objetivo.position;
        }

        if (animator != null)
        {
            animator.SetFloat("Velocidad", agent.velocity.magnitude);
        }
    }

    public void ActivarPersecucion(Transform objetivoNuevo, Transform puntoAparicion)
    {
        if (objetivoNuevo == null)
        {
            Debug.LogWarning("[MonstruoPerseguidor] No se recibió objetivo para perseguir.");
            return;
        }

        objetivo = objetivoNuevo;
        persiguiendo = true;

        MostrarMonstruo(true);

        if (agent != null && !agent.enabled)
            agent.enabled = true;

        Vector3 posicionSpawn = puntoAparicion != null ? puntoAparicion.position : transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(posicionSpawn, out hit, 2f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
        else
        {
            transform.position = posicionSpawn;
            Debug.LogWarning("[MonstruoPerseguidor] El punto de aparición no cayó sobre el NavMesh. Se usó la posición directa.");
        }

        agent.isStopped = false;
        agent.ResetPath();
        agent.SetDestination(objetivo.position);
        ultimoDestino = objetivo.position;

        Debug.Log("[MonstruoPerseguidor] Monstruo activado y persiguiendo al jugador.");
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

        if (animator != null)
        {
            animator.SetFloat("Velocidad", 0f);
        }

        MostrarMonstruo(false);

        Debug.Log("[MonstruoPerseguidor] Monstruo desactivado.");
    }

    void MostrarMonstruo(bool mostrar)
    {
        foreach (Renderer rend in renderersMonstruo)
            rend.enabled = mostrar;

        foreach (Collider col in collidersMonstruo)
            col.enabled = mostrar;
    }
}
