using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public abstract class WaypointHandler : InteractionHandler
{
    [SerializeField] protected WaypointType waypointType;
    [SerializeField] protected bool waypointForceDirection = false;
    [SerializeField] protected Vector3 waypointEnterDirection = Vector3.zero;
    protected int waypointIndex;

    public WaypointType WaypointType => waypointType;
    public bool WaypointForceDirection => waypointForceDirection;
    public Vector3 WaypointEnterDirection => waypointEnterDirection;

    public Action<WaypointHandler> OnWaypointSelected;

    public int WaypointIndex
    {
        get { return waypointIndex; }
        set { waypointIndex = value; }
    }

    //public int[] waypointsInRange;
    //public int[] desksInRange;

    public void WaypointSelected ()
    {
        Debug.Log("Selected");
        OnWaypointSelected?.Invoke(this);
    }

    public void WaypointActivate ()
    {
        //Debug.Log("Activate");
        //OnWaypointSelected?.Invoke(this);
    }
}

public enum WaypointType
{
    Floor, Desk, Door
}