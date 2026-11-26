using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public static AudioManager Instance { get; private set; }



    [Header("Audio Sources (สร้างอัตโนมัติถ้าไม่ใส่)")]

    public AudioSource musicSource;

    public AudioSource sfxSource;


    [Header("Clips")]

    public AudioClip bgm;
    public AudioClip attackClip;
    public AudioClip hurtClip;
    public AudioClip pickupClip;
    public AudioClip winClip;
    public AudioClip loseClip;

    [Range(0, 1)] public float musicVolume = 0.5f;
    [Range(0, 1)] public float sfxVolume = 1f;

    void Awake()
    {

        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;

        musicSource.playOnAwake = false;
        sfxSource.playOnAwake = false;

        ApplyVolume();

    }
    void Start()

    {
    
        if (bgm != null) PlayMusic(bgm);
    
    }



    void ApplyVolume()
    {
    
        musicSource.volume = musicVolume;
    
        sfxSource.volume = sfxVolume;
    
    }



    public void PlayMusic(AudioClip clip)

    {
    
        if (clip == null) return;
    
        musicSource.clip = clip;
        musicSource.Play();
    
    }

    public void PlaySFX(AudioClip clip)

    {
        if (clip != null) sfxSource.PlayOneShot(clip, sfxVolume);
    
    }



    public void SetMusicVolume(float v) { musicVolume = v; ApplyVolume(); }
    public void SetSFXVolume(float v) { sfxVolume = v; ApplyVolume(); }

    public void PlayAttack() => PlaySFX(attackClip);
    public void PlayHurt() => PlaySFX(hurtClip);

    public void PlayPickup() => PlaySFX(pickupClip);

    public void PlayWin() => PlaySFX(winClip);

    public void PlayLose() => PlaySFX(loseClip);



}