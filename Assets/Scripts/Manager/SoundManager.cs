using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private const string SOUNDMANAGER_VOLUME = "volume";

    [SerializeField]private AudioClipRefsSO audioClipRefsSO;

    private int volume = 5; //“Ù¡ø

    private void Awake()
    {
        Instance = this;
        LoadVolume();
    }
    private void Start()
    {
        OrderManager.Instance.OnRecipeSucceed += OrderManager_OnRecipeSucceed;
        OrderManager.Instance.OnRecipeFailed += OrderManager_OnRecipeFailed;
        CuttingCounter.OnCut += CuttingCounter_OnCut;
        KitchenObjectHolder.OnDrop += KitchenObjectHolder_OnDrop;
        KitchenObjectHolder.OnPickup += KitchenObjectHolder_OnPickup;
        TrashCounter.OnObjectTrashed += TrashCounter_OnObjectTrashed;
        
    }

    private void TrashCounter_OnObjectTrashed(object sender, System.EventArgs e)
    {
        PlaySound(audioClipRefsSO.trash);
    }

    private void KitchenObjectHolder_OnPickup(object sender, System.EventArgs e)
    {
        PlaySound(audioClipRefsSO.objectPickup);
    }

    private void KitchenObjectHolder_OnDrop(object sender, System.EventArgs e)
    {
        PlaySound(audioClipRefsSO.objectDrop);
    }

    private void CuttingCounter_OnCut(object sender, System.EventArgs e)
    {
        PlaySound(audioClipRefsSO.chop); //≤•∑≈«–≤À…˘“Ù
    }

    private void OrderManager_OnRecipeFailed(object sender, System.EventArgs e)
    {
        PlaySound(audioClipRefsSO.deliveryFail); //≤•∑≈ ß∞‹…˘“Ù
    }

    private void OrderManager_OnRecipeSucceed(object sender, System.EventArgs e)
    {
        PlaySound(audioClipRefsSO.deliverySuccess);
    }
    public void PlayWarningSound(float volumeMutipler = .8f)
    {
        PlaySound(audioClipRefsSO.warning,volumeMutipler);
    }
    public void PlayCountDownSound(float volumeMutipler = .1f)
    {
        PlaySound(audioClipRefsSO.warning, volumeMutipler);
    }
    public void PlayStepSound(float volumeMutipler = .1f)
    {
        PlaySound(audioClipRefsSO.footstep, volumeMutipler);
    }
    private void PlaySound(AudioClip[] clips,float volumeMutipler = .1f)
    {
        PlaySound(clips, Camera.main.transform.position, volumeMutipler);
    }
    private void PlaySound(AudioClip[] clips,Vector3 position,float volumeMutipler = .1f)
    {
        if(volume == 0) return;
        int index = Random.Range(0, clips.Length);
        AudioSource.PlayClipAtPoint(clips[index], position, volumeMutipler * (volume/10.0f));
    }
    public void ChangeVolum()
    {
        //volume  0-10   0-1
        volume++;
        if(volume > 10)
        {
            volume = 0;
        }
        SaveVolume();

    }
    public int GetVolume()
    {
        return volume; //∑µªÿ“Ù¡ø
    }
    private void SaveVolume()
    {
        PlayerPrefs.SetInt(SOUNDMANAGER_VOLUME, volume);
    }
    private void LoadVolume()
    {
        volume = PlayerPrefs.GetInt(SOUNDMANAGER_VOLUME,volume);

    }
}
