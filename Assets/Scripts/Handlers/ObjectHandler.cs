using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectHandler : InteractionHandler
{
    protected Vector3 startPosition;
    protected Vector3 startEulerAngles;

    private void Start()
    {
        startPosition = transform.position;
        startEulerAngles = transform.eulerAngles;
    }

    public void RestartPosition()
    {
        Debug.Log("Foi");
        transform.position = startPosition;
        transform.eulerAngles = startEulerAngles;
    }
}
