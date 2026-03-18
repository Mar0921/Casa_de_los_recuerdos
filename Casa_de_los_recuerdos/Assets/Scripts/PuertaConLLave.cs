using UnityEngine;

public class PuertaConLlave : MonoBehaviour
{
    public string nombreLlaveRequerida = "Llave";
    public float anguloApertura = 90f;
    public float velocidadApertura = 2f;
    public bool consumirLlave = true;
    public string mensajeConLlave = "Presiona P para abrir la puerta";
    public string mensajeSinLlave = "Necesitas una llave para abrir esta puerta";

    private bool estaAbierta = false;
    private bool estaAbriendose = false;
    private Quaternion rotacionCerrada;
    private Quaternion rotacionAbierta;

    void Start()
    {
        rotacionCerrada = transform.rotation;
        rotacionAbierta = rotacionCerrada * Quaternion.Euler(0, -anguloApertura, 0);

        ActualizarMensaje();
    }

    void Update()
    {
        if (estaAbriendose)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotacionAbierta,
                Time.deltaTime * velocidadApertura
            );

            if (Quaternion.Angle(transform.rotation, rotacionAbierta) < 0.5f)
            {
                transform.rotation = rotacionAbierta;
                estaAbriendose = false;
                estaAbierta = true;
            }
        }

        if (!estaAbierta && Input.GetKeyDown(KeyCode.P))
        {
            IntentarAbrirPuerta();
        }

        ActualizarMensaje();
    }

    void ActualizarMensaje()
    {
        ObjetoInteractuable interactuable = GetComponent<ObjetoInteractuable>();
        if (interactuable != null)
        {
            interactuable.mensaje = TieneJugadorLlave() ? mensajeConLlave : mensajeSinLlave;
        }
    }

    bool TieneJugadorLlave()
    {
        if (Inventario.instancia == null)
            return false;

        foreach (ObjetoRecolectable obj in Inventario.instancia.ObtenerObjetos())
        {
            if (obj.nombreObjeto == nombreLlaveRequerida)
                return true;
        }
        return false;
    }

    void IntentarAbrirPuerta()
    {
        ObjetoRecolectable llave = BuscarLlaveEnInventario();

        if (llave != null)
        {
            AbrirPuerta();

            if (consumirLlave)
            {
                Inventario.instancia.QuitarObjeto(llave);
                Destroy(llave.gameObject);
                Debug.Log($"Llave '{nombreLlaveRequerida}' consumida.");
            }
        }
        else
        {
            Debug.Log(mensajeSinLlave);
        }
    }

    ObjetoRecolectable BuscarLlaveEnInventario()
    {
        if (Inventario.instancia == null)
            return null;

        foreach (ObjetoRecolectable obj in Inventario.instancia.ObtenerObjetos())
        {
            if (obj.nombreObjeto == nombreLlaveRequerida)
                return obj;
        }
        return null;
    }

    void AbrirPuerta()
    {
        Debug.Log("¡Puerta abierta!");
        estaAbriendose = true;
    }
}