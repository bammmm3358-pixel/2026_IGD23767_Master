using UnityEngine;
using UnityEngine.UI;

// ใช้ทดสอบเสียง: ผูกปุ่ม OnClick กับ method และผูก Slider กับตัวแปร
public class SoundTestUI : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;

    void Start()
    {
        var am = AudioManager.Instance;
        if (am == null) return;

        if (musicSlider)
        {
            musicSlider.value = am.musicVolume;
            musicSlider.onValueChanged.AddListener(am.SetMusicVolume);
        }
        if (sfxSlider)
        {
            sfxSlider.value = am.sfxVolume;
            sfxSlider.onValueChanged.AddListener(am.SetSFXVolume);
        }
    }

    public void TestAttack() { if (AudioManager.Instance) AudioManager.Instance.PlayAttack(); }
    public void TestHurt()   { if (AudioManager.Instance) AudioManager.Instance.PlayHurt(); }
    public void TestPickup() { if (AudioManager.Instance) AudioManager.Instance.PlayPickup(); }
    public void TestWin()    { if (AudioManager.Instance) AudioManager.Instance.PlayWin(); }
    public void TestLose()   { if (AudioManager.Instance) AudioManager.Instance.PlayLose(); }
}
