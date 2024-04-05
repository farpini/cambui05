using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorHandler : WaypointHandler
{
    public int floorIndex;
    public Color floorSelectedColor;
    public Color floorDefaultColor;
    public MeshRenderer floorMeshRenderer;

    public void Awake ()
    {
        waypointType = WaypointType.Floor;
        OnUnselect();
    }

    public override void OnSelect ()
    {
        floorMeshRenderer.material.color = floorSelectedColor;
    }

    public override void OnUnselect ()
    {
        floorMeshRenderer.material.color = floorDefaultColor;
    }
}