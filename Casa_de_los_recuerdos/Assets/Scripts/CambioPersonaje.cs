using UnityEngine;
using System.Collections;

public class CambioPersonaje : MonoBehaviour
{
    public GameObject lumen;
    public GameObject vyre;
    public float cooldown = 0.5f;

    [Header("Efectos")]
    public ParticleSystem particulasCambio; // Prefab o referencia a un sistema de partículas

    [Header("Sonido")]
    public AudioSource audioSource;
    public AudioClip sonidoCambio;
    public float volumenSonido = 1f;

    private bool puedeCambiar = true;
    private bool estaCambiando = false;
    private GameObject personajeActual;

    void Start()
    {
        personajeActual = lumen;
        lumen.SetActive(true);
        vyre.SetActive(false);

        // Configurar AudioSource si no está asignado
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && sonidoCambio != null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }
        if (audioSource != null)
            audioSource.volume = volumenSonido;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && puedeCambiar && !estaCambiando)
        {
            StartCoroutine(CambiarPersonaje());
        }
    }

    IEnumerator CambiarPersonaje()
    {
        estaCambiando = true;
        puedeCambiar = false;

        // 🔊 Reproducir sonido al inicio del cambio
        if (audioSource != null && sonidoCambio != null)
        {
            audioSource.PlayOneShot(sonidoCambio, volumenSonido);
        }

        // Mostrar partículas en la posición del personaje actual
        if (particulasCambio != null)
        {
            ParticleSystem ps = Instantiate(particulasCambio, personajeActual.transform.position, Quaternion.identity);
            ps.Play();
            Destroy(ps.gameObject, ps.main.duration + 0.5f);
        }

        // Guardar posición y rotación
        Vector3 posicion = personajeActual.transform.position;
        Quaternion rotacion = personajeActual.transform.rotation;

        // Guardar velocidad del Rigidbody si existe
        Vector3 velocidad = Vector3.zero;
        Rigidbody rbActual = personajeActual.GetComponent<Rigidbody>();
        if (rbActual != null)
        {
            velocidad = rbActual.linearVelocity;
        }

        // Determinar nuevo personaje
        GameObject nuevoPersonaje = (personajeActual == lumen) ? vyre : lumen;

        // 1️⃣ Efecto de desaparición
        yield return EfectoCambio(personajeActual, true);

        // 2️⃣ DESACTIVAR el personaje actual
        personajeActual.SetActive(false);

        // 3️⃣ Configurar posición del nuevo ANTES de activarlo
        nuevoPersonaje.transform.position = posicion;
        nuevoPersonaje.transform.rotation = rotacion;

        // 4️⃣ ACTIVAR el nuevo personaje
        nuevoPersonaje.SetActive(true);

        // 🆕 5️⃣ Esperar un frame para que se inicialicen los componentes
        yield return null;

        // 🆕 6️⃣ RESETEAR completamente el Animator
        Animator animNuevo = nuevoPersonaje.GetComponent<Animator>();
        if (animNuevo != null)
        {
            animNuevo.Rebind(); // Resetea el animator
            animNuevo.Update(0f); // Actualiza al frame 0

            // 🔥 CRÍTICO: Resetear específicamente el parámetro "ilumine"
            if (animNuevo.parameters != null)
            {
                foreach (AnimatorControllerParameter param in animNuevo.parameters)
                {
                    if (param.name == "ilumine")
                    {
                        animNuevo.SetBool("ilumine", false);
                        break;
                    }
                }
            }
        }

        // 🆕 7️⃣ Si es Lumen, resetear el script de luz también
        if (nuevoPersonaje == lumen)
        {
            LuzPersonaje luzScript = nuevoPersonaje.GetComponent<LuzPersonaje>();
            if (luzScript != null)
            {
                // Si usas el script corregido:
                // luzScript.ResetearLuz();

                // O forzar el reset manual:
                Light luz = luzScript.luz;
                if (luz != null)
                {
                    luz.intensity = 0f;
                    luz.enabled = false;
                }
            }
        }

        // 8️⃣ Restaurar velocidad
        Rigidbody rbNuevo = nuevoPersonaje.GetComponent<Rigidbody>();
        if (rbNuevo != null)
        {
            rbNuevo.linearVelocity = velocidad;
        }

        // 9️⃣ Actualizar referencia
        personajeActual = nuevoPersonaje;

        // 🔟 Efecto de aparición
        yield return EfectoCambio(personajeActual, false);

        // 1️⃣1️⃣ Cooldown
        yield return new WaitForSeconds(cooldown);

        estaCambiando = false;
        puedeCambiar = true;
    }

    IEnumerator EfectoCambio(GameObject personaje, bool desaparecer)
    {
        // Obtener todos los renderers actuales
        Renderer[] todosRenderers = personaje.GetComponentsInChildren<Renderer>();

        // Filtrar solo los que existen y no están siendo destruidos
        var renderersValidos = new System.Collections.Generic.List<Renderer>();
        var coloresOriginales = new System.Collections.Generic.List<Color>();

        for (int i = 0; i < todosRenderers.Length; i++)
        {
            if (todosRenderers[i] != null && todosRenderers[i].gameObject != null)
            {
                try
                {
                    Color col = ObtenerColor(todosRenderers[i].material);
                    renderersValidos.Add(todosRenderers[i]);
                    coloresOriginales.Add(col);
                }
                catch (System.Exception)
                {
                    // Si el material no es accesible, ignorar este renderer
                    continue;
                }
            }
        }

        if (renderersValidos.Count == 0)
        {
            yield return new WaitForSeconds(0.2f);
            yield break;
        }

        float tiempo = 0.2f;
        float t = 0;

        if (desaparecer)
        {
            while (t < tiempo)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, t / tiempo);

                for (int i = 0; i < renderersValidos.Count; i++)
                {
                    if (renderersValidos[i] == null) continue;
                    Color c = coloresOriginales[i];
                    c.a = alpha;
                    AsignarColor(renderersValidos[i].material, c);
                }
                yield return null;
            }
        }
        else
        {
            // Fijar alpha 0 al inicio
            for (int i = 0; i < renderersValidos.Count; i++)
            {
                if (renderersValidos[i] == null) continue;
                Color c = coloresOriginales[i];
                c.a = 0f;
                AsignarColor(renderersValidos[i].material, c);
            }

            while (t < tiempo)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, t / tiempo);

                for (int i = 0; i < renderersValidos.Count; i++)
                {
                    if (renderersValidos[i] == null) continue;
                    Color c = coloresOriginales[i];
                    c.a = alpha;
                    AsignarColor(renderersValidos[i].material, c);
                }
                yield return null;
            }
        }

        // Restaurar alpha final
        float alphaFinal = desaparecer ? 0f : 1f;
        for (int i = 0; i < renderersValidos.Count; i++)
        {
            if (renderersValidos[i] == null) continue;
            Color c = coloresOriginales[i];
            c.a = alphaFinal;
            AsignarColor(renderersValidos[i].material, c);
        }
    }

    // Método auxiliar para obtener color según el shader
    private Color ObtenerColor(Material mat)
    {
        if (mat.HasProperty("_Color"))
            return mat.GetColor("_Color");
        else if (mat.HasProperty("_TintColor"))
            return mat.GetColor("_TintColor");
        else
            return Color.white; // fallback
    }

    // Método auxiliar para asignar color según el shader
    private void AsignarColor(Material mat, Color color)
    {
        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", color);
        else if (mat.HasProperty("_TintColor"))
            mat.SetColor("_TintColor", color);
        // Si no tiene ninguna, no se puede cambiar el color, pero evitamos error
    }
}