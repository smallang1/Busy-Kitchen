using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    private const string MUSICMANAGER_VOLUME = "MusicManagerVolume"; //音量设置的key值

    private AudioSource audioSource;

    private float originalVolume;
    private int volume = 5; //用户可以设置的大小 volume = 5; //用户可以设置的大小

    private void Awake()
    {
        Instance = this;
        LoadVolume();
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        originalVolume = audioSource.volume;
        UpdateVolume();
    }

    private void UpdateVolume()
    {
        if(volume == 0)
        {
            audioSource.enabled = false;
        }
        else
        {
            audioSource.enabled = true; //音量不为0时，启用音频源
            audioSource.volume = originalVolume * (volume / 10.0f);
        }
    }
    public void ChangeVolume()
    {
        volume++;
        if(volume > 10)
        {
            volume = 0;
        }
        SaveVolume();
        UpdateVolume();
    }
    public int GetVolume()
    {
        return volume;
    }
    private void SaveVolume()
    {
        PlayerPrefs.SetInt(MUSICMANAGER_VOLUME, volume);
    }
    private void LoadVolume()
    {
        volume = PlayerPrefs.GetInt(MUSICMANAGER_VOLUME,volume);
    }
}
