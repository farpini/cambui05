using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class UserManager : MonoBehaviour
{
    public static UserManager instance;

    protected static WorldState CurrentWorldState;
    protected static int CurrentWorldStateArg;
    protected static Dictionary<string, StateData[]> WorldStateDict;

    protected static WorldSettings WorldSettings = null;

    public PlayerHandler playerHandlerPrefab;
    public MateHandler mateHandlerPrefab;

    public GameObject XRGO;
    public GameObject XRCameraOffsetGO;
    public GameObject XRSimulatorGO;
    public XRInteractionManager XRManager;
    public WaypointHandler waypointToDropEPI;
    public Transform dropTransform;
    public FireHandler fireHandler;
    public ExtinguisherHandler extinguisherHandler;

    public TMP_Text canvasClassRoomText;
    public TMP_Text canvasPracticeRoomText;

    public List<ButtonHandler> buttons;

    public List<GameObject> floorWaypointHolder;
    public List<GameObject> deskWaypointHolder;
    public List<GameObject> doorWaypointHolder;

    public ObjectHandler[] objectHandlers;

    public RawImage boardImage;
    public List<Texture> classImages;
    private int classImagesIndex;

    private Dictionary<string, StudentQuizData> studentQuizData;
    private Dictionary<string, int> studentFireStateDict;

    public int[] waypointsRoomOrigins = new int[3];

    public PlayerHandler playerHandler;
    private List<MateHandler> mateHandlers;

    private WaypointsData waypointsData;
    public List<WaypointHandler> waypoints;

    private int quizIndex = 0;

    //private int currentUsersConnectedCount = 0;
    private bool isPlayerLogged = false;
    

    private bool registeredToWaypointChanged = false;
    private bool registeredToLoggedFlagChanged = false;

    private bool checkingClientLogged = false;
    private bool hasClientLogged = false;

    private bool firstUpdate = false;

    private bool fireAccidentDone;

    public Action<WorldState, StateData> OnWorldStateDataChanged;
    public Action<string, string> OnScoreChanged;
    public Action<List<string>> OnDestinationMsgUsernamesChanged;
    public Action<string> OnReceivedMessage;


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

        fireHandler.DeactivateFire();

        mateHandlers = new();
        studentQuizData = new();
        studentFireStateDict = new();

        fireAccidentDone = false;

        WorldStateDict = new();

        ProfessorInstructionsData.Initialize(classImages.Count);

        FirebaseManager.instance.OnFirebaseInitialized += OnFirebaseInitialized;
        FirebaseManager.instance.OnLoginSuccess += OnLogin;
    }

    private void Start ()
    {
        SetXRButton();
        LoadWaypointHandlers();
        InitializeObjectHandlers();
        InitializeExtinguisher();
    }

    private void Update()
    {
        if (WorldSettings == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            XRSimulatorGO.SetActive(!XRSimulatorGO.activeSelf);
        }

        if (!isPlayerLogged)
        {
            return;
        }

        UpdateMatesLabel();
    }

    private void OnDestroy()
    {
        if (registeredToLoggedFlagChanged)
        {
            //FirebaseManager.instance.UnregisterUsersLoggedFlagChangeValueEvent(OnLoggedFlagChanged);
        }
    }

    private void InitializeExtinguisher ()
    {
        //fireHandler.ActivateFire();
        extinguisherHandler.Initialize();
        //extinguisherHandler.OnFireExtinguisherStateChanged += OnFireExtinguished;
    }

    private void OnFirebaseInitialized ()
    {
        StartCoroutine(FirebaseManager.instance.GetWorldSettingsData(OnWorldSettingsRead));
        FirebaseManager.instance.OnFirebaseInitialized -= OnFirebaseInitialized;
    }

    private void OnWorldSettingsRead (WorldSettings worldSettings)
    {
        WorldSettings = worldSettings;
        CreatePlayerHandler();
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
            if (!playerHandler.RegisterData.IsProfessor || !button.waypointProfessorAccess) button.gameObject.SetActive(false); 
        }
    }

    private void InitializeObjectHandlers()
    {
        if (objectHandlers == null)
        {
            return;
        }

        for (int i = 0; i < objectHandlers.Length; i++)
        {
            objectHandlers[i].OnObjectDropped = OnObjectDropped;
            objectHandlers[i].OnObjectPicked = OnObjectPicked;
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
        playerHandler.SetXRGO(XRGO, XRCameraOffsetGO, extinguisherHandler);
        playerHandler.SetPosition(waypoints[0].transform.position);
        playerHandler.SetMovementSpeed(WorldSettings.characterSpeed);
        playerHandler.OnClientWaypointReached += OnPlayerWaypointReached;
        playerHandler.OnFireStateChanged += OnPlayerFireStateChanged;

        FirebaseManager.instance.RegisterCharacterMovementSpeedChangeValueEvent(OnCharacterMovementSpeedChanged);
    }

    private void OnLogin(string _userId)
    {
        FirebaseManager.instance.OnLoginSuccess -= OnLogin;

        Debug.Log("Login success: " + _userId);

        playerHandler.SetUserId(_userId);

        checkingClientLogged = true;

        //FirebaseManager.instance.RegisterUsersLoggedFlagChangeValueEvent(OnLoggedFlagChanged);

        registeredToLoggedFlagChanged = true;

        isPlayerLogged = true;

        // set the player runtime data to the database, it will continue in OnPlayerUserRuntimeDataWrite
        FirebaseManager.instance.SetUserRuntimeData(_userId, new UserRuntimeData(0, 0, ClientState.Idle, "", -1, 0), OnPlayerUserRuntimeDataWrite);

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
        // get world settings
        StartCoroutine(FirebaseManager.instance.GetAllWorldStateData(OnWorldSettingLoaded));
    }

    private void OnWorldSettingLoaded (Dictionary<string, StateData[]> _worldStateData)
    {
        foreach (var door in doorWaypointHolder)
        {
            door.gameObject.SetActive(true);
        }

        if (_worldStateData != null)
        {
            WorldStateDict = _worldStateData;

            PrintWorldStateMessage();
            /*
            foreach (var worldState in _worldStateData)
            {
                Debug.Log("WorldState: " + worldState.Key.ToString());
                foreach (var stateData in worldState.Value)
                {
                    Debug.Log(": " + stateData.stateMsg.ToString());
                }
            }

            Debug.Log(_worldStateData.Count);
            */
        }
        else
        {
            Debug.LogError("ITS NULL");
        }

        // start tracking users
        StartCoroutine(StartTrackingMateLogins());

        //StartCoroutine(FirebaseManager.instance.GetAllUsersRuntimeData(OnAllUsersRuntimeDataRead));
    }

    private void OnAllUsersRuntimeDataRead(Dictionary<string, UserRuntimeData> usersDictionary)
    {
        //Debug.LogWarning("OnAllUsersRuntimeDataRead EXECUTED");

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
                var mateHandler = GetMateHandler(clientUserId);
                if (mateHandler == null)
                {
                    CreateMateHandler(clientUserId, client.Value);
                }
                else
                {
                    if (!firstUpdate)
                    {
                        mateHandler.UpdateFirstTime();
                        firstUpdate = true;
                    }
                }
            }
            else
            {
                if (!playerHandler.IsClientInitialized)
                {
                    var playerRuntimeDataStart = new UserRuntimeData(0, 0, ClientState.Idle, "", -1, 0);
                    UpdatePlayerHandler(clientUserId, playerRuntimeDataStart);
                }
            }
        }

        UpdateProfessorInstructionText();

        //currentUsersConnectedCount = usersDictionary.Count;

        //FirebaseManager.instance.SetUsersConnectedCount(currentUsersConnectedCount);

        //StartCoroutine(StartTrackingMateLogins());
    }

    private void OnWorldStateDataRead (Dictionary<string, StateData[]> _worldStateData)
    {
        if (_worldStateData != null)
        {
            WorldStateDict = _worldStateData;

            PrintWorldStateMessage();
            /*
            foreach (var worldState in _worldStateData)
            {
                Debug.Log("WorldState: " + worldState.Key.ToString());
                foreach (var stateData in worldState.Value)
                {
                    Debug.Log(": " + stateData.stateMsg.ToString());
                }
            }

            Debug.Log(_worldStateData.Count);
            */
        }
        else
        {
            Debug.LogError("ITS NULL");
        }
    }

    private IEnumerator StartTrackingMateLogins()
    {
        while (true)
        {
            StartCoroutine(FirebaseManager.instance.GetAllUsersRuntimeData(OnAllUsersRuntimeDataRead));
            yield return new WaitForSeconds(5f);
        }

        //FirebaseManager.instance.RegisterUsersConnectedCountChangeValueEvent(OnConnectedCountChanged);
    }

    /*
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
    */

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
        if (userRegisterData.IsProfessor)
        {
            canvasClassRoomText.gameObject.SetActive(true);
            canvasPracticeRoomText.gameObject.SetActive(false);
            FirebaseManager.instance.SetWorldState(WorldState.WaitingOnClassRoom);
            FirebaseManager.instance.SetWorldStateArg(0);
        }
        else
        {
            canvasClassRoomText.gameObject.SetActive(false);
            canvasPracticeRoomText.gameObject.SetActive(false);
            studentQuizData.Add(playerHandler.UserId, new StudentQuizData { epiIds = new string[WorldSettings.quizAnswerKeys.Length] });
            FirebaseManager.instance.RegisterQuizResultTextChangeValueEvent(OnQuizResultTextChanged);
        }

        playerHandler.SetUserRegisterData(userRegisterData);
        playerHandler.SetPosition(waypoints[0].transform.position);
        playerHandler.OnClientMessageChanged = OnClientMessageChanged;
        playerHandler.InitializeClient();
        FirebaseManager.instance.
            RegisterUserRuntimeAttributeChangeValueEvent(userId, UserRuntimeAttribute.message,
            playerHandler.OnClientMessageValueChanged);
        
        UpdateButton();
    }

    private void SetWorldState(WorldState worldState)
    {
        CurrentWorldStateArg = 0;
        CurrentWorldState = worldState;
        if (worldState == WorldState.ClassStarted)
        {
            boardImage.gameObject.SetActive(true);
        }
        if (worldState == WorldState.FireAccident)
        {
            fireHandler.ActivateFire();
        }
        else
        {
            fireHandler.DeactivateFire();
        }
        PrintWorldStateMessage();
    }

    private void SetWorldStateArg(int worldStateArg)
    {
        CurrentWorldStateArg = worldStateArg;
        if (CurrentWorldState == WorldState.ClassStarted)
        {
            classImagesIndex = CurrentWorldStateArg;
            boardImage.texture = classImages[classImagesIndex];
        }
        if (CurrentWorldState == WorldState.FireAccident && playerHandler.RegisterData.IsProfessor)
        {
            PrintWorldStateMessageDirectly(new StateData { stateMsg = "Aguardando alunos apagarem o fogo.", stateMsgDuration = 0, stateMsgToShow = true });
        }
        else
        {
            PrintWorldStateMessage();
        }

        UpdateProfessorInstructionText();
    }

    private void UpdateProfessorInstructionText ()
    {
        if (playerHandler != null && playerHandler.RegisterData != null && playerHandler.RegisterData.IsProfessor)
        {
            string instructionText = ProfessorInstructionsData.GetInstructionText(CurrentWorldState, CurrentWorldStateArg);
            canvasClassRoomText.text = instructionText;
            canvasPracticeRoomText.text = instructionText;
        }
    }

    private void OnButtonClicked(ButtonType type, int classId)
    {
        if (CurrentWorldState == WorldState.WaitingOnClassRoom && classId == 0)
        {
            switch (type)
            {
                case ButtonType.Next:
                break;
                case ButtonType.Start:
                ProfessorStartClass();
                break;
                case ButtonType.Previous:
                break;
                default:
                break;
            }
        }
        else if (CurrentWorldState == WorldState.ClassStarted && classId == 0)
        {
            switch (type)
            {
                case ButtonType.Next:
                ProfessorNextClassScreenClick();
                break;
                case ButtonType.Start:
                ProfessorEndClass();
                break;
                case ButtonType.Previous:
                ProfessorPreviousClassScreenClick();
                break;
                default:
                break;
            }
        }
        else if (CurrentWorldState == WorldState.WaitingOnPracticeRoom && classId == 1)
        {
            switch (type)
            {
                case ButtonType.Next:
                break;
                case ButtonType.Start:
                ProfessorStartPractice();
                break;
                case ButtonType.Previous:
                break;
                default:
                break;
            }
        }
        else if (CurrentWorldState == WorldState.PracticeStarted && classId == 1)
        {
            switch (type)
            {
                case ButtonType.Next:
                break;
                case ButtonType.Start:
                ProfessorStartFireAccident();
                break;
                case ButtonType.Previous:
                break;
                default:
                break;
            }
        }
        else if (CurrentWorldState == WorldState.FireAccident && classId == 1 && fireAccidentDone)
        {
            switch (type)
            {
                case ButtonType.Next:
                case ButtonType.Start:
                ProfessorStartQuiz();
                break;
                default:
                break;
            }
        }
        else if (CurrentWorldState == WorldState.QuizStarted && classId == 1)
        {
            switch (type)
            {
                case ButtonType.Next:
                ProfessorQuizQuestionClicked();
                break;
                case ButtonType.Start:
                break;
                case ButtonType.Previous:
                break;
                default:
                break;
            }
        }
    }

    private void ProfessorStartClass()
    {
        FirebaseManager.instance.SetWorldState(WorldState.ClassStarted);
        SetWorldState(WorldState.ClassStarted);
        SetWorldStateArg(0);
    }

    private void ProfessorStartPractice ()
    {
        FirebaseManager.instance.SetWorldState(WorldState.PracticeStarted);
        SetWorldState(WorldState.PracticeStarted);
        SetWorldStateArg(0);
    }

    private void ProfessorEndClass ()
    {
        if (classImagesIndex != (classImages.Count - 1))
        {
            return;
        }

        canvasClassRoomText.gameObject.SetActive(false);
        canvasPracticeRoomText.gameObject.SetActive(true);
        classImagesIndex = 0;
        FirebaseManager.instance.SetWorldState(WorldState.WaitingOnPracticeRoom);
        SetWorldState(WorldState.WaitingOnPracticeRoom);
        SetWorldStateArg(0);
    }

    private void ProfessorNextClassScreenClick()
    {
        if (classImagesIndex >= 0 && classImagesIndex < classImages.Count - 1)
        {
            classImagesIndex++;
            FirebaseManager.instance.SetWorldStateArg(classImagesIndex);
            SetWorldStateArg(classImagesIndex);
        }
    }

    private void ProfessorPreviousClassScreenClick()
    {
        if (classImagesIndex > 0) 
        {
            classImagesIndex--;
            FirebaseManager.instance.SetWorldStateArg(classImagesIndex);
            SetWorldStateArg(classImagesIndex);
        }
    }

    private void ProfessorStartFireAccident ()
    {
        FirebaseManager.instance.SetWorldState(WorldState.FireAccident);
        SetWorldState(WorldState.FireAccident);
        FirebaseManager.instance.SetWorldStateArg(0);
        SetWorldStateArg(0);
    }

    private void ProfessorStartQuiz ()
    {
        ClearAllStudentEpiId();

        quizIndex = 0;

        FirebaseManager.instance.SetWorldState(WorldState.QuizStarted);
        SetWorldState(WorldState.QuizStarted);
        FirebaseManager.instance.SetWorldStateArg(0);
        SetWorldStateArg(0);
    }

    private void ProfessorQuizQuestionClicked ()
    {
        CurrentWorldStateArg++;
        FirebaseManager.instance.SetWorldStateArg(CurrentWorldStateArg);
        SetWorldStateArg(CurrentWorldStateArg);

        if (CurrentWorldStateArg > 1)
        {
            Debug.LogWarning("Leu");
            StartCoroutine(FirebaseManager.instance.GetAllUsersRuntimeData(OnAllUsersRuntimeDataReadForQuiz));
            return;
        }

        //ClearAllStudentEpiId();
    }

    private void OnAllUsersRuntimeDataReadForQuiz (Dictionary<string, UserRuntimeData> usersDictionary)
    {
        foreach (var userData in usersDictionary)
        {
            if (userData.Key != playerHandler.UserId)
            {
                Debug.Log("val: " + userData.Value.epiId);
                studentQuizData[userData.Key].epiIds[quizIndex] = userData.Value.epiId;
            }
        }

        Debug.LogWarning("Leu banco");

        quizIndex++;

        if (CurrentWorldStateArg == 4)
        {
            PrintQuizResult();
            ProfessorEndQuiz();
            return;
        }

        ClearAllStudentEpiId();
    }

    private void ProfessorEndQuiz ()
    {
        FirebaseManager.instance.SetWorldState(WorldState.QuizFinished);
        SetWorldState(WorldState.QuizFinished);
    }

    private void PrintQuizResult ()
    {
        string usernameText = "";
        string resultText = "";

        foreach (var studentData in studentQuizData)
        {
            //Debug.Log("Estudante: " + studentData.Key + " resultado:");
            var count = 0;
            for (int i = 0; i < studentData.Value.epiIds.Length; i++)
            {
                var studentOption = int.Parse(studentData.Value.epiIds[i]);

                Debug.Log("CHECK: " + studentOption + " and " + WorldSettings.quizAnswerKeys[i]);

                if (studentOption == WorldSettings.quizAnswerKeys[i])
                {
                    count++;
                }

                //Debug.Log("Pergunta " + i + ": valor " + studentOption);
            }

            usernameText += GetMateHandler(studentData.Key).RegisterData.username + ":\n";
            resultText += count + " acertos\n";
            //result += GetMateHandler(studentData.Key).RegisterData.username + ":" + count + " acertos\n";
        }

        OnScoreChanged?.Invoke(usernameText, resultText);

        usernameText += "$";

        string result = usernameText + resultText;

        FirebaseManager.instance.SetQuizText(result);
    }

    private void ClearAllStudentEpiId ()
    {
        if (playerHandler.RegisterData.IsProfessor)
        {
            for (int i = 0; i < mateHandlers.Count; i++)
            {
                mateHandlers[i].RuntimeData.epiId = (-1).ToString();
                FirebaseManager.instance.SetUserRuntimeAttribute(mateHandlers[i].UserId, UserRuntimeAttribute.epiId, -1);
            }
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

        // check if mates are already sit in a desk waypoint
        if (waypointHandler.WaypointType == WaypointType.Desk)
        {
            // professor cannot sit on desks
            //if (playerHandler.RegisterData.IsProfessor)
            //{
            //    return;
            //}

            for (int i = 0; i < mateHandlers.Count; i++)
            {
                if (mateHandlers[i].CurrentWaypoint == waypointHandler)
                {
                    return;
                }
            }
        }

        FirebaseManager.instance.SetUserRuntimeAttribute(playerHandler.UserId, UserRuntimeAttribute.waypoint, waypointHandler.WaypointIndex);
        OnClientWaypointChanged(playerHandler.UserId, waypointHandler.WaypointIndex);
    }

    private void CreateMateHandler(string _userId, UserRuntimeData _userRuntimeData)
    {
        var mateHandler = Instantiate(mateHandlerPrefab);
        mateHandler.SetUserId(_userId);
        mateHandler.SetUserRuntimeData(_userRuntimeData);
        mateHandler.OnClientWaypointReached += OnMateWaypointReached;
        mateHandlers.Add(mateHandler);

        // it will continue the creation OnMateRegisterDataRead
        StartCoroutine(FirebaseManager.instance.GetUserRegisterData(_userId, OnMateRegisterDataRead));
    }

    private void OnMateRegisterDataRead(string userId, UserRegisterData userRegisterData)
    {
        var mateHandler = GetMateHandler(userId);
        if (mateHandler != null)
        {
            if (!studentFireStateDict.TryGetValue(userId, out var value))
            {
                studentFireStateDict.Add(userId, 0);
            }

            mateHandler.SetUserRegisterData(userRegisterData);
            var waypointIdx = int.Parse(mateHandler.RuntimeData.waypoint);
            mateHandler.ChangeModel();
            mateHandler.SetPosition(waypoints[waypointIdx].transform.position);
            mateHandler.SetInitAnimation(waypoints[waypointIdx]);
            mateHandler.OnMateWaypointChanged = OnClientWaypointChanged;
            mateHandler.InitializeClient();
            FirebaseManager.instance.
                RegisterUserRuntimeAttributeChangeValueEvent(userId, UserRuntimeAttribute.waypoint,
                mateHandler.OnMateWaypointValueChanged);
            FirebaseManager.instance.
                RegisterUserRuntimeAttributeChangeValueEvent(userId, UserRuntimeAttribute.state,
                mateHandler.OnMateStateValueChanged);
            FirebaseManager.instance.
                RegisterWorldStateChangeValueEvent(OnWorldStateChanged);
            FirebaseManager.instance.
                RegisterWorldStateArgChangeValueEvent(OnWorldStateArgChanged);

            UpdateUsernameDestinationMsg();

            if (!mateHandler.RegisterData.IsProfessor)
            {
                studentQuizData.Add(mateHandler.UserId, new StudentQuizData { epiIds = new string[WorldSettings.quizAnswerKeys.Length] });

                if (playerHandler != null && playerHandler.IsClientInitialized)
                {
                    if (playerHandler.RegisterData.IsProfessor)
                    {
                        FirebaseManager.instance.
                            RegisterUserRuntimeAttributeChangeValueEvent(userId, UserRuntimeAttribute.fireState,
                            mateHandler.OnMateFireStateValueChanged);
                        mateHandler.OnMateFireStateChanged += OnStudentFireStateValueChanged;
                    }
                }
            }
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
            var waypoint = waypoints[waypointValue];

            if (waypoints[waypointValue].WaypointType == WaypointType.Door)
            {
                waypoint = waypoints[waypointsRoomOrigins[waypoints[waypointValue].GetComponent<DoorHandler>().roomIndex]];
                playerHandler.SetNewWaypoint(waypoint);
                playerHandler.SetPosition(waypoint.transform.position);
                playerHandler.SetRotation();
            }
            else
            {
                playerHandler.SetNewWaypoint(waypoint);
            }
        }
        else
        {
            var mateHandler = GetMateHandler(userId);
            if (mateHandler != null)
            {
                var waypoint = waypoints[waypointValue];

                if (waypoints[waypointValue].WaypointType == WaypointType.Door)
                {
                    waypoint = waypoints[waypointsRoomOrigins[waypoints[waypointValue].GetComponent<DoorHandler>().roomIndex]];
                    mateHandler.SetPosition(waypoint.transform.position);
                }

                mateHandler.SetNewWaypoint(waypoint);

                if (waypoint != playerHandler.CurrentWaypoint)
                {
                    mateHandler.ShowModel(true);
                }
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

        if (userId != playerHandler.UserId)
        {
            return;
        }

        OnReceivedMessage?.Invoke(message);
    }

    private void SendClientMessage(string userId, string message)
    {
        FirebaseManager.instance.SetUserRuntimeAttribute(userId, UserRuntimeAttribute.message, message);
    }

    private void PrintWorldStateMessage ()
    {
        if (WorldStateDict.TryGetValue(CurrentWorldState.ToString(), out var stateData))
        {
            if (stateData.Length > CurrentWorldStateArg)
            {
                OnWorldStateDataChanged?.Invoke(CurrentWorldState, stateData[CurrentWorldStateArg]);
            }
        }
        else
        {
            Debug.LogWarning("Not present in dict.");
        }
    }

    private void PrintWorldStateMessageDirectly (StateData stateData)
    {
        OnWorldStateDataChanged?.Invoke(CurrentWorldState, stateData);
    }

    private void OnObjectPicked (ObjectHandler obj)
    {
        if (playerHandler.CurrentObject != null)
        {
            Debug.LogWarning("Ja tem objecto na mao");
            return;
        }

        playerHandler.SetObjectHandler(obj);
    }

    private DropData OnObjectDropped (ObjectHandler obj)
    {
        if (playerHandler.CurrentObject == null)
        {
            Debug.LogWarning("Cannot drop null object.");
            return new DropData { transformToDrop = null, dropOnOrigin = true };
        }

        playerHandler.SetObjectHandler(null);

        if (playerHandler.CurrentWaypoint.Equals(waypointToDropEPI))
        {
            playerHandler.RuntimeData.epiId = obj.EpiId.ToString();
            FirebaseManager.instance.SetUserRuntimeAttribute(playerHandler.UserId, UserRuntimeAttribute.epiId, obj.EpiId);

            return new DropData { transformToDrop = dropTransform, dropOnOrigin = false };
        }

        return new DropData { transformToDrop = null, dropOnOrigin = true };
    }

    private void OnQuizResultTextChanged (object sender, ValueChangedEventArgs args)
    {
        var message = args.Snapshot.Value.ToString();
        var texts = message.Split('$');
        if (texts.Length > 1)
        {
            OnScoreChanged?.Invoke(texts[0], texts[1]);
        }
    }

    private void OnCharacterMovementSpeedChanged (object sender, ValueChangedEventArgs args)
    {
        float speed = float.Parse(args.Snapshot.Value.ToString());
        if (playerHandler != null)
        {
            playerHandler.SetMovementSpeed(speed);
        }
    }

    private void OnPlayerWaypointReached (ClientHandler clientHandler, WaypointHandler waypoint)
    {
        for (int i = 0; i < mateHandlers.Count; i++)
        {
            mateHandlers[i].ShowModel(mateHandlers[i].CurrentWaypoint != playerHandler.CurrentWaypoint);
        }
    }

    private void OnMateWaypointReached (ClientHandler clientHandler, WaypointHandler waypoint)
    {
        clientHandler.ShowModel(waypoint != playerHandler.CurrentWaypoint);
    }

    public void OnSendMsgButtonClicked (string message, string destination, int destinationValue)
    {
        if (playerHandler == null || !playerHandler.IsClientInitialized)
        {
            return;
        }

        string msg = "";

        if (destinationValue > 0)
        {
            msg += ("[P] " + playerHandler.RegisterData.username + ": " + message);

            for (int i = 0; i < mateHandlers.Count; i++)
            {
                if (mateHandlers[i].RegisterData.username == destination)
                {
                    FirebaseManager.instance.SetUserRuntimeAttribute(mateHandlers[i].UserId, UserRuntimeAttribute.message, msg);
                    break;
                }
            }
        }
        else
        {
            msg += (playerHandler.RegisterData.username + ": " + message);

            for (int i = 0; i < mateHandlers.Count; i++)
            {
                FirebaseManager.instance.SetUserRuntimeAttribute(mateHandlers[i].UserId, UserRuntimeAttribute.message, msg);
            }

            //FirebaseManager.instance.SetUserRuntimeAttribute(playerHandler.UserId, UserRuntimeAttribute.message, msg);
        }

        OnReceivedMessage?.Invoke(msg);
    }

    private void UpdateUsernameDestinationMsg ()
    {
        List<string> destinationOptions = new List<string>();
        destinationOptions.Add("Geral");
        for (int i = 0; i < mateHandlers.Count; i++)
        {
            if (mateHandlers[i].RegisterData == null)
            {
                continue;
            }

            destinationOptions.Add(mateHandlers[i].RegisterData.username);
        }

        OnDestinationMsgUsernamesChanged?.Invoke(destinationOptions);
    }

    public void OnMateMessageValueChanged (object sender, ValueChangedEventArgs args)
    {
        var message = args.Snapshot.Value.ToString();
        OnReceivedMessage?.Invoke(message);
    }

    private void OnStudentFireStateValueChanged (string _userId, int fireStateValue)
    {
        if (fireAccidentDone)
        {
            return;
        }

        Debug.LogWarning("userId " + _userId + "va: " + fireStateValue + " dict: " + studentFireStateDict.Count);

        if (studentFireStateDict.TryGetValue(_userId, out var value))
        {
            studentFireStateDict[_userId] = fireStateValue;
            Debug.LogWarning("AAA");
        }

        var allDone = true;

        // check whether all students are in the last fire state
        foreach (var student in studentFireStateDict)
        {
            if (student.Key == playerHandler.UserId)
            {
                continue;
            }

            if (student.Value != 2)
            {
                allDone = false;
                break;
            }

            Debug.LogWarning("BB " + student.Value);
        }

        if (allDone && studentFireStateDict.Count > 0)
        {
            OnProfessorFireExtinguishedDone();
        }
    }

    private void OnProfessorFireExtinguishedDone ()
    {
        fireAccidentDone = true;
        fireHandler.DeactivateFire();
        PrintWorldStateMessageDirectly(new StateData
        {
            stateMsg = "Todos alunos apagaram o fogo. Prossiga a aula",
            stateMsgDuration = 0,
            stateMsgToShow = true
        });
    }

    private void OnPlayerFireStateChanged (int _fireStateValue)
    {
        if (_fireStateValue > 2)
        {
            return;
        }

        if (_fireStateValue == 2)
        {
            fireHandler.DeactivateFire();
        }

        playerHandler.RuntimeData.fireState = _fireStateValue.ToString();
        FirebaseManager.instance.SetUserRuntimeAttribute(playerHandler.UserId, UserRuntimeAttribute.fireState, _fireStateValue);

        if (!playerHandler.RegisterData.IsProfessor)
        {
            if (WorldStateDict.TryGetValue(CurrentWorldState.ToString(), out var stateData))
            {
                if (stateData.Length > _fireStateValue)
                {
                    OnWorldStateDataChanged?.Invoke(CurrentWorldState, stateData[_fireStateValue]);
                }
            }
            else
            {
                Debug.LogWarning("Not present in dict.");
            }
        }
        else
        {
            if (_fireStateValue == 2)
            {
                OnProfessorFireExtinguishedDone();
            }
        }
    }
}