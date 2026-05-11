using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AI;
#endif

public class NavMeshBakeExclusor : MonoBehaviour
{
    [Header("Tags de objetos a EXCLUIR del NavMesh")]
    [Tooltip("Agrega aquí todos los tags cuyos objetos NO deben ser navegables.")]
    public List<string> tagsAExcluir = new List<string> { "Obstacle", "NoNavMesh" };

    [Header("Opciones")]
    [Tooltip("Si está activo, aplica las exclusiones automáticamente al iniciar el juego (runtime).")]
    public bool aplicarEnAwake = false;

    private void Awake()
    {
        if (aplicarEnAwake)
            AplicarExclusiones();
    }
    public void AplicarExclusiones()
    {
        int totalModificados = 0;

        foreach (string tag in tagsAExcluir)
        {
            GameObject[] objetosConTag;

            try
            {
                objetosConTag = GameObject.FindGameObjectsWithTag(tag);
            }
            catch (UnityException)
            {
                Debug.LogWarning($"[NavMeshBakeExclusor] El tag '{tag}' no existe en el proyecto. Agrégalo en Edit > Project Settings > Tags & Layers.");
                continue;
            }

            foreach (GameObject obj in objetosConTag)
            {
                Collider col = obj.GetComponent<Collider>();
                if (col == null)
                {
                    Debug.LogWarning($"[NavMeshBakeExclusor] '{obj.name}' tiene el tag '{tag}' pero no tiene Collider. Se omite.");
                    continue;
                }

                // Agregar o reutilizar NavMeshModifier
                NavMeshModifier modifier = obj.GetComponent<NavMeshModifier>();
                if (modifier == null)
                    modifier = obj.AddComponent<NavMeshModifier>();

                modifier.overrideArea = true;
                modifier.area = NavMesh.GetAreaFromName("Not Walkable");

                totalModificados++;
                Debug.Log($"[NavMeshBakeExclusor] '{obj.name}' (tag: {tag}) marcado como No Navegable.");
            }
        }

        Debug.Log($"[NavMeshBakeExclusor] {totalModificados} objeto(s) marcados como No Navegables.");
    }


#if UNITY_EDITOR

    public void AplicarYBakear()
    {
        AplicarExclusiones();
        UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
        Debug.Log("[NavMeshBakeExclusor] NavMesh bakeado correctamente.");
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(NavMeshBakeExclusor))]
public class NavMeshBakeExclusorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NavMeshBakeExclusor script = (NavMeshBakeExclusor)target;

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Aplicar Exclusiones (sin bakear)", GUILayout.Height(30)))
        {
            script.AplicarExclusiones();
        }

        EditorGUILayout.Space(4);

        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
        if (GUILayout.Button("Aplicar Exclusiones y BAKEAR NavMesh", GUILayout.Height(36)))
        {
            script.AplicarYBakear();
        }
        GUI.backgroundColor = Color.white;
    }
}
#endif