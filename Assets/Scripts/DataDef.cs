using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


[Serializable]
public class WaypointsData : ScriptableObject
{
    public List<int[]> waypointsInRangeList;
    public List<int[]> desksWaypointsInRangeList;
    public List<int[]> waypointsDesksInRangeList;

    public void Initialize ()
    {
        waypointsInRangeList = new List<int[]>
        {
            new int[] { 1, 2, 7 }, //0
            new int[] { 0, 2, 3 }, //1
            new int[] { 0, 1, 4 }, //2
            new int[] { 1, 5 }, //3
            new int[] { 2, 6 }, //4
            new int[] { 3 }, //5
            new int[] { 4 }, //6
            new int[] { 0 }, //7
        };

        desksWaypointsInRangeList = new List<int[]>
        {
            new int[] { 1, 3}, //0
            new int[] { 3 }, //1
            new int[] { 5 }, //2
            new int[] { 5 }, //3
            new int[] { 1, 3}, //4
            new int[] { 3 }, //5
            new int[] { 5 }, //6
            new int[] { 5 }, //7
            new int[] { 2, 4 }, //8
            new int[] { 4 }, //9
            new int[] { 6 }, //10
            new int[] { 6 }, //11
            new int[] { 2, 4 }, //12
            new int[] { 4 }, //13
            new int[] { 6 }, //14
            new int[] { 6 }, //15
        };

        waypointsDesksInRangeList = new List<int[]>
        {
            new int[] { 0, 4},
            new int[] { 8, 12},
            new int[] { 0, 1, 4, 5},
            new int[] { 8, 9, 12, 13},
            new int[] { 2, 3, 6, 7},
            new int[] { 10, 11, 14, 15},
        };
    }
}

[Serializable]
public class UserRegisterData
{
    public string username;
    public string matricula;
    public string genero;
    public string tipo;

    public UserRegisterData (string _u, string _m, string _g, string _t)
    {
        username = _u;
        matricula = _m;
        genero = _g;
        tipo = _t;
    }
}

[Serializable]
public class UserRuntimeData
{
    public string waypoint;
    public string roomId;
    public string state;

    public UserRuntimeData (int _w, int _rid, ClientState _cs)
    {
        waypoint = _w.ToString();
        roomId = _rid.ToString();
        state = _cs.ToString();
    }
}

public enum ClientState
{
    Idle = 0,
    Walking = 1,
    Sit = 2
}

public enum UserRegisterAttribute
{
    username, matricula, genero, tipo
}

public enum UserRuntimeAttribute
{
    waypoint, roomId, state
}

public enum ClientGender
{
    none, masculino, feminino
}

public enum ClientType
{
    professor, aluno
}

public enum ClientStatus
{
    offline, online
}