using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeskHandler : WaypointHandler
{
    public int deskIndex;

    public void Awake ()
    {
        waypointType = WaypointType.Desk;
    }
}
