using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Google.MiniJSON;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Metadata;
using Unity.Mathematics;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Analytics;
using UnityEngine.XR.Interaction.Toolkit;

public class UserManager : MonoBehaviour
{
    public static UserManager instance;

    protected static WorldState CurrentWorldState;
    protected static int CurrentWorldStateArg;

    public PlayerHandler playerHandlerPrefab;
    public MateHandler mateHandlerPrefab;

    public GameObject XRGO;
    public GameObject XRSimulatorGO;
    public XRInteractionManager XRManager;
    public GameObject transformToTest;

    public List<ButtonHandler> buttons;

    public List<GameObject> floorWaypointHolder;
    public List<GameObject> deskWaypointHolder;
    public List<GameObject> doorWaypointHolder;

    public RawImage boardImage;
    public List<Texture> classImages;
    private int classImagesIndex;

    private int[] waypointsRoomOrigins = new int[3];

    public PlayerHandler playerHandler;
    private List<MateHandler> mateHandlers;

    private WaypointsData waypointsData;
    public List<WaypointHandler> waypoints;

    private int currentUsersConnectedCount = 0;
    private bool isPlayerLogged = false;

    private bool registeredToWaypointChanged = false;
    private bool registeredToLoggedFlagChanged = false;

    private bool checkingClientLogged = false;
    private bool hasClientLogged = false;


    private void Awake()
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

    private void Start()
    {
        SetXRButton();
        LoadWaypointHandlers();
        CreatePlayerHandler();

        FirebaseManager.instance.OnLoginSuccess += OnLogin;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            XRSimulatorGO.SetActive(!XRSimulatorGO.activeSelf);
        }

        if (!isPlayerLogged)
        {
            return;
        }

        UpdateMatesLabel();
        /*
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            var message = "Olá a todos";
            SendClientMessage(playerHandler.UserId, message);
        }
        */
 

        //Debug.LogWarning("RoomId: " + roomId);

        //if (playerHandler.RuntimeData.roomId != null) roomId = int.Parse(playerHandler.RuntimeData.roomId);
    }

    private void OnDestroy()
    {
        if (registeredToLoggedFlagChanged)
        {
            //FirebaseManager.instance.UnregisterUsersLoggedFlagChangeValueEvent(OnLoggedFlagChanged);
        }
    }

    private void SetXRButton()
    {
        foreach (var button in buttons)
        {
            button.SetXRManager(XRManager);
            button.OnButtonClicked = OnButtonClicked;
        }
    }

    private void UpdateButton()
    {
        foreach (var button in buttons)
        {
            if (playerHandler.RegisterData.tipo == "aluno" && !button.waypointStudentAccess) button.gameObject.SetActive(false); 
        }
    }

    private void LoadWaypointHandlers()
    {
        int waypointIdx = 0;

        for (int i = 0; i < floorWaypointHolder.Count; i++)
        {
            var floorArray = floorWaypointHolder[i].GetComponentsInChildren<WaypointHandler>();
            waypoints.AddRange(floorArray.ToList());

            for (int j = 0; j < floorArray.Length; j++, waypointIdx++)
            {
                if (j == 0) waypointsRoomOrigins[i] = waypointIdx;

                waypoints[waypointIdx] = floorArray[j];
                waypoints[waypointIdx].WaypointIndex = waypointIdx;
                waypoints[waypointIdx].SetXRManager(XRManager);
                waypoints[waypointIdx].OnWaypointSelected = OnPlayerWaypointClicked;
            }
        }

        for (int i = 0; i < deskWaypointHolder.Count; i++)
        {
            var deskArray = deskWaypointHolder[i].GetComponentsInChildren<WaypointHandler>();
            waypoints.AddRange(deskArray.ToList());

            for (int j = 0; j < deskArray.Length; j++, waypointIdx++)
            {
                waypoints[waypointIdx] = deskArray[j];
                waypoints[waypointIdx].WaypointIndex = waypointIdx;
                waypoints[waypointIdx].SetXRManager(XRManager);
                waypoints[waypointIdx].OnWaypointSelected = OnPlayerWaypointClicked;
            }
        }

        for (int i = 0; i < doorWaypointHolder.Count; i++)
        {
            var doorArray = doorWaypointHolder[i].GetComponentsInChildren<WaypointHandler>();
            waypoints.AddRange(doorArray.ToList());

            for (int j = 0; j < doorArray.Length; j++, waypointIdx++)
            {
                waypoints[waypointIdx] = doorArray[j];
                waypoints[waypointIdx].WaypointIndex = waypointIdx;
                waypoints[waypointIdx].SetXRManager(XRManager);
                waypoints[waypointIdx].OnWaypointSelected = OnPlayerWaypointClicked;
            }
        }

        //Debug.Log("WaypointCount: " + waypointsCount);
    }

    private void CreatePlayerHandler()
    {
        playerHandler = Instantiate(playerHandlerPrefab);
        playerHandler.SetXRGO(XRGO);
        playerHandler.SetPosition(waypoints[0].transform.position);
        //playerHandler.SetPosition(transformToTest.transform.position);
    }

    private void OnLogin(string _userId)
    {
        Debug.Log("Login success: " + _userId);

        playerHandler.SetUserId(_userId);

        checkingClientLogged = true;

        //FirebaseManager.instance.RegisterUsersLoggedFlagChangeValueEvent(OnLoggedFlagChanged);

        registeredToLoggedFlagChanged = true;

        isPlayerLogged = true;

        // set the player runtime data to the database, it will continue in OnPlayerUserRuntimeDataWrite
        FirebaseManager.instance.SetUserRuntimeData(_userId, new UserRuntimeData(0, 0, ClientState.Idle, ""), OnPlayerUserRuntimeDataWrite);

        //StartCoroutine(PostLoginWaitForLoggedFlag(_userId));
    }

    private IEnumerator PostLoginWaitForLoggedFlag(string _userId)
    {
        /*
        // set to true so we can force the value to change when set to false after 0.5f
        FirebaseManager.instance.SetUsersLoggedFlag(true);

        yield return new WaitForSeconds(0.5f);

        hasClientLogged = false;

        FirebaseManager.instance.SetUsersLoggedFlag(false);

        //yield return new WaitUntilForSeconds(() => hasClientLogged, 2f);
        yield return new WaitForSeconds(2f);

        checkingClientLogged = false;

        // it needs to clear the users connected data (no client responds to the flag)
        if (!hasClientLogged)
        {
            Debug.Log("IT NEEDS TO CLEAR!!!");
        }
        */

        yield return null;

        // set the player runtime data to the database, it will continue in OnPlayerUserRuntimeDataWrite
        //FirebaseManager.instance.SetUserRuntimeData(_userId, new UserRuntimeData(0, 0, ClientState.Idle, ""), OnPlayerUserRuntimeDataWrite);
    }

    private void OnPlayerUserRuntimeDataWrite(string _userId)
    {
        // get all users connected
        StartCoroutine(FirebaseManager.instance.GetAllUsersRuntimeData(OnAllUsersRuntimeDataRead));
    }

    private void OnAllUsersRuntimeDataRead(Dictionary<string, UserRuntimeData> usersDictionary)
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
                    var playerRuntimeDataStart = new UserRuntimeData(0, 0, ClientState.Idle, "");
                    UpdatePlayerHandler(clientUserId, playerRuntimeDataStart);
                }
            }
        }

        currentUsersConnectedCount = usersDictionary.Count;

        FirebaseManager.instance.SetUsersConnectedCount(currentUsersConnectedCount);

        StartCoroutine(StartTrackingMateLogins());
    }

    private IEnumerator StartTrackingMateLogins()
    {
        yield return new WaitForSeconds(5f);
        FirebaseManager.instance.RegisterUsersConnectedCountChangeValueEvent(OnConnectedCountChanged);
    }

    public void OnConnectedCountChanged(object sender, ValueChangedEventArgs args)
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

    /*public void OnLoggedFlagChanged(object sender, ValueChangedEventArgs args)
    {
        bool usersLoggedFlag = bool.Parse(args.Snapshot.Value.ToString());

        if (!usersLoggedFlag)
        {
            // to not respond to itself when is checking the a client logged
            if (checkingClientLogged)
            {
                return;
            }

            // respond to the just logged client that we're already logged
            FirebaseManager.instance.SetUsersLoggedFlag(true);
        }
        else
        {
            if (checkingClientLogged)
            {
                hasClientLogged = true;
            }
        }
    }*/

    private void UpdatePlayerHandler(string _userId, UserRuntimeData _userRuntimeData)
    {
        playerHandler.SetUserRuntimeData(_userRuntimeData);

        // it will continue the creation OnPlayerRegisterDataRead
        StartCoroutine(FirebaseManager.instance.GetUserRegisterData(_userId, OnPlayerRegisterDataRead));
    }

    private void OnPlayerRegisterDataRead(string userId, UserRegisterData userRegisterData)
    {
        playerHandler.SetUserRegisterData(userRegisterData);
        playerHandler.SetPosition(waypoints[0].transform.position);
        //playerHandler.SetCamera(true, true);
        //playerHandler.OnWaypointClicked = OnPlayerWaypointClicked;
        playerHandler.OnRoomChange = OnRoomChange;
        //playerHandler.OnButtonClicked = OnButtonClicked;
        playerHandler.InitializeClient();
        UpdateButton();
    }

    private void SetWorldState(WorldState worldState)
    {
        CurrentWorldState = worldState;
        if(worldState == WorldState.InClass) boardImage.gameObject.SetActive(true);
    }

    private void SetWorldStateArg(int worldStateArg)
    {
        CurrentWorldStateArg = worldStateArg;
        boardImage.texture = classImages[classImagesIndex];
    }

    private void OnButtonClicked(ButtonType type)
    {
        switch (type)
        {
            case ButtonType.Next:
                ProfessorNextClick();
                break;
            case ButtonType.Start:
                ProfessorStartClass();
                break;
            case ButtonType.Previous:
                ProfessorPreviousClick();
                break;
            default:
                break;
        }
    }

    private void ProfessorStartClass()
    {
        FirebaseManager.instance.SetWorldState(WorldState.InClass);
        SetWorldState(WorldState.InClass);
    }

    private void ProfessorNextClick()
    {
        if(classImagesIndex >= 0 && classImagesIndex < classImages.Count - 1)
        {
            FirebaseManager.instance.SetWorldStateArg(classImagesIndex++);
            SetWorldStateArg(classImagesIndex++);
        }
    }

    private void ProfessorPreviousClick()
    {
        if(classImagesIndex > 0) 
        {
            FirebaseManager.instance.SetWorldStateArg(classImagesIndex--);
            SetWorldStateArg(classImagesIndex--);
        }
    }

    private void OnWorldStateChanged(object sender, ValueChangedEventArgs args)
    {
        var message = args.Snapshot.Value.ToString();
        WorldState state = (WorldState)Enum.Parse(typeof(WorldState), message);
        SetWorldState(state);
    }

    private void OnWorldStateArgChanged(object sender, ValueChangedEventArgs args)
    {
        var message = args.Snapshot.Value.ToString();
        int stateArg = int.Parse(message);
        SetWorldStateArg(stateArg);
    }

    private void OnPlayerWaypointClicked (WaypointHandler waypointHandler)
    {
        if (!isPlayerLogged)
        {
            return;
        }

        FirebaseManager.instance.SetUserRuntimeAttribute(playerHandler.UserId, UserRuntimeAttribute.waypoint, waypointHandler.WaypointIndex);
        OnClientWaypointChanged(playerHandler.UserId, waypointHandler.WaypointIndex);
    }

    private void OnRoomChange(int _roomId)
    {
        Debug.Log(_roomId);
        //Debug.Log("Chamou");
        playerHandler.SetNewRoomLocation(waypoints[waypointsRoomOrigins[_roomId]]);
    }

    private void CreateMateHandler(string _userId, UserRuntimeData _userRuntimeData)
    {
        var mateHandler = Instantiate(mateHandlerPrefab);
        mateHandler.SetUserId(_userId);
        mateHandler.SetUserRuntimeData(_userRuntimeData);
        mateHandlers.Add(mateHandler);

        // it will continue the creation OnMateRegisterDataRead
        StartCoroutine(FirebaseManager.instance.GetUserRegisterData(_userId, OnMateRegisterDataRead));
    }

    private void OnMateRegisterDataRead(string userId, UserRegisterData userRegisterData)
    {
        var mateHandler = GetMateHandler(userId);
        if (mateHandler != null)
        {
            mateHandler.SetUserRegisterData(userRegisterData);
            var waypointIdx = int.Parse(mateHandler.RuntimeData.waypoint);
            mateHandler.ChangeModel();
            mateHandler.SetPosition(waypoints[waypointIdx].transform.position);
            mateHandler.OnMateWaypointChanged = OnClientWaypointChanged;
            mateHandler.OnMateMessageChanged = OnClientMessageChanged;
            mateHandler.InitializeClient();
            FirebaseManager.instance.
                RegisterUserRuntimeAttributeChangeValueEvent(userId, UserRuntimeAttribute.waypoint,
                mateHandler.OnMateWaypointValueChanged);
            FirebaseManager.instance.
                RegisterUserRuntimeAttributeChangeValueEvent(userId, UserRuntimeAttribute.message,
                mateHandler.OnMateMessageValueChanged);
            FirebaseManager.instance.
                RegisterWorldStateChangeValueEvent(OnWorldStateChanged);
            FirebaseManager.instance.
                RegisterWorldStateArgChangeValueEvent(OnWorldStateArgChanged);
        }
    }

    private MateHandler GetMateHandler(string userId)
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

    private void OnClientWaypointChanged(string userId, int waypointValue)
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

    private void UpdateMatesLabel()
    {
        var playerPosition = playerHandler.transform.position;

        for (int i = 0; i < mateHandlers.Count; i++)
        {
            mateHandlers[i].UpdateMateLabel(playerPosition);
        }
    }

    private void OnClientMessageChanged (string userId, string message)
    {
        if (userId == "")
        {
            return;
        }

        if (userId == playerHandler.UserId)
        {
            return;
        }

        var mateHandler = GetMateHandler(userId);

        if (mateHandler == null)
        {
            Debug.LogError("not found mate");
            return;
        }

        Debug.Log(mateHandler.RegisterData.username + ": " + message);
    }

    private void SendClientMessage(string userId, string message)
    {
        FirebaseManager.instance.SetUserRuntimeAttribute(userId, UserRuntimeAttribute.message, message);
    }
}