using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserShooter : MonoBehaviour
{
    public Material newMaterial;  // Arrastra el material nuevo aquí en el Inspector.
    public VolumeController volumeController;  // Referencia al script de control de volumen.
    private GameObject lastHitObject;
    private Material lastHitMaterial;

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        RestoreMaterial();

        if (Physics.Raycast(ray, out hit))
        {
            lastHitObject = hit.collider.gameObject;
            Renderer targetRenderer = lastHitObject.GetComponent<Renderer>();
            if (targetRenderer != null)
            {
                lastHitMaterial = targetRenderer.material;
                targetRenderer.material = newMaterial;
            }
            AudioSource audioSource = lastHitObject.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                volumeController.SetActiveAudioSource(audioSource);
            }
        }
    }

    void RestoreMaterial()
    {
        if (lastHitObject != null)
        {
            Renderer targetRenderer = lastHitObject.GetComponent<Renderer>();
            if (targetRenderer != null)
            {
                targetRenderer.material = lastHitMaterial;
            }
            lastHitObject = null;
            lastHitMaterial = null;
        }
    }
}