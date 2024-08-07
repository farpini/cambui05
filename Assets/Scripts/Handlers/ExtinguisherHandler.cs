using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtinguisherHandler : MonoBehaviour
{
    [SerializeField] private GameObject smokeEffect;
    [SerializeField] private Transform smokeOutTransform;
    [SerializeField] private AudioSource smokeAudio;


    public void Awake ()
    {
        DeactivateExtinguisher();
    }

    public void ActivateExtinguisher ()
    {
        smokeEffect.SetActive(true);
        smokeAudio.Play();
    }

    public void DeactivateExtinguisher ()
    {
        smokeEffect.SetActive(false);
        smokeAudio.Pause();
    }
}