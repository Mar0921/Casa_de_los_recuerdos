using UnityEngine;
using System.Collections;

public class CambioPersonaje : MonoBehaviour
{
    public GameObject lumen;
    public GameObject vyre;
    public float cooldown = 0.5f;

    private bool puedeCambiar = true;
    private bool estaCambiando = false;
    private GameObject personajeActual;

    void Start()
    {
        personajeActual = lumen;
        lumen.SetActive(true);
        vyre.SetActive(false);
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
        Renderer[] renderers = personaje.GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0)
        {
            float tiempo = 0.2f;
            float t = 0;

            Color[] coloresOriginales = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                coloresOriginales[i] = renderers[i].material.color;
            }

            if (desaparecer)
            {
                while (t < tiempo)
                {
                    t += Time.deltaTime;
                    float alpha = Mathf.Lerp(1f, 0f, t / tiempo);

                    for (int i = 0; i < renderers.Length; i++)
                    {
                        Color c = coloresOriginales[i];
                        renderers[i].material.color = new Color(c.r, c.g, c.b, alpha);
                    }

                    yield return null;
                }
            }
            else
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    Color c = coloresOriginales[i];
                    renderers[i].material.color = new Color(c.r, c.g, c.b, 0f);
                }

                while (t < tiempo)
                {
                    t += Time.deltaTime;
                    float alpha = Mathf.Lerp(0f, 1f, t / tiempo);

                    for (int i = 0; i < renderers.Length; i++)
                    {
                        Color c = coloresOriginales[i];
                        renderers[i].material.color = new Color(c.r, c.g, c.b, alpha);
                    }

                    yield return null;
                }
            }

            float alphaFinal = desaparecer ? 0f : 1f;
            for (int i = 0; i < renderers.Length; i++)
            {
                Color c = coloresOriginales[i];
                renderers[i].material.color = new Color(c.r, c.g, c.b, alphaFinal);
            }
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
        }
    }
}