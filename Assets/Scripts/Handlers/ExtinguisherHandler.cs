using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExtinguisherHandler : ObjectHandler
{
    [SerializeField] private GameObject smokeEffect;
    [SerializeField] private Transform smokeOutTransform;
    [SerializeField] private AudioSource smokeAudio;
    [SerializeField] private Collider fireCollider;
    [SerializeField] private Transform target;
    [SerializeField] private GameObject extinguisherPanel;
    [SerializeField] private Image extinguisherBar;

    private bool isRayOnFire;
    private bool fireExtinguished;
    private float extinguishedTotal;

    public Action<int> OnFireExtinguisherStateChanged;

    public void Awake ()
    {
        DeactivateExtinguisher();
    }

    public void ActivateExtinguisher ()
    {
        extinguisherPanel.SetActive(true);
        smokeEffect.SetActive(true);
        smokeAudio.Play();
    }

    public void DeactivateExtinguisher ()
    {
        extinguisherPanel.SetActive(false);
        smokeEffect.SetActive(false);
        smokeAudio.Pause();
    }

    public void Initialize ()
    {
        extinguishedTotal = 0f;
        extinguisherBar.fillAmount = 0f;
        fireExtinguished = false;
        extinguisherPanel.SetActive(false);
        StartCoroutine(CheckingHitFire());
    }

    public void PickExtinguisher ()
    {
        if (!fireExtinguished)
        {
            OnFireExtinguisherStateChanged?.Invoke(1);
            PickEPI();
        }
    }

    public void RestartExtinguisherPosition ()
    {
        RestartPosition();

        if (!fireExtinguished )
        {
            OnFireExtinguisherStateChanged?.Invoke(0);
        }
    }

    private IEnumerator CheckingHitFire ()
    {
        while (!fireExtinguished)
        {
            var ray = new Ray();
            ray.origin = smokeOutTransform.position;
            ray.direction = (target.position - smokeOutTransform.position).normalized;

            var checkHit = fireCollider.Raycast(ray, out var rayCastInfo, 10f);

            if (checkHit && !isRayOnFire)
            {
                ActivateExtinguisher();
            }
            else if (!checkHit && isRayOnFire)
            {
                DeactivateExtinguisher();
            }
            else if (isRayOnFire)
            {
                extinguishedTotal += 0.1f;
                extinguisherBar.fillAmount = extinguishedTotal;

                if (extinguishedTotal >= 1f)
                {
                    extinguisherBar.fillAmount = 1f;
                    fireExtinguished = true;
                    DeactivateExtinguisher();
                    OnFireExtinguisherStateChanged?.Invoke(2);
                }
            }

            isRayOnFire = checkHit;

            yield return new WaitForSeconds(0.5f);
        }
    }
}