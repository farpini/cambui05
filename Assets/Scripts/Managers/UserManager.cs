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

    public GameObject prefabAvatar1;
    public GameObject prefabAvatar2;
    public PlayerHandler playerHandlerPrefab;

    public PlayerHandler playerHandler;

    public WaypointHandler[] waypoints;

    public List<int[]> waypointsInRangeList;

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
            new int[] { 1, 2, 5 }, // 0
            new int[] { 0, 2, 3 }, // 1
            new int[] { 0, 1, 4 }, // 2
            new int[] { 1 }, // 3
            new int[] { 2 }, // 4
            new int[] { 0 } // 5
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

        // notify other clients that this user connected
        UpdateUsersConnected();

        // create a player handler for this client
        CreatePlayerHandler(_userId);
    }

    private void GetUserData (string userId)
    {
        FirebaseManager.instance.GetUser(userId, (userDataGlobal) =>
        {
            Debug.Log("Player data loaded from the database successfuly!");
        });
    }

    private void SetNewUserData (string _userId, string _userName, GenderType _gender, ClientType _type)
    {
        userDataGlobal = new UserData
        {
            atributos = new Dictionary<string, object>() 
            {
                { "username", _userName},
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

    private void OnNewUsersPendingChanged (object sender, ValueChangedEventArgs args)
    {
        /*
        var newUsersPeding = args.Snapshot.Value.ToString();

        if (newUsersPeding != playerData.userId && newUsersPeding != "")
        {
            pendingUserRead = false;

            StartCoroutine(GetDataFromUserPending());

            FirebaseDatabase.DefaultInstance.GetReference("users/" + newUsersPeding)
                .GetValueAsync()
                .ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Debug.LogError(task);
                    }
                    else if (task.IsCompleted)
                    {
                        DataSnapshot snapshot = task.Result;
                        pendingUserJson = snapshot.GetRawJsonValue();
                        pendingUserRead = true;
                    }
                });
        }
        */
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
        mate.onChangeClientState += OnClientStateChanged;

        matesData.Add(mate);
    }

    public void OnPlayerWaypointChanged (int waypoint)
    {
        FirebaseDatabase.DefaultInstance.GetReference("users/" + playerData.userId + "/atributos/waypoint").SetValueAsync(waypoint);
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
        FirebaseDatabase.DefaultInstance.GetReference("teste/" + playerData.userId + "/atributos/state").SetValueAsync(clientState.ToString());
    }

    private void CreatePlayerHandler (string _userId)
    {
        playerHandler = Instantiate(playerHandlerPrefab);

        playerHandler.SetCamera(waypoints[0]);

        playerData = ScriptableObject.CreateInstance<PlayerSO>();
        playerData.avatarId = 0;
        playerData.userId = _userId;

        playerHandler.Initialize((PlayerSO)playerData);

        playerData.OnChangeWaypoint += OnPlayerWaypointChanged;
        playerData.onChangeClientState += OnClientStateChanged;
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

        for (int i = 0; i < waypointHandler.waypointsInRange.Length; i++)
        {
            waypoints[waypointHandler.waypointsInRange[i]].gameObject.SetActive(true);
        }

        OnPlayerWaypointChanged(waypointHandler.waypointIndex);
    }
}

[Serializable]
public class UserData
{
    public Dictionary<string, object> atributos;
}

public enum UserAttribute
{
    genero, sala, tipo, waypoint, username, status, state
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