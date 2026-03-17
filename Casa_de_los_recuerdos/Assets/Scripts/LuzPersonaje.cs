using UnityEngine;
using UnityEngine.InputSystem;

public class LuzPersonaje : MonoBehaviour
{
    public Light luz;
    public float intensidadMax = 3f;
    public float duracion = 6f;
    public float cooldown = 8f;
    public float velocidadTransicion = 2f;

    private float tiempoRestante = 0f;
    private float tiempoCooldown = 0f;
    private bool luzActiva = false;

    void Start()
    {
        luz.intensity = 0f;
        luz.enabled = false;
    }

    void Update()
    {
        // Activar con L si no está en cooldown
        if (Keyboard.current.lKey.wasPressedThisFrame && tiempoCooldown <= 0f && !luzActiva)
        {
            luzActiva = true;
            tiempoRestante = duracion;
            luz.enabled = true;
        }

        if (luzActiva)
        {
            // Fade in suave
            luz.intensity = Mathf.MoveTowards(luz.intensity, intensidadMax, velocidadTransicion * Time.deltaTime);

            tiempoRestante -= Time.deltaTime;

            if (tiempoRestante <= 0f)
            {
                luzActiva = false;
                tiempoCooldown = cooldown;
            }
        }
        else
        {
            // Fade out suave
            luz.intensity = Mathf.MoveTowards(luz.intensity, 0f, velocidadTransicion * Time.deltaTime);

            if (luz.intensity <= 0f)
                luz.enabled = false;

            if (tiempoCooldown > 0f)
                tiempoCooldown -= Time.deltaTime;
        }
    }
}