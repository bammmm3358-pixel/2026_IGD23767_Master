using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip levelBGM;
    public AudioClip bossBGM;

    void Start()
    {
        PlayBGM(levelBGM);
    }

    public void PlayBGM(AudioClip clip)
    {
        if (audioSource.clip == clip) return;
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void PlayBossMusic()
    {
        PlayBGM(bossBGM);
    }
}