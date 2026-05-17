using UnityEngine;
using TMPro;
using System.Collections;

public class ZonaSegura : MonoBehaviour
{
    public MonstruoController monstruo;
    public ZonaPersecucion zonaPersecucion;
    public GameObject panelPersecucion;

    [Header("Text")]
    public TextMeshProUGUI textoPersecucion;
    public float velocidadEscritura = 0.07f;
    public float intensidadTemblor = 5f; 
    public float duracionTemblor = 0.1f; 

    private string mensajeOriginal = "";
    private Vector3 posicionOriginal;
    private RectTransform rectTexto;
    private bool efectoIniciado = false;

    private void Start()
    {
        if (panelPersecucion != null)
            panelPersecucion.SetActive(false);

        if (textoPersecucion != null)
        {
            mensajeOriginal = textoPersecucion.text;
            textoPersecucion.text = "";

            rectTexto = textoPersecucion.GetComponent<RectTransform>();
            if (rectTexto != null)
            {
                posicionOriginal = rectTexto.anchoredPosition;
                Debug.Log($"Posición original guardada: {posicionOriginal}");
            }
        }
    }

    private void Update()
    {
        if (!efectoIniciado && panelPersecucion != null && panelPersecucion.activeSelf)
        {
            IniciarEfectoEscritura();
        }
    }

    private void IniciarEfectoEscritura()
    {
        if (textoPersecucion != null && !efectoIniciado)
        {
            efectoIniciado = true;
            StartCoroutine(EscribirConTemblor());
        }
    }

    private IEnumerator EscribirConTemblor()
    {
        textoPersecucion.text = "";

        for (int i = 0; i <= mensajeOriginal.Length; i++)
        {
            textoPersecucion.text = mensajeOriginal.Substring(0, i);

            if (i > 0)
            {
                yield return StartCoroutine(TemblorLetra());
            }

            yield return new WaitForSeconds(velocidadEscritura);
        }

        Debug.Log("[Efecto] Texto completo mostrado");
    }

    private IEnumerator TemblorLetra()
    {
        float tiempoPasado = 0f;

        while (tiempoPasado < duracionTemblor)
        {
            float offsetX = Random.Range(-intensidadTemblor, intensidadTemblor);
            float offsetY = Random.Range(-intensidadTemblor, intensidadTemblor);

            if (rectTexto != null)
                rectTexto.anchoredPosition = posicionOriginal + new Vector3(offsetX, offsetY);

            tiempoPasado += Time.deltaTime;
            yield return null;
        }

        // Volver a la posición original
        if (rectTexto != null)
            rectTexto.anchoredPosition = posicionOriginal;
    }

    private void OnTriggerEnter(Collider other)
    {
        Transform jugador = ObtenerJugador(other);
        if (jugador == null)
            return;

        if (monstruo != null)
            monstruo.DesactivarYDesaparecer();

        if (panelPersecucion != null)
            panelPersecucion.SetActive(false);

        if (zonaPersecucion != null)
            zonaPersecucion.Resetear();

        efectoIniciado = false;
        if (textoPersecucion != null)
            textoPersecucion.text = "";

        if (rectTexto != null)
            rectTexto.anchoredPosition = posicionOriginal;

        Debug.Log("[ZonaSegura] El jugador entró a una zona segura.");
    }

    private Transform ObtenerJugador(Collider other)
    {
        if (other.CompareTag("Player"))
            return other.transform;
        Transform raiz = other.transform.root;
        if (raiz.CompareTag("Player"))
            return raiz;
        MovimientoPJ movimiento = other.GetComponentInParent<MovimientoPJ>();
        if (movimiento != null && movimiento.CompareTag("Player"))
            return movimiento.transform;
        return null;
    }
}