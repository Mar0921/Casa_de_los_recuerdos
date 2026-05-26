using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestorRompibles : MonoBehaviour
{
    public static GestorRompibles instancia;

    public GameObject llaveObjeto;
    public float alturaInicial = 5f;
    public bool activarFisica = true;

    public AudioClip sonidoLlaveAparece;
    public GameObject efectoLlaveAparece;
    public GameObject personajeLumen;
    public float volumenSonido = 0.9f;

    public bool mostrarDebugInfo = true;

    private List<ObjetoRompible> rompiblesEnEscena = new List<ObjetoRompible>();
    private HashSet<ObjetoRompible> rompiblesDestruidos = new HashSet<ObjetoRompible>();
    private bool llaveYaActivada = false;
    public Font fuentePersonalizada;

    private bool mostrarMensajeLlave = false;
    private bool mensajeLlaveOculto = false;

    void Awake()
    {
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        instancia = this;
    }

    void Start()
    {
        if (llaveObjeto == null)
        {
            Debug.LogError("[GestorRompibles] ¡No hay llave asignada!");
            return;
        }

        PrepararLlave();
        Debug.Log("[GestorRompibles] Sistema iniciado. Esperando objetos rompibles...");
    }

    void PrepararLlave()
    {
        if (llaveObjeto == null) return;

        // Quitar tag para que no sea detectada por RecolectorInteractor
        llaveObjeto.tag = "Untagged";

        Vector3 posicionOriginal = llaveObjeto.transform.position;
        llaveObjeto.transform.position = new Vector3(
            posicionOriginal.x,
            posicionOriginal.y + alturaInicial,
            posicionOriginal.z
        );

        Rigidbody rb = llaveObjeto.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = llaveObjeto.AddComponent<Rigidbody>();
            Debug.Log("[GestorRompibles] Se agregó Rigidbody a la llave automáticamente.");
        }
        rb.isKinematic = true;
        rb.useGravity = false;

        Collider[] colliders = llaveObjeto.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
            col.enabled = false;

        Renderer[] renderers = llaveObjeto.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            if (!(rend is ParticleSystemRenderer))
                rend.enabled = false;
        }

        Debug.Log($"[GestorRompibles] Llave preparada en Y={llaveObjeto.transform.position.y}m");
    }

    public void RegistrarRompible(ObjetoRompible rompible)
    {
        if (rompible == null) return;

        if (!rompiblesEnEscena.Contains(rompible))
        {
            rompiblesEnEscena.Add(rompible);

            if (mostrarDebugInfo)
                Debug.Log($"[GestorRompibles] Registrado: '{rompible.gameObject.name}'. Total: {rompiblesEnEscena.Count}");
        }
    }

    public void NotificarRompibleDestruido(ObjetoRompible rompible)
    {
        if (llaveYaActivada) return;
        if (rompible == null) return;
        if (rompiblesDestruidos.Contains(rompible)) return;

        rompiblesDestruidos.Add(rompible);

        if (mostrarDebugInfo)
            Debug.Log($"[GestorRompibles] Destruido: '{rompible.gameObject.name}'. Progreso: {rompiblesDestruidos.Count}/{rompiblesEnEscena.Count}");

        if (rompiblesDestruidos.Count >= rompiblesEnEscena.Count && rompiblesEnEscena.Count > 0)
            ActivarLlave();
    }

    void ActivarLlave()
    {
        if (llaveYaActivada) return;
        if (llaveObjeto == null)
        {
            Debug.LogError("[GestorRompibles] No se puede activar la llave.");
            return;
        }

        llaveYaActivada = true;
        Debug.Log("[GestorRompibles] ¡TODOS LOS OBJETOS DESTRUIDOS! ¡Activando llave!");

        // Restaurar tag para que sea detectable y recolectable
        llaveObjeto.tag = "Recolectable";

        llaveObjeto.SetActive(true);

        Renderer[] renderers = llaveObjeto.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            if (!(rend is ParticleSystemRenderer))
                rend.enabled = true;
        }

        Collider[] colliders = llaveObjeto.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
            col.enabled = true;

        if (activarFisica)
        {
            Rigidbody rb = llaveObjeto.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.isKinematic = false;
                rb.useGravity = true;
                Debug.Log("[GestorRompibles] ¡Llave cayendo con física!");
            }
        }

        if (sonidoLlaveAparece != null)
        {
            Camera cam = Camera.main;
            Vector3 pos = cam != null ? cam.transform.position : llaveObjeto.transform.position;
            AudioSource.PlayClipAtPoint(sonidoLlaveAparece, pos, volumenSonido);
        }

        if (efectoLlaveAparece != null)
        {
            GameObject efecto = Instantiate(efectoLlaveAparece, llaveObjeto.transform.position, Quaternion.identity);
            Destroy(efecto, 5f); // Se destruye a los 5 segundos
        }

        mostrarMensajeLlave = true;
        StartCoroutine(OcultarMensajeLlave());
    }

    IEnumerator OcultarMensajeLlave()
    {
        yield return new WaitForSeconds(3f);
        mostrarMensajeLlave = false;
        mensajeLlaveOculto = true;
    }

    public int ObtenerRompiblesRestantes()
    {
        return rompiblesEnEscena.Count - rompiblesDestruidos.Count;
    }

    void OnGUI()
    {
        if (!mostrarDebugInfo) return;
        if (personajeLumen == null || !personajeLumen.activeInHierarchy) return;
        if (mensajeLlaveOculto) return;

        float anchoPantalla = Screen.width;
        float altoPantalla = Screen.height;

        GUIStyle estilo = new GUIStyle();
        estilo.alignment = TextAnchor.MiddleCenter;
        estilo.fontStyle = FontStyle.Bold;

        if (fuentePersonalizada != null)
            estilo.font = fuentePersonalizada;

        estilo.fontSize = 70;
        estilo.normal.textColor = Color.white;

        string info = $"Objetos: {rompiblesDestruidos.Count}/{rompiblesEnEscena.Count}";
        float yPrimeraLinea = (altoPantalla / 2) + 150;

        GUI.color = new Color(0, 0, 0, 0.6f);
        GUI.Box(new Rect(anchoPantalla / 2 - 300, yPrimeraLinea - 10, 600, 90), "");
        GUI.color = Color.white;
        GUI.Label(new Rect(0, yPrimeraLinea, anchoPantalla, 90), info, estilo);

        estilo.fontSize = 65;
        float ySegundaLinea = yPrimeraLinea + 95;

        if (llaveYaActivada && mostrarMensajeLlave)
        {
            estilo.normal.textColor = Color.green;
            GUI.color = new Color(0, 0.5f, 0, 0.6f);
            GUI.Box(new Rect(anchoPantalla / 2 - 300, ySegundaLinea - 10, 600, 85), "");
            GUI.color = Color.white;
            GUI.Label(new Rect(0, ySegundaLinea, anchoPantalla, 85), "Busca la llave", estilo);
        }
        else if (!llaveYaActivada && rompiblesEnEscena.Count > 0)
        {
            estilo.normal.textColor = Color.yellow;
            GUI.color = new Color(0.5f, 0.5f, 0, 0.6f);
            GUI.Box(new Rect(anchoPantalla / 2 - 300, ySegundaLinea - 10, 600, 85), "");
            GUI.color = Color.white;
            GUI.Label(new Rect(0, ySegundaLinea, anchoPantalla, 85), $"Quedan: {ObtenerRompiblesRestantes()}", estilo);
        }
    }

    void OnDestroy()
    {
        if (instancia == this)
            instancia = null;
    }
}