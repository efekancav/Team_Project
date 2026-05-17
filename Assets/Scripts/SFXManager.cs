using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [Header("Player")]
    public AudioClip jump;
    public AudioClip damage;
    public AudioClip hit;

    [Header("Interactions")]
    public AudioClip collect;
    public AudioClip buttonPress;
    public AudioClip buttonRelease;

    [Header("Objects")]
    public AudioClip blockMove;
    public AudioClip bounce;

    [Header("Level")]
    public AudioClip levelFinish;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySFX(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}