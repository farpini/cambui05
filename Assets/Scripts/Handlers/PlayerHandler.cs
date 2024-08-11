using System;
using UnityEngine;

public class PlayerHandler : ClientHandler
{
    public ExtinguisherHandler extinguisher;

    public float cameraSpeed = 70f;
    public float cameraVerticalRotationMax = 40f;

    public Transform headTransform;
    public Transform sitingTransform;

    private Transform oculusTransform;

    public Action<ButtonType> OnButtonClicked;
    public Action<int> OnFireStateChanged;


    public void Update ()
    {
        if (!isClientInitialized)
        {
            return;
        }

        var deltaTime = Time.deltaTime;

        var currentState = runtimeData.state;
        var currentRoomId = runtimeData.roomId;

        CheckButtonClick();
        UpdatePosition(deltaTime);

        if (currentState != runtimeData.state)
        {
            FirebaseManager.instance.SetUserRuntimeAttribute(UserId, UserRuntimeAttribute.state, runtimeData.state);
        }

        if (currentRoomId != runtimeData.roomId)
        {
            FirebaseManager.instance.SetUserRuntimeAttribute(UserId, UserRuntimeAttribute.roomId, runtimeData.roomId);
        }
    }

    public override void SetCamera (bool isStand, bool initRotation = false)
    {
        if (oculusTransform == null)
        {
            return;
        }

        oculusTransform.SetParent(isStand ? headTransform : sitingTransform, false);
        oculusTransform.localPosition = Vector3.zero;
        if (initRotation)
        {
            oculusTransform.localEulerAngles = new Vector3(0f, 0f, 0f);
        }
    }

    public void SetXRGO (GameObject xrgo, GameObject cameraOffSet, ExtinguisherHandler extinguisherHandler)
    {
        oculusTransform = xrgo.transform;
        lookTransform = cameraOffSet.transform;
        extinguisher = extinguisherHandler;
        extinguisher.OnFireExtinguisherStateChanged += OnFireExtinguisherStateChanged;
        SetCamera(true);
    }

    private void OnFireExtinguisherStateChanged (int stateValue)
    {
        OnFireStateChanged?.Invoke(stateValue);
    }

    private void CheckButtonClick()
    {
        if (registerData.tipo != "professor")
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            var layerMask = LayerMask.GetMask("Professor");

            if (Physics.Raycast(ray, out var hitInfo, 1000f, layerMask))
            {
                var buttonHandler = hitInfo.transform.gameObject.GetComponent<ButtonHandler>();
                if (buttonHandler != null)
                {
                    Debug.Log("Button: " +  buttonHandler.type);
                    OnButtonClicked?.Invoke(buttonHandler.type);
                }
            }
        }
    }

    protected override void OnWaypointPositionReached ()
    {
        var currentState = runtimeData.state;

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
        else if (currentWaypoint.WaypointType == WaypointType.Door)
        {
            runtimeData.state = ClientState.Idle.ToString();
        }

        OnClientWaypointReached?.Invoke(this, currentWaypoint);
    }
}