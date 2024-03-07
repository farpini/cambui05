using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Google.MiniJSON;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Metadata;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Analytics;

public class UserManager : MonoBehaviour
{
    public static UserManager instance;

    public PlayerHandler playerHandlerPrefab;
    public MateHandler mateHandlerPrefab;

    public PlayerHandler playerHandler;

    public WaypointHandler[] waypoints;
    public DeskHandler[] desks;

    public List<int[]> waypointsInRangeList;
    public List<int[]> desksWaypointsInRangeList;
    public List<int[]> waypointsDesksInRangeList;

    public UserData userDataGlobal;

    //public UserData userData;

    public PlayerSO playerData;
    public List<MateSO> matesData;

    bool pendingUserRead = false;
    string pendingUserJson = "";

    int usersConnected;

    

    private void Awake ()
    {
        if (instance == null) instance = this;

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

    private void Start ()
    {
        //OnLoginSuccess();
        // OnMateLogged((int)Random.Range(0.0f, 100.0f));
        FirebaseManager.instance.OnLoginSuccess += OnLogin;
        FirebaseManager.instance.OnNewRegister += SetNewUserData;

        if (waypoints.Length != waypointsInRangeList.Count)
        {
            Debug.LogError("error");
            return;
        }

        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i].waypointsInRange = waypointsInRangeList[i];
        }

        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i].waypointsInRange = waypointsInRangeList[i];
            if (i >= 1 && i <= 6) waypoints[i].desksInRange = waypointsDesksInRangeList[i - 1];
        }

        for (int i = 0; i < desks.Length; i++)
        {
            desks[i].waypointsInRange = desksWaypointsInRangeList[i];
        }
    }

    private void Update ()
    {
        // testing only
        if (Input.GetKeyDown(KeyCode.J))
        {
            OnClientStateChanged(playerData.clientState);
            Debug.Log("OK");
        }

        if (matesData.Count < usersConnected - 1)
        {
            //adicionar userAPI na lista mate
        }
        else if (matesData.Count > usersConnected - 1)
        {
            //remover userAPI da lista mate
        }
    }

    private void OnLogin (string _userId)
    {
        Debug.Log("Login success");

        // get the user data from the database
        GetUserData(_userId);

        // create a player handler for this client
        CreatePlayerHandler(_userId);

        // notify other clients that this user connected
        UpdateUsersConnected();
    }

    private void GetUserData (string userId)
    {
        FirebaseManager.instance.GetUser(userId, (userDataGlobal) =>
        {
            Debug.Log("Player data loaded from the database successfuly!");
        });
    }

    private void SetNewUserData (string _userId, string _userName, string _matricula, GenderType _gender, ClientType _type)
    {
        userDataGlobal = new UserData
        {
            atributos = new Dictionary<string, object>() 
            {
                { "username", _userName},
                { "matricula", _matricula},
                { "waypoint", 0 },
                { "sala", 0 },
                { "genero", _gender.ToString() },
                { "tipo", _type.ToString() },
                { "status", ClientStatus.offline.ToString() },
                { "state", ClientState.Stand.ToString() } 
            }
        };

        FirebaseManager.instance.SetUser(_userId, userDataGlobal);
    }

    private void UpdateUsersConnected ()
    {
        FirebaseDatabase.DefaultInstance.GetReference("newUsersPending").SetValueAsync(playerData.userId);

        /*
        FirebaseDatabase.DefaultInstance
        .GetReference("usersConnectedCount")
        .GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError(task);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                int value = int.Parse(Convert.ToString(snapshot.Value));
                value++;
                FirebaseDatabase.DefaultInstance.GetReference("usersConnectedCount").SetValueAsync(value);
                FirebaseDatabase.DefaultInstance.GetReference("newUsersPending").SetValueAsync(playerData.userId);
            }
        });
        */
    }

    private void CreateMateHandler (UserData mateUserData, string mateId)
    {
        //var mateHandler = Instantiate();


        var mateHandler = Instantiate(mateHandlerPrefab);

        var mateData = ScriptableObject.CreateInstance<MateSO>();
        mateData.userData = mateUserData;
        mateData.userId = mateId;

        mateHandler.Initialize(mateData);

        FirebaseManager.instance.RegisterUserAttributeChangeValueEvent(mateData.userId, UserAttribute.waypoint,
            mateData.OnWaypointChangedValue);



        //playerData.OnChangeWaypoint += OnPlayerWaypointChanged;
        //playerHandler.OnClientStateChanged += OnClientStateChanged;
        //playerHandler.OnPlayerDeskChanged += OnPlayerDeskChanged;
        //FirebaseDatabase.DefaultInstance.GetReference("usersConnectedCount").ValueChanged += OnUserConnectedCountChanged;
        //FirebaseDatabase.DefaultInstance.GetReference("newUsersPending").ValueChanged += OnNewUsersPendingChanged;
        //playerHandler.OnPlayerGoalWaypointChanged += NewPlayerGoal;

    }

    private void OnNewUsersPendingChanged (object sender, ValueChangedEventArgs args)
    {
        string newUserConnectedId = args.Snapshot.Value.ToString();

        if (newUserConnectedId == "" || newUserConnectedId == playerData.userId)
        {
            return;
        }

        FirebaseManager.instance.GetUser(newUserConnectedId, (UserData newUserData) =>
        {
            CreateMateHandler(newUserData, newUserConnectedId);
        });
    }

    /*
    private IEnumerator GetDataFromUserPending ()
    {
        yield return new WaitUntil(() => pendingUserRead);

        pendingUserRead = false;
        UserData newUserData = JsonConvert.DeserializeObject<UserData>(pendingUserJson);

        Debug.LogWarning(newUserData.atributos["username"]);
    }

    private void OnUserConnectedCountChanged (object sender, ValueChangedEventArgs args)
    {
        var userConnectedCount = args.Snapshot.Value;
        Debug.Log(userConnectedCount);
    }
    */

    public void OnMateLogged (int userId)
    {
        MateSO mate = null;
        var obj = new GameObject();
        var mateHandler = obj.AddComponent<MateHandler>();

        mate = ScriptableObject.CreateInstance<MateSO>();
        mateHandler.Initialize((MateSO)mate);

        mate.OnChangeWaypoint += OnPlayerWaypointChanged;
        mate.OnChangeClientState += OnClientStateChanged;

        matesData.Add(mate);
    }

    public void OnPlayerWaypointChanged (int waypoint)
    {
        //FirebaseDatabase.DefaultInstance.GetReference("users/" + playerData.userId + "/atributos/waypoint").SetValueAsync(waypoint);
        FirebaseManager.instance.SetUserAttribute<int>(playerData.userId, UserAttribute.waypoint, waypoint);
        // send waypoint to firebase.
    }

    public void OnClientStateChanged (ClientState clientState)
    {
        switch (clientState)
        {
            case ClientState.Sit:
            clientState = ClientState.Stand;
            break;
            case ClientState.Stand:
            clientState = ClientState.Sit;
            break;
        }
        playerData.clientState = clientState;
        FirebaseManager.instance.SetUserAttribute<ClientState>(playerData.userId, UserAttribute.state, playerData.clientState);
        //FirebaseDatabase.DefaultInstance.GetReference("users/" + playerData.userId + "/atributos/state").SetValueAsync(clientState.ToString());
    }

    private void CreatePlayerHandler (string _userId)
    {
        playerHandler = Instantiate(playerHandlerPrefab);

        playerHandler.SetCamera(waypoints[0]);

        playerData = ScriptableObject.CreateInstance<PlayerSO>();
        playerData.userData = userDataGlobal;
        playerData.avatarId = 0;
        playerData.userId = _userId;

        playerHandler.Initialize((PlayerSO)playerData);

        playerData.OnChangeWaypoint += OnPlayerWaypointChanged;
        playerHandler.OnClientStateChanged += OnClientStateChanged;
        playerHandler.OnPlayerDeskChanged += OnPlayerDeskChanged;
        //FirebaseDatabase.DefaultInstance.GetReference("usersConnectedCount").ValueChanged += OnUserConnectedCountChanged;
        FirebaseDatabase.DefaultInstance.GetReference("newUsersPending").ValueChanged += OnNewUsersPendingChanged;
        playerHandler.OnPlayerGoalWaypointChanged += NewPlayerGoal;
    }

    private void NewPlayerGoal (WaypointHandler waypointHandler)
    {
        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < desks.Length; i++)
        {
            desks[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < waypointHandler.waypointsInRange.Length; i++)
        {
            waypoints[waypointHandler.waypointsInRange[i]].gameObject.SetActive(true);
        }

        for (int i = 0; i < waypointHandler.desksInRange.Length; i++)
        {
            desks[waypointHandler.desksInRange[i]].gameObject.SetActive(true);
        }

        OnPlayerWaypointChanged(waypointHandler.waypointIndex);
    }

    private void OnPlayerDeskChanged(DeskHandler deskHandler)
    {
        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < deskHandler.waypointsInRange.Length; i++)
        {
            waypoints[deskHandler.waypointsInRange[i]].gameObject.SetActive(true);
        }

        for (int i = 0; i < desks.Length; i++)
        {
            if (i != deskHandler.deskIndex)
            {
                desks[i].gameObject.SetActive(false);
            }
        }

        OnPlayerWaypointChanged(deskHandler.deskIndex);
    }
}

[Serializable]
public class UserData
{
    public Dictionary<string, object> atributos;
}

public enum UserAttribute
{
    genero, sala, tipo, waypoint, username, status, state, matricula
}

public enum GenderType
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