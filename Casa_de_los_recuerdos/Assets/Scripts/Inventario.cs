using System.Collections.Generic;
using UnityEngine;

public class Inventario : MonoBehaviour
{
    public static Inventario instancia;

    private List<string> objetos = new List<string>();

    void Awake()
    {
        instancia = this;
    }

    public void AgregarObjeto(string nombre)
    {
        objetos.Add(nombre);
        Debug.Log($"Inventario: se agregó '{nombre}'. Total: {objetos.Count}");
    }

    public List<string> ObtenerObjetos()
    {
        return objetos;
    }
}
