using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorHandler : WaypointHandler
{
    public int roomIndex;

    public void Awake()
    {
        waypointType = WaypointType.Door;
    }
}
