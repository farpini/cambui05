using System;
using TMPro;
using UnityEngine;

public abstract class ClientHandler : MonoBehaviour
{
    protected static float movementSpeed = 0.02f;

    protected WaypointHandler currentWaypoint;
    protected ObjectHandler currentObject;

    protected string userId;
    protected UserRegisterData registerData;
    protected UserRuntimeData runtimeData;

    protected bool isClientInitialized = false;

    protected Transform lookTransform;

    public string UserId => userId;
    public UserRegisterData RegisterData => registerData;
    public UserRuntimeData RuntimeData => runtimeData;
    public bool IsClientInitialized => isClientInitialized;
    public WaypointHandler CurrentWaypoint => currentWaypoint;
    public ObjectHandler CurrentObject => currentObject;

    public Action<int> OnRoomChange;
    public Action<WaypointHandler> OnWaypointClicked;


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

    public void SetObjectHandler (ObjectHandler objectHandler)
    {
        if (currentObject != null)
        {
            Debug.LogWarning("Ja tem objecto na mao");
            return;
        }

        currentObject = objectHandler;
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

                Debug.Log("REACHED");

                if (currentWaypoint.WaypointForceDirection && lookTransform != null)
                {
                    lookTransform.rotation = Quaternion.LookRotation(currentWaypoint.WaypointEnterDirection);
                }
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
            //animator.SetInteger("stateValue", 2);
            runtimeData.state = ClientState.Sit.ToString();

            SetCamera(false);
        }
        else if (currentWaypoint.WaypointType == WaypointType.Floor)
        {
            //animator.SetInteger("stateValue", 0);

            runtimeData.state = ClientState.Idle.ToString();

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

    /*
    public void SetNewRoomLocation(WaypointHandler waypoint) 
    { 
        currentWaypoint = waypoint;
        runtimeData.state = ClientState.Idle.ToString();
        SetPosition(waypoint.transform.position);
        OnWaypointClicked?.Invoke(waypoint);
    }  */

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