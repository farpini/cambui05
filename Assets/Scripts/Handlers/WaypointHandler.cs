using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public abstract class WaypointHandler : InteractionHandler
{
    [SerializeField] protected WaypointType waypointType;
    protected int waypointIndex;

    public WaypointType WaypointType => waypointType;

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