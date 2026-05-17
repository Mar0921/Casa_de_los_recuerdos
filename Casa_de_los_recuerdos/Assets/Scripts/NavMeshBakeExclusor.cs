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

    [Header("Opciones de Exclusión")]
    [Tooltip("Si está activo, aplica las exclusiones automáticamente al iniciar el juego (runtime).")]
    public bool aplicarEnAwake = false;

    [Header("Gestión de Tablas Caídas")]
    [Tooltip("Actualizar el NavMesh automáticamente cuando caigan tablas")]
    public bool gestionarTablasCaidas = true;

    [Tooltip("Tag de las tablas que deben ser monitoreadas")]
    public string tagTablas = "tabla";

    [Tooltip("Tiempo de espera antes de actualizar NavMesh (para agrupar múltiples caídas)")]
    public float tiempoEsperaActualizacion = 2f;

    [Tooltip("NavMesh Surface a actualizar dinámicamente (se busca automáticamente si está vacío)")]
    public NavMeshSurface navMeshSurface;

    [Header("Debug")]
    [Tooltip("Mostrar información de debug en pantalla durante el juego")]
    public bool mostrarDebugEnPantalla = false;

    // Referencias internas
    private List<TablasCaida> tablas = new List<TablasCaida>();
    private bool esperandoActualizacion = false;
    private int tablasEnSuelo = 0;

    private void Awake()
    {
        if (aplicarEnAwake)
            AplicarExclusiones();

        if (gestionarTablasCaidas)
            InicializarGestorTablas();
    }

    #region Exclusión de NavMesh (Funcionalidad Original)

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

    #endregion

    #region Gestión de Tablas Caídas

    private void InicializarGestorTablas()
    {
        // Buscar todas las tablas en la escena
        BuscarTablas();

        // Buscar NavMeshSurface si no se asignó
        if (navMeshSurface == null)
        {
            navMeshSurface = FindObjectOfType<NavMeshSurface>();

            if (navMeshSurface == null)
            {
                Debug.LogWarning("[NavMeshBakeExclusor] No se encontró NavMeshSurface. " +
                    "Agrega un NavMeshSurface a la escena para actualización dinámica del NavMesh.");
            }
        }

        // Iniciar monitoreo de tablas
        if (tablas.Count > 0)
        {
            InvokeRepeating(nameof(VerificarEstadoTablas), 1f, 0.5f);
            Debug.Log($"[NavMeshBakeExclusor] Monitoreando {tablas.Count} tabla(s) para actualización dinámica del NavMesh.");
        }
    }

    private void BuscarTablas()
    {
        GameObject[] objetosTabla;
        try
        {
            objetosTabla = GameObject.FindGameObjectsWithTag(tagTablas);
        }
        catch (UnityException)
        {
            Debug.LogWarning($"[NavMeshBakeExclusor] El tag '{tagTablas}' no existe. Crea el tag para las tablas.");
            return;
        }

        tablas.Clear();
        foreach (GameObject obj in objetosTabla)
        {
            TablasCaida tabla = obj.GetComponent<TablasCaida>();
            if (tabla != null && !tablas.Contains(tabla))
            {
                tablas.Add(tabla);
            }
        }
    }

    private void VerificarEstadoTablas()
    {
        int tablasEnSueloActual = 0;

        foreach (TablasCaida tabla in tablas)
        {
            if (tabla != null)
            {
                // Usar reflection para acceder al campo privado haCaido
                var campo = tabla.GetType().GetField("haCaido",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (campo != null && (bool)campo.GetValue(tabla))
                {
                    tablasEnSueloActual++;
                }
            }
        }

        // Si hay nuevas tablas en el suelo, programar actualización del NavMesh
        if (tablasEnSueloActual > tablasEnSuelo)
        {
            tablasEnSuelo = tablasEnSueloActual;
            ProgramarActualizacionNavMesh();
        }
    }

    private void ProgramarActualizacionNavMesh()
    {
        if (navMeshSurface == null) return;

        if (!esperandoActualizacion)
        {
            esperandoActualizacion = true;
            CancelInvoke(nameof(ActualizarNavMeshDinamico));
            Invoke(nameof(ActualizarNavMeshDinamico), tiempoEsperaActualizacion);
            Debug.Log($"[NavMeshBakeExclusor] NavMesh se actualizará en {tiempoEsperaActualizacion} segundos...");
        }
    }

    private void ActualizarNavMeshDinamico()
    {
        if (navMeshSurface != null)
        {
            // En lugar de reconstruir, desactiva/activa NavMeshModifier en las tablas
            foreach (TablasCaida tabla in tablas)
            {
                if (tabla != null && tabla.haCaido)
                {
                    // Marcar la tabla como navegable
                    var modifier = tabla.GetComponent<NavMeshModifier>();
                    if (modifier == null)
                        modifier = tabla.gameObject.AddComponent<NavMeshModifier>();

                    modifier.overrideArea = true;
                    modifier.area = 0; // Walkable
                }
            }

            // NO llames a BuildNavMesh si tienes meshes sin read access
            // esperandoActualizacion = false;
        }
    }

    /// <summary>
    /// Método público para resetear todas las tablas monitoreadas
    /// </summary>
    public void ResetearTodasLasTablas()
    {
        foreach (TablasCaida tabla in tablas)
        {
            if (tabla != null)
            {
                tabla.Resetear();
            }
        }

        tablasEnSuelo = 0;

        // Actualizar NavMesh después del reset
        if (navMeshSurface != null)
        {
            Invoke(nameof(ActualizarNavMeshDinamico), 0.5f);
        }

        Debug.Log("[NavMeshBakeExclusor] Todas las tablas han sido reseteadas.");
    }

    /// <summary>
    /// Agregar una tabla dinámicamente al sistema de monitoreo
    /// </summary>
    public void AgregarTabla(TablasCaida tabla)
    {
        if (tabla != null && !tablas.Contains(tabla))
        {
            tablas.Add(tabla);
            Debug.Log($"[NavMeshBakeExclusor] Tabla '{tabla.gameObject.name}' agregada al monitoreo.");
        }
    }

    /// <summary>
    /// Remover una tabla del sistema de monitoreo
    /// </summary>
    public void RemoverTabla(TablasCaida tabla)
    {
        if (tablas.Contains(tabla))
        {
            tablas.Remove(tabla);
            Debug.Log($"[NavMeshBakeExclusor] Tabla '{tabla.gameObject.name}' removida del monitoreo.");
        }
    }

    /// <summary>
    /// Refrescar la lista de tablas (útil si se instancian tablas en runtime)
    /// </summary>
    public void RefrescarListaTablas()
    {
        BuscarTablas();
        tablasEnSuelo = 0;
        Debug.Log($"[NavMeshBakeExclusor] Lista de tablas actualizada. Total: {tablas.Count}");
    }

    #endregion

    #region Debug en Pantalla

    private void OnGUI()
    {
        if (!mostrarDebugEnPantalla || !Application.isPlaying) return;

        GUILayout.BeginArea(new Rect(10, 10, 350, 150));

        // Fondo semi-transparente
        GUI.Box(new Rect(0, 0, 350, 150), "");

        GUILayout.Label($"<b>NavMesh Manager</b>", new GUIStyle(GUI.skin.label) { richText = true, fontSize = 14 });
        GUILayout.Space(5);

        if (gestionarTablasCaidas)
        {
            GUILayout.Label($"Tablas Total: {tablas.Count}");
            GUILayout.Label($"Tablas en Suelo: {tablasEnSuelo}");
            GUILayout.Label($"NavMesh Surface: {(navMeshSurface != null ? "✓" : "✗")}");

            GUILayout.Space(5);

            if (GUILayout.Button("Resetear Todas las Tablas", GUILayout.Height(25)))
            {
                ResetearTodasLasTablas();
            }

            if (GUILayout.Button("Refrescar Lista de Tablas", GUILayout.Height(25)))
            {
                RefrescarListaTablas();
            }
        }
        else
        {
            GUILayout.Label("Gestión de tablas desactivada");
        }

        GUILayout.EndArea();
    }

    #endregion

    #region Editor (Funcionalidad Original)

#if UNITY_EDITOR
    public void AplicarYBakear()
    {
        AplicarExclusiones();
        UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
        Debug.Log("[NavMeshBakeExclusor] NavMesh bakeado correctamente.");
    }
#endif

    #endregion
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
        EditorGUILayout.LabelField("Herramientas de Exclusión", EditorStyles.boldLabel);

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

        // Herramientas de gestión de tablas (solo en play mode)
        if (Application.isPlaying && script.gestionarTablasCaidas)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Herramientas de Tablas (Play Mode)", EditorStyles.boldLabel);

            if (GUILayout.Button("Refrescar Lista de Tablas", GUILayout.Height(30)))
            {
                script.RefrescarListaTablas();
            }

            EditorGUILayout.Space(4);

            GUI.backgroundColor = new Color(0.8f, 0.4f, 0.4f);
            if (GUILayout.Button("Resetear Todas las Tablas", GUILayout.Height(30)))
            {
                script.ResetearTodasLasTablas();
            }
            GUI.backgroundColor = Color.white;
        }
    }
}
#endif