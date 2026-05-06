using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class MonstruoPerseguir : MonoBehaviour
{
    [Header("Referencias")]
    public NavMeshAgent agent;
    public Animator animator;

    [Header("Estado visual")]
    public GameObject modeloVisual;

    [Header("Configuración")]
    public bool ocultoAlIniciar = true;
    public float distanciaMinimaActualizacion = 0.2f;

    [Header("Ataque y reinicio")]
    public LayerMask playerLayer;
    public string playerTag = "Player";
    public float duracionFade = 1f;
    public float delayAntesFade = 2f;           // Nuevo: tiempo de espera antes del fade
    public Image pantallaFade;                  // Imagen asignada manualmente (debe estar oculta al inicio)
    public string nombreEscenaReinicio = "";

    private Transform objetivo;
    private bool persiguiendo = false;
    private bool haAtacado = false;

    private Renderer[] renderersMonstruo;
    private Collider[] collidersMonstruo;
    private Vector3 ultimoDestino;

    private Image fadeImage;
    private GameObject fadeCanvasObj;

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
        InicializarFade();
    }

    private void InicializarFade()
    {
        if (pantallaFade != null)
        {
            fadeImage = pantallaFade;
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(false);
        }
        else
        {
            fadeImage = null;
            fadeCanvasObj = null;
        }
    }

    void Update()
    {
        if (!persiguiendo || objetivo == null || agent == null || !agent.enabled || haAtacado)
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

    void OnCollisionEnter(Collision collision)
    {
        if (haAtacado) return;
        if (EsJugador(collision.gameObject))
            AtacarJugador();
    }

    void OnTriggerEnter(Collider other)
    {
        if (haAtacado) return;
        if (EsJugador(other.gameObject))
            AtacarJugador();
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
            animator.SetBool("EstaAtacando", true);
        }

        Debug.Log("[MonstruoPerseguidor] ¡Ataque al jugador! Esperando " + delayAntesFade + " segundos antes del fade.");
        StartCoroutine(IniciarFadeConDelay());
    }

    // Nueva corrutina que espera y luego inicia el fade
    private IEnumerator IniciarFadeConDelay()
    {
        yield return new WaitForSeconds(delayAntesFade);
        yield return StartCoroutine(FadeYReiniciar());
    }

    private IEnumerator FadeYReiniciar()
    {
        // Crear o conseguir la imagen de fade
        if (fadeImage == null)
        {
            GameObject canvasObj = new GameObject("FadeCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;

            GameObject imageObj = new GameObject("FadeImage");
            imageObj.transform.SetParent(canvas.transform, false);
            Image img = imageObj.AddComponent<Image>();
            img.color = Color.black;

            RectTransform rect = img.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            CanvasGroup group = canvasObj.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;

            img.gameObject.SetActive(false);
            fadeImage = img;
            fadeCanvasObj = canvasObj;

            DontDestroyOnLoad(canvasObj);
        }

        fadeImage.gameObject.SetActive(true);
        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;

        float tiempo = 0f;
        while (tiempo < duracionFade)
        {
            tiempo += Time.deltaTime;
            float alpha = Mathf.Clamp01(tiempo / duracionFade);
            color.a = alpha;
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;

        if (string.IsNullOrEmpty(nombreEscenaReinicio))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        else
            SceneManager.LoadScene(nombreEscenaReinicio);
    }

    public void ActivarPersecucion(Transform objetivoNuevo, Transform puntoAparicion)
    {
        if (objetivoNuevo == null)
        {
            Debug.LogWarning("[MonstruoPerseguidor] No se recibió objetivo para perseguir.");
            return;
        }

        haAtacado = false;
        objetivo = objetivoNuevo;
        persiguiendo = true;

        MostrarMonstruo(true);

        if (agent != null && !agent.enabled)
            agent.enabled = true;

        Vector3 posicionSpawn = puntoAparicion != null ? puntoAparicion.position : transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(posicionSpawn, out hit, 2f, NavMesh.AllAreas))
            agent.Warp(hit.position);
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
            animator.SetFloat("Velocidad", 0f);

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