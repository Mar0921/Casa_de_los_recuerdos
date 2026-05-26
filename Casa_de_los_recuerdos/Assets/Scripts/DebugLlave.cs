using UnityEngine;

public class DebugLlave : MonoBehaviour
{
    void OnDisable()
    {
        Debug.LogWarning($"Llave desactivada desde: {gameObject.name}");
        Debug.LogWarning(System.Environment.StackTrace);
    }

    void OnDestroy()
    {
        Debug.LogWarning($"Llave DESTRUIDA");
        Debug.LogWarning(System.Environment.StackTrace);
    }
}