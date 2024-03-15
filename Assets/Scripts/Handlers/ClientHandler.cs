using System;
using TMPro;
using UnityEngine;

public abstract class ClientHandler : MonoBehaviour
{
    protected static float movementSpeed = 0.01f;
    protected static Vector3 sitFixedDirection = Vector3.forward;
    protected static Vector3 professorFixedDirection = Vector3.back;

    protected Animator animator;

    protected WaypointHandler currentWaypoint;

    protected string userId;
    protected UserRegisterData registerData;
    protected UserRuntimeData runtimeData;

    protected bool isClientInitialized = false;

    public string UserId => userId;
    public UserRegisterData RegisterData => registerData;
    public UserRuntimeData RuntimeData => runtimeData;
    public bool IsClientInitialized => isClientInitialized;

    public Action<int> OnRoomChange;

    private void Start()
    {
        
        //animator.SetInteger("stateValue", 0);
    }

    public void SetUserId (string _userId)
    {
        userId = _userId;
    }

    public void SetUserRegisterData (UserRegisterData _registerData)
    {
        registerData = _registerData;
        SetUsernameLabel();
    }

    public void SetUserRuntimeData (UserRuntimeData _runtimeData)
    {
        runtimeData = _runtimeData;
    }

    public virtual void InitializeClient ()
    {
        isClientInitialized = true;
    }

    public void ChangeModel()
    {
        switch (registerData.genero)
        {
            case "masculino":
                transform.GetChild(0).GetChild(3).gameObject.SetActive(true);
                animator = transform.GetComponentInChildren<Animator>(); 
                break;
            case "feminino":
                transform.GetChild(0).GetChild(2).gameObject.SetActive(true);
                animator = transform.GetComponentInChildren<Animator>();
                break;
        }
    }

    public void SetPosition (Vector3 _position)
    {
        transform.position = _position;
    }

    public void SetNewWaypoint (WaypointHandler waypointHandler)
    {
        runtimeData.waypoint = waypointHandler.WaypointIndex.ToString();
        currentWaypoint = waypointHandler;

        var currentPosition = transform.position;
        var targetPosition = currentWaypoint.transform.position;

        var direction = (targetPosition - currentPosition).normalized;
        UpdateMovementLookRotation(direction);

        if (!IsCloseEnoughToTarget(currentPosition, targetPosition))
        {
            // set state to moving
            runtimeData.state = ClientState.Walking.ToString();
            
        }
        else
        {
            runtimeData.state = ClientState.Idle.ToString();
        }

        // always stand-up when a new waypoint is set
        SetCamera(true);
    }

    protected void UpdatePosition ()
    {
        if (currentWaypoint == null)
        {
            return;
        }

        var currentPosition = transform.position;
        var targetPosition = currentWaypoint.transform.position;
        var newPosition = Vector3.MoveTowards(currentPosition, targetPosition, movementSpeed);

        if (IsCloseEnoughToTarget(newPosition, targetPosition))
        {
            if (runtimeData.state == ClientState.Walking.ToString())
            {
                transform.position = targetPosition;
                OnWaypointPositionReached();
            }
        }
        else
        {
            if (runtimeData.state != ClientState.Walking.ToString())
            {
                runtimeData.state = ClientState.Walking.ToString();
            }
            
            transform.position = newPosition;
        }
    }

    protected void ChangeAnimator()
    {
        switch (runtimeData.state)
        {
            case "Idle":
                animator.SetInteger("stateValue", 0);
                break;
            case "Walking":
                animator.SetInteger("stateValue", 1);
                break;
            case "Sit":
                animator.SetInteger("stateValue", 2);
                break;
        }
    }

    private bool IsCloseEnoughToTarget (Vector3 currentPosition, Vector3 targetPosition)
    {
        return Vector3.Distance(currentPosition, targetPosition) < 0.1f;
    }

    private void OnWaypointPositionReached ()
    {
        if (currentWaypoint.WaypointType == WaypointType.Desk)
        {
            //animator.SetInteger("stateValue", 2);
            runtimeData.state = ClientState.Sit.ToString();

            // set the direction to the class board
            transform.rotation = Quaternion.LookRotation(sitFixedDirection);

            SetCamera(false);
        }
        else if (currentWaypoint.WaypointType == WaypointType.Floor)
        {
            //animator.SetInteger("stateValue", 0);

            runtimeData.state = ClientState.Idle.ToString();

            // this is temporary, waypoint index (1) represents the professor position to start the class
            // so set it's direction to be straight to the students
            if (currentWaypoint.WaypointIndex == 1)
            {
                transform.rotation = Quaternion.LookRotation(professorFixedDirection);
            }

            SetCamera(true);
        }
        else if (currentWaypoint.WaypointType == WaypointType.Door)
        {
            Debug.Log("Porta");
            FirebaseManager.instance.SetUserRuntimeAttribute(userId, UserRuntimeAttribute.roomId, currentWaypoint.GetComponent<DoorHandler>().roomIndex.ToString());
            //UserManager.instance.roomId = int.Parse(runtimeData.roomId);
            OnRoomChange?.Invoke(currentWaypoint.GetComponent<DoorHandler>().roomIndex);
            //UserManager.instance.LoadWaypointHandlers();
        }
    }

    public void SetNewRoomLocation(WaypointHandler waypoint) 
    { 
        currentWaypoint = waypoint;
        runtimeData.state = ClientState.Idle.ToString();
        SetPosition(waypoint.transform.position);
    }  

    public virtual void SetCamera (bool isStand, bool initRotation = false)
    {
    }

    public virtual void SetUsernameLabel ()
    {
    }

    protected virtual void UpdateMovementLookRotation (Vector3 direction)
    {
    }
}