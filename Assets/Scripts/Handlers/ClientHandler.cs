using TMPro;
using UnityEngine;

public abstract class ClientHandler : MonoBehaviour
{
    protected static float movementSpeed = 0.01f;

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

    private bool IsCloseEnoughToTarget (Vector3 currentPosition, Vector3 targetPosition)
    {
        return Vector3.Distance(currentPosition, targetPosition) < 0.1f;
    }

    private void OnWaypointPositionReached ()
    {
        if (currentWaypoint.WaypointType == WaypointType.Desk)
        {
            runtimeData.state = ClientState.Sit.ToString();
            SetCamera(false);
        }
        else if (currentWaypoint.WaypointType == WaypointType.Floor)
        {
            runtimeData.state = ClientState.Idle.ToString();
            SetCamera(true);
        }
    }

    public virtual void SetCamera (bool isStand, bool initRotation = false)
    {
    }

    public virtual void SetUsernameLabel ()
    {
    }
}