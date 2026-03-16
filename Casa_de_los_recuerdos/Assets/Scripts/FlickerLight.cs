using UnityEngine;
using System.Collections;

public class FlickerLight : MonoBehaviour
{
    public Light myLight;
    public float minIntensity = 0.5f;
    public float maxIntensity = 2f;
    public float flickerSpeed = 0.05f;

    void Start()
    {
        if (myLight == null)
            myLight = GetComponent<Light>();

        StartCoroutine(Flicker());
    }

    IEnumerator Flicker()
    {
        while (true)
        {
            myLight.intensity = Random.Range(minIntensity, maxIntensity);
            yield return new WaitForSeconds(flickerSpeed);
        }
    }
}