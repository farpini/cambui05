using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public abstract class InteractionHandler : MonoBehaviour
{
    [SerializeField] protected XRSimpleInteractable XRInter;
    [SerializeField] protected InteractionType interactionType;
    public bool waypointProfessorAccess;
    public bool waypointStudentAccess;

    public void SetXRManager (XRInteractionManager xrmanager)
    {
        XRInter.interactionManager = xrmanager;
    }

    public virtual void OnUnselect () 
    { 
    }

    public virtual void OnSelect ()
    {
    }
}

public enum InteractionType
{
    Waypoint, Button, Object
}