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

    public GameObject floorWaypointHolder;
    public GameObject deskWaypointHolder;

    public PlayerHandler playerHandler;
    private List<MateHandler> mateHandlers;

    private WaypointsData waypointsData;
    private WaypointHandler[] waypoints;

    private int currentUsersConnectedCount = 0;
    

    private void Awake ()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("Cannot have two UsersManager");
            return;
        }

        mateHandlers = new();
    }

    private void Start ()
    {
        LoadWaypointHandlers();
        CreatePlayerHandler();

        FirebaseManager.instance.OnLoginSuccess += OnLogin;
    }

    private void Update ()
    {
        UpdateMatesLabel();
    }

    private void LoadWaypointHandlers ()
    {
        var floorWaypoints = floorWaypointHolder.GetComponentsInChildren<WaypointHandler>();
        var deskWaypoints = deskWaypointHolder.GetComponentsInChildren<WaypointHandler>();
        var waypointsCount = floorWaypoints.Length + deskWaypoints.Length;

        waypoints = new WaypointHandler[waypointsCount];

        int waypointIdx = 0;

        for (int i = 0; i < floorWaypoints.Length; i++, waypointIdx++)
        {
            waypoints[waypointIdx] = floorWaypoints[i];
            waypoints[waypointIdx].WaypointIndex = waypointIdx;
        }

        for (int i = 0; i < deskWaypoints.Length; i++, waypointIdx++)
        {
            waypoints[waypointIdx] = deskWaypoints[i];
            waypoints[waypointIdx].WaypointIndex = waypointIdx;
        }

        //Debug.Log("WaypointCount: " + waypointsCount);
    }

    private void CreatePlayerHandler ()
    {
        playerHandler = Instantiate(playerHandlerPrefab);
    }

    private void OnLogin (string _userId)
    {
        Debug.Log("Login success: " + _userId);

        playerHandler.SetUserId(_userId);

        // set the player runtime data to the database, it will continue in OnPlayerUserRuntimeDataWrite
        FirebaseManager.instance.SetUserRuntimeData(_userId, new UserRuntimeData(0, 0, ClientState.Idle), OnPlayerUserRuntimeDataWrite);
    }

    private void OnPlayerUserRuntimeDataWrite (string _userId)
    {
        // get all users connected
        StartCoroutine(FirebaseManager.instance.GetAllUsersRuntimeData(OnAllUsersRuntimeDataRead));
    }

    private void OnAllUsersRuntimeDataRead (Dictionary<string, UserRuntimeData> usersDictionary)
    {
        Debug.LogWarning("OnAllUsersRuntimeDataRead EXECUTED");

        foreach (var client in usersDictionary)
        {
            var clientUserId = client.Key;

            if (clientUserId == "")
            {
                Debug.LogError("ClientUserId is null.");
                continue;
            }

            if (clientUserId != playerHandler.UserId)
            {
                // only create a mate if it does not exist
                if (GetMateHandler(clientUserId) == null)
                {
                    CreateMateHandler(clientUserId, client.Value);
                }
            }
            else
            {
                if (!playerHandler.IsClientInitialized)
                {
                    var playerRuntimeDataStart = new UserRuntimeData(0, 0, ClientState.Idle);
                    UpdatePlayerHandler(clientUserId, playerRuntimeDataStart);
                }
            }
        }

        currentUsersConnectedCount = usersDictionary.Count;

        FirebaseManager.instance.SetUsersConnectedCount(currentUsersConnectedCount);

        StartCoroutine(StartTrackingMateLogins());
    }

    private IEnumerator StartTrackingMateLogins ()
    {
        yield return new WaitForSeconds(5f);
        FirebaseManager.instance.RegisterUsersConnectedCountChangeValueEvent(OnConnectedCountChanged);
    }

    public void OnConnectedCountChanged (object sender, ValueChangedEventArgs args)
    {
        FirebaseManager.instance.UnregisterUsersConnectedCountChangeValueEvent(OnConnectedCountChanged);
        int usersConnectedCountRead = int.Parse(args.Snapshot.Value.ToString());

        if (usersConnectedCountRead != currentUsersConnectedCount)
        {
            Debug.Log("UsersConnectedCountChanged from: " + currentUsersConnectedCount + " to " + usersConnectedCountRead);
            currentUsersConnectedCount = usersConnectedCountRead;
            StartCoroutine(FirebaseManager.instance.GetAllUsersRuntimeData(OnAllUsersRuntimeDataRead));
        }
    }

    private void UpdatePlayerHandler (string _userId, UserRuntimeData _userRuntimeData)
    {
        playerHandler.SetUserRuntimeData(_userRuntimeData);

        // it will continue the creation OnPlayerRegisterDataRead
        StartCoroutine(FirebaseManager.instance.GetUserRegisterData(_userId, OnPlayerRegisterDataRead));
    }

    private void OnPlayerRegisterDataRead (string userId, UserRegisterData userRegisterData)
    {
        playerHandler.SetUserRegisterData(userRegisterData);
        playerHandler.ChangeModel();
        playerHandler.SetPosition(waypoints[0].transform.position);
        playerHandler.SetCamera(true, true);
        playerHandler.OnWaypointClicked = OnPlayerWaypointClicked;
        playerHandler.InitializeClient();
    }

    private void OnPlayerWaypointClicked (WaypointHandler waypointHandler)
    {
        FirebaseManager.instance.SetUserRuntimeAttribute(playerHandler.UserId, UserRuntimeAttribute.waypoint, waypointHandler.WaypointIndex);
        OnClientWaypointChanged(playerHandler.UserId, waypointHandler.WaypointIndex);
    }

    private void CreateMateHandler (string _userId, UserRuntimeData _userRuntimeData)
    {
        var mateHandler = Instantiate(mateHandlerPrefab);
        mateHandler.SetUserId(_userId);
        mateHandler.SetUserRuntimeData(_userRuntimeData);
        mateHandlers.Add(mateHandler);

        // it will continue the creation OnMateRegisterDataRead
        StartCoroutine(FirebaseManager.instance.GetUserRegisterData(_userId, OnMateRegisterDataRead));
    }

    private void OnMateRegisterDataRead (string userId, UserRegisterData userRegisterData)
    {
        var mateHandler = GetMateHandler(userId);
        if (mateHandler != null)
        {
            mateHandler.SetUserRegisterData(userRegisterData);
            var waypointIdx = int.Parse(mateHandler.RuntimeData.waypoint);
            mateHandler.ChangeModel();
            mateHandler.SetPosition(waypoints[waypointIdx].transform.position);
            mateHandler.OnMateWaypointChanged = OnClientWaypointChanged;
            mateHandler.InitializeClient();
            FirebaseManager.instance.
                RegisterUserRuntimeAttributeChangeValueEvent(userId, UserRuntimeAttribute.waypoint, 
                mateHandler.OnMateWaypointValueChanged);
        }
    }

    private MateHandler GetMateHandler (string userId)
    {
        for (int i = 0; i < mateHandlers.Count; i++)
        {
            if (mateHandlers[i].UserId == userId)
            {
                return mateHandlers[i];
            }
        }

        return null;
    }

    private void OnClientWaypointChanged (string userId, int waypointValue)
    {
        if (userId == "")
        {
            return;
        }

        if (userId == playerHandler.UserId)
        {
            playerHandler.SetNewWaypoint(waypoints[waypointValue]);
        }
        else
        {
            var mateHandler = GetMateHandler(userId);
            if (mateHandler != null)
            {
                mateHandler.SetNewWaypoint(waypoints[waypointValue]);
            }
        }
    }

    private void UpdateMatesLabel ()
    {
        var playerPosition = playerHandler.transform.position;

        for (int i = 0; i < mateHandlers.Count; i++)
        {
            mateHandlers[i].UpdateMateLabel(playerPosition);
        }
    }
}