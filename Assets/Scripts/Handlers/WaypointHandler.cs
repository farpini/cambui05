using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WaypointHandler : MonoBehaviour
{
    protected WaypointType waypointType;
    protected int waypointIndex;

    public bool waypointProfessorAccess;
    public bool waypointStudentAccess;

    public WaypointType WaypointType => waypointType;

    public int WaypointIndex
    {
        get { return waypointIndex; }
        set { waypointIndex = value; }
    }

    //public int[] waypointsInRange;
    //public int[] desksInRange;
}

public enum WaypointType
{
    Floor, Desk
}