using UnityEngine;

public class EstrellaBrillo : MonoBehaviour
{
    [Header("Efecto")]
    public Light luzEstrella;          // Opcional: una luz local
    public Material materialOriginal;
    public Material materialBrillante;
    public float duracionBrillo = 5f;

    private Renderer rend;
    private float temporizador = 0f;
    private bool brillando = false;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null && materialOriginal == null)
            materialOriginal = rend.material;

        if (luzEstrella != null)
            luzEstrella.enabled = false;
    }

    void Update()
    {
        if (brillando)
        {
            temporizador -= Time.deltaTime;
            if (temporizador <= 0f)
            {
                ApagarBrillo();
            }
        }
    }

    // Método llamado desde LuzPersonaje
    public void Brillar(float duracion)
    {
        duracionBrillo = duracion;
        temporizador = duracionBrillo;
        brillando = true;

        // Cambiar material a brillante
        if (rend != null && materialBrillante != null)
            rend.material = materialBrillante;

        // Encender luz de la estrella si tiene
        if (luzEstrella != null)
            luzEstrella.enabled = true;
    }

    void ApagarBrillo()
    {
        brillando = false;
        // Volver al material original
        if (rend != null && materialOriginal != null)
            rend.material = materialOriginal;

        if (luzEstrella != null)
            luzEstrella.enabled = false;
    }
}