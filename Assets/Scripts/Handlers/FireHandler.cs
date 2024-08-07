using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireHandler : MonoBehaviour
{
    [SerializeField] private AudioSource fireAudio;

    public void ActivateFire ()
    {
        gameObject.SetActive(true);
        fireAudio.Play();
    }

    public void DeactivateFire ()
    {
        gameObject.SetActive(false);
        fireAudio.Stop();
    }
}