
using Firebase.Database;
using System;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UIElements;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public abstract class ClientHandler : MonoBehaviour
{
    protected static float movementSpeed = 2f;

    protected WaypointHandler currentWaypoint;
    protected ObjectHandler currentObject;

    protected string userId;
    protected UserRegisterData registerData;
    protected UserRuntimeData runtimeData;

    protected bool isClientInitialized = false;
    protected bool hasStateChanged = false;

    protected Transform lookTransform;

    public string UserId => userId;
    public UserRegisterData RegisterData => registerData;
    public UserRuntimeData RuntimeData => runtimeData;
    public bool IsClientInitialized => isClientInitialized;
    public WaypointHandler CurrentWaypoint => currentWaypoint;
    public ObjectHandler CurrentObject => currentObject;

    public Action<int> OnRoomChange;
    public Action<WaypointHandler> OnWaypointClicked;
    public Action<ClientHandler, WaypointHandler> OnClientWaypointReached;
    public Action<string, string> OnClientMessageChanged;


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

    public void SetRotation ()
    {
        lookTransform.rotation = Quaternion.Euler(currentWaypoint.WaypointEnterDirection);
        lookTransform.GetComponentInParent<XROrigin>().MatchOriginUpCameraForward(lookTransform.up, lookTransform.forward);
    }

    public void SetMovementSpeed (float _movementSpeed)
    {
        movementSpeed = _movementSpeed;
    }

    public void SetObjectHandler (ObjectHandler objectHandler)
    {
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
            if (this is PlayerHandler)
            {
                // set state to moving
                runtimeData.state = ClientState.Walking.ToString();
                FirebaseManager.instance.SetUserRuntimeAttribute(UserId, UserRuntimeAttribute.state, runtimeData.state);
                hasStateChanged = true;
            }
        }
        else
        {
            if (this is PlayerHandler)
            {
                runtimeData.state = waypointHandler.WaypointType == WaypointType.Desk ? ClientState.Sit.ToString() :
                    ClientState.Idle.ToString();
                FirebaseManager.instance.SetUserRuntimeAttribute(UserId, UserRuntimeAttribute.state, runtimeData.state);
                hasStateChanged = true;
            }
        }

        //FirebaseManager.instance.SetUserRuntimeAttribute(UserId, UserRuntimeAttribute.state, runtimeData.state);

        // always stand-up when a new waypoint is set
        SetCamera(true);
    }

    protected void UpdatePosition (float deltaTime)
    {
        if (currentWaypoint == null)
        {
            return;
        }

        var currentPosition = transform.position;
        var targetPosition = currentWaypoint.transform.position;
        var newPosition = Vector3.MoveTowards(currentPosition, targetPosition, movementSpeed * deltaTime);

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

                    if(this is PlayerHandler)
                    {
                        lookTransform.GetComponentInParent<XROrigin>().MatchOriginUpCameraForward(lookTransform.up, lookTransform.forward);
                    }
                }
            }
        }
        else
        {
            if (this is PlayerHandler)
            {
                if (runtimeData.state != ClientState.Walking.ToString())
                {
                    runtimeData.state = ClientState.Walking.ToString();
                    FirebaseManager.instance.SetUserRuntimeAttribute(UserId, UserRuntimeAttribute.state, runtimeData.state);
                }
            }

            transform.position = newPosition;
        }
    }

    private bool IsCloseEnoughToTarget (Vector3 currentPosition, Vector3 targetPosition)
    {
        return Vector3.Distance(currentPosition, targetPosition) < 0.1f;
    }

    protected virtual void OnWaypointPositionReached ()
    {
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

    public virtual void OnClientMessageValueChanged (object sender, ValueChangedEventArgs args)
    {
        var message = args.Snapshot.Value.ToString();
        OnClientMessageChanged?.Invoke(UserId, message);
        //Debug.Log(RegisterData.username.ToString() + ": " + message);
    }

    public virtual void ShowModel (bool _toShow)
    {
    }
}
