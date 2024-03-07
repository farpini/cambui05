using System;
using UnityEngine;

public class PlayerHandler : ClientHandler
{
    public PlayerSO playerData => (PlayerSO)clientData;

    // public int stateValue => playerData.clientState;

    public Transform headTransform;

    public Transform sitingTransform;

    public static float cameraSpeed = 70f;

    public Vector3 goalPosition;

    public float verticalRotationMax;

    public Action<WaypointHandler> OnPlayerGoalWaypointChanged;
    public Action<DeskHandler> OnPlayerDeskChanged;
    public Action<ClientState> OnClientStateChanged;

    public Animator animator;


    public void Initialize (PlayerSO _playerData)
    {
        clientData = _playerData;
    }

    public void ClickWaypointButton ()
    {
        playerData.actualWaypoint = 0;
        playerData.OnChangeWaypoint.Invoke(playerData.actualWaypoint);


    }

    public void Update ()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            var layerMask = LayerMask.GetMask("Waypoint");

            if (Physics.Raycast(ray, out var hitInfo, 1000f, layerMask))
            {
                if (hitInfo.transform.gameObject.GetComponent<WaypointHandler>())
                {
                    var waypointHandler = hitInfo.transform.gameObject.GetComponent<WaypointHandler>();
                    Debug.LogWarning("HIT: " + waypointHandler.waypointIndex);
                    SetNewGoal(waypointHandler);
                }
                else if (hitInfo.transform.gameObject.GetComponent<DeskHandler>())
                {
                    var deskHandler = hitInfo.transform.gameObject.GetComponent<DeskHandler>();
                    Debug.LogWarning("HIT SEAT: " + deskHandler.deskIndex);
                    SetNewDesk(deskHandler);
                }
            }
        }

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            var playerRotation = transform.eulerAngles;
            var rotationValue = playerRotation.y;
            rotationValue += Time.deltaTime * cameraSpeed;
            playerRotation.y = rotationValue;
            transform.eulerAngles = playerRotation;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) 
        {
            var playerRotation = transform.eulerAngles;
            var rotationValue = playerRotation.y;
            rotationValue -= Time.deltaTime * cameraSpeed;
            playerRotation.y = rotationValue;
            transform.eulerAngles = playerRotation;
        }

        var verticalRotationForNegative = 360f - verticalRotationMax;

        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            var cameraRotation = Camera.main.gameObject.transform.eulerAngles;
            var cameraRotationX = cameraRotation.x;
            cameraRotationX -= Time.deltaTime * cameraSpeed;
            if (cameraRotationX > verticalRotationMax && cameraRotationX < verticalRotationForNegative)
            {
                cameraRotationX = cameraRotationX > verticalRotationMax ? verticalRotationForNegative : verticalRotationMax;
            }
            cameraRotation.x = cameraRotationX;
            Camera.main.gameObject.transform.eulerAngles = cameraRotation;
        }
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            var cameraRotation = Camera.main.gameObject.transform.eulerAngles;
            var cameraRotationX = cameraRotation.x;
            cameraRotationX += Time.deltaTime * cameraSpeed;
            if (cameraRotationX > verticalRotationMax && cameraRotationX < verticalRotationForNegative)
            {
                cameraRotationX = cameraRotationX > verticalRotationMax ? verticalRotationMax : verticalRotationForNegative;
            }
            cameraRotation.x = cameraRotationX;
            Camera.main.gameObject.transform.eulerAngles = cameraRotation;
        }

        UpdatePosition();
    }

    public void SetCamera (WaypointHandler waypointHandler)
    {
        Camera.main.transform.SetParent(headTransform);
        Camera.main.transform.localPosition = Vector3.zero;
        Camera.main.transform.localEulerAngles = new Vector3(0f, 0f, 0f);

        transform.position = waypointHandler.transform.position;
        goalPosition = transform.position;
    }

    private void SetNewGoal (WaypointHandler waypointHandler)
    {
        goalPosition = waypointHandler.transform.position;

        OnPlayerGoalWaypointChanged?.Invoke(waypointHandler);
        if (playerData.clientState == ClientState.Sit) OnClientStateChanged?.Invoke(playerData.clientState);
    }

    private void UpdatePosition ()
    {
        Vector3 currentPosition = transform.position;
        transform.position = Vector3.MoveTowards(currentPosition, goalPosition, 0.01f);
    }

    private void SetNewDesk(DeskHandler deskHandler)

    {

        Camera.main.transform.SetParent(sitingTransform);
        Camera.main.transform.localPosition = Vector3.zero;
        Camera.main.transform.localEulerAngles = new Vector3(0f, 0f, 0f);

        goalPosition = deskHandler.transform.position;

        OnPlayerDeskChanged?.Invoke(deskHandler);
        OnClientStateChanged?.Invoke(playerData.clientState);
    }
}