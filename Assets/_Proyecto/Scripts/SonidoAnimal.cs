using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SonidoAnimal : MonoBehaviour
{
    [SerializeField] private AudioClip sonido;
    [Tooltip("Segundos de espera antes de que pueda volver a sonar")]
    [SerializeField] private float espera = 4f;
    [Range(0f, 1f)]
    [SerializeField] private float volumen = 0.8f;

    private AudioSource audioSource;
    private float proximoSonido;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;       
        audioSource.minDistance = 3f;        
        audioSource.maxDistance = 25f;       
        audioSource.rolloffMode = AudioRolloffMode.Linear;
    }

    // Lo llama el sistema de mirada (CameraPointerManager) cuando la mirada toca al animal
    public void OnPointerEnterXR()
    {
        if (sonido == null || Time.time < proximoSonido) return;

        // Un tono un poquito distinto cada vez para que no suene repetido
        audioSource.pitch = Random.Range(0.92f, 1.08f);
        audioSource.PlayOneShot(sonido, volumen);
        proximoSonido = Time.time + espera;
    }
}
