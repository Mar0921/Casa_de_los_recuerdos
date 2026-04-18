using UnityEngine;

public class CamaraSeguir : MonoBehaviour
{
    public Vector3 offset = new Vector3(0f, 5f, 7f);
    public float suavidad = 5f;

    private Transform objetivo;

    void LateUpdate()
    {
        BuscarPlayerActivo();

        if (!objetivo) return;

        Vector3 posicionDeseada = objetivo.position + offset;
        transform.position = Vector3.Lerp(transform.position, posicionDeseada, suavidad * Time.deltaTime);

        transform.LookAt(objetivo);
    }

    void BuscarPlayerActivo()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject p in players)
        {
            if (p.activeInHierarchy)
            {
                objetivo = p.transform;
                return;
            }
        }
    }
}