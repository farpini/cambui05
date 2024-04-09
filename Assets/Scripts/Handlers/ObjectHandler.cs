using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectHandler : InteractionHandler
{
    protected Vector3 startPosition;
    protected Vector3 startEulerAngles;

    [SerializeField] protected int epiId;

    public int EpiId => epiId;

    public Action<ObjectHandler> OnObjectPicked;
    public Func<ObjectHandler, DropData> OnObjectDropped;


    private void Start()
    {
        startPosition = transform.position;
        startEulerAngles = transform.eulerAngles;
    }

    public void RestartPosition()
    {
        DropData result = OnObjectDropped.Invoke(this);
        if (result.dropOnOrigin)
        {
            transform.position = startPosition;
            transform.eulerAngles = startEulerAngles;
        }
        else
        {
            transform.position = result.transformToDrop.position;
            transform.rotation = result.transformToDrop.rotation;
            StartCoroutine(DestroyObjectOnSeconds(2f));
        }
    }

    public void PickEPI ()
    {
        OnObjectPicked?.Invoke(this);
    }

    private IEnumerator DestroyObjectOnSeconds (float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }
}

public class DropData
{
    public Transform transformToDrop;
    public bool dropOnOrigin;
}