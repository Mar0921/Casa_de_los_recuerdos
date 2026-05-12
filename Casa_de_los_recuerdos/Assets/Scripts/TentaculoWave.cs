using UnityEngine;

public class TentaculoWave : MonoBehaviour
{
    [Header("Puntos de control")]
    public Transform[] huesos;
    public float velocidad = 2f;
    public float amplitud = 0.2f;

    private MeshFilter meshFilter;
    private Mesh mesh;
    private Vector3[] verticesOriginales;
    private Vector3[] verticesModificados;
    private Vector3[] posicionesHuesosOriginales;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = Instantiate(meshFilter.mesh);
        meshFilter.mesh = mesh;

        if (!mesh.isReadable)
        {
            Debug.LogError("❌ Activa Read/Write en el mesh");
            return;
        }

        verticesOriginales = mesh.vertices;
        verticesModificados = new Vector3[verticesOriginales.Length];

        // Guardar posiciones originales de los huesos
        posicionesHuesosOriginales = new Vector3[huesos.Length];
        for (int i = 0; i < huesos.Length; i++)
        {
            if (huesos[i] != null)
                posicionesHuesosOriginales[i] = huesos[i].localPosition;
        }

        Debug.Log($"✅ Listo. {huesos.Length} huesos, {verticesOriginales.Length} vértices");
    }

    void Update()
    {
        float tiempo = Time.time * velocidad;

        // Mover huesos
        for (int i = 0; i < huesos.Length; i++)
        {
            if (huesos[i] == null) continue;

            float t = i / (float)(huesos.Length - 1); // 0 = base, 1 = punta
            float offsetX = Mathf.Sin(tiempo + i) * amplitud * t;
            float offsetZ = Mathf.Cos(tiempo * 0.8f + i) * amplitud * 0.5f * t;

            huesos[i].localPosition = posicionesHuesosOriginales[i] + new Vector3(offsetX, 0, offsetZ);
        }

        // Deformar mesh (versión simplificada)
        for (int i = 0; i < verticesOriginales.Length; i++)
        {
            Vector3 vertice = verticesOriginales[i];
            Vector3 nuevaPos = vertice;

            // Encontrar el hueso más cercano (simplificado)
            int huesoCercano = 0;
            float menorDistancia = float.MaxValue;

            for (int j = 0; j < huesos.Length; j++)
            {
                float dist = Vector3.Distance(vertice, posicionesHuesosOriginales[j]);
                if (dist < menorDistancia)
                {
                    menorDistancia = dist;
                    huesoCercano = j;
                }
            }

            // Aplicar desplazamiento del hueso más cercano
            Vector3 desplazamiento = huesos[huesoCercano].localPosition - posicionesHuesosOriginales[huesoCercano];
            nuevaPos += desplazamiento * (1f - menorDistancia / 0.5f);

            verticesModificados[i] = nuevaPos;
        }

        mesh.vertices = verticesModificados;
        mesh.RecalculateNormals();
    }
}