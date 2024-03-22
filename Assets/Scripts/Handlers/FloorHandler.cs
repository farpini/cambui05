using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorHandler : WaypointHandler
{
    public int floorIndex;

    public void Awake ()
    {
        waypointType = WaypointType.Floor;
    }


}
