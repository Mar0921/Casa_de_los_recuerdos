using System.Collections.Generic;
using UnityEngine;

public class Inventario : MonoBehaviour
{
    public static Inventario instancia;

    private List<ObjetoRecolectable> objetos = new List<ObjetoRecolectable>();

    void Awake()
    {
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        instancia = this;
    }

    public void AgregarObjeto(ObjetoRecolectable objeto)
    {
        if (objeto == null)
        {
            Debug.LogWarning("[Inventario] Se intentó agregar un objeto nulo.");
            return;
        }

        objetos.Add(objeto);
        Debug.Log($"[Inventario] Se agregó '{objeto.nombreObjeto}'. Total: {objetos.Count}");
    }

    public void QuitarObjeto(ObjetoRecolectable objeto)
    {
        if (objeto == null) return;

        if (objetos.Remove(objeto))
        {
            Debug.Log($"[Inventario] Se quitó '{objeto.nombreObjeto}'. Total: {objetos.Count}");
        }
    }

    public ObjetoRecolectable ObtenerUltimoObjeto()
    {
        if (objetos.Count == 0)
            return null;

        return objetos[objetos.Count - 1];
    }

    public bool TieneObjetos()
    {
        return objetos.Count > 0;
    }

    public List<ObjetoRecolectable> ObtenerObjetos()
    {
        return objetos;
    }
}