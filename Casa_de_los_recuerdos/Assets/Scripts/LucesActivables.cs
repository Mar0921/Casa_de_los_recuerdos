using UnityEngine;

public class LucesActivables : MonoBehaviour
{
    [Header("Luces")]
    public Light[] luces;
    public float intensidadMax = 3f;
    public float velocidadTransicion = 1.5f;

    [Header("Imagen en el mundo")]
    public Renderer imagenMundo;      // Arrastra el Quad aquí
    public float delayImagen = 2f;    // Segundos antes de aparecer
    public float velocidadFade = 1f;

    private bool encendiendo = false;
    private bool apagando = false;
    private float timerImagen = 0f;
    private bool imagenActivada = false;
    private Material mat;

    void Start()
    {
        foreach (Light luz in luces)
        {
            luz.intensity = 0f;
            luz.enabled = true;
        }

        if (imagenMundo != null)
        {
            mat = imagenMundo.material;
            // Empieza completamente transparente
            Color c = mat.color;
            c.a = 0f;
            mat.color = c;
            imagenMundo.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (encendiendo)
        {
            foreach (Light luz in luces)
            {
                luz.intensity = Mathf.MoveTowards(luz.intensity, intensidadMax, velocidadTransicion * Time.deltaTime);
            }

            // Contar el delay antes de aparecer la imagen
            if (!imagenActivada)
            {
                timerImagen += Time.deltaTime;
                if (timerImagen >= delayImagen)
                    imagenActivada = true;
            }

            // Fade in de la imagen
            if (imagenActivada && mat != null)
            {
                Color c = mat.color;
                c.a = Mathf.MoveTowards(c.a, 1f, velocidadFade * Time.deltaTime);
                mat.color = c;
            }
        }
        else if (apagando)
        {
            foreach (Light luz in luces)
            {
                luz.intensity = Mathf.MoveTowards(luz.intensity, 0f, velocidadTransicion * Time.deltaTime);
            }

            // Fade out de la imagen
            if (mat != null)
            {
                Color c = mat.color;
                c.a = Mathf.MoveTowards(c.a, 0f, velocidadFade * Time.deltaTime);
                mat.color = c;
            }

            timerImagen = 0f;
            imagenActivada = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            encendiendo = true;
            apagando = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            encendiendo = false;
            apagando = true;
        }
    }
}