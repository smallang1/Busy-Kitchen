using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    private Player player;
    private float stepSoundRate = 0.12f;
    private float stepSoundTimer = 0;
    void Start()
    {
        player = GetComponent<Player>();
    }

    
    void Update()
    {
        if (player == null || !player.enabled)
        {
            return;
        }
        stepSoundTimer += Time.deltaTime;
        if (stepSoundTimer >= stepSoundRate)
        {
            stepSoundTimer = 0;
            if (player.IsWalking)
            {
                float volume = 0.3f;
                SoundManager.Instance.PlayStepSound(volume);
            }         
        }
    }
}