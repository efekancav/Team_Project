using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;
    private AudioSource audioSource;

    public AudioClip musicClip;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource.clip = musicClip;
            audioSource.Play();
        }
        else
        {
            // Если музыка другая → меняем
            if (instance.audioSource.clip != musicClip)
            {
                instance.audioSource.clip = musicClip;
                instance.audioSource.Play();
            }

            Destroy(gameObject);
        }
    }
}