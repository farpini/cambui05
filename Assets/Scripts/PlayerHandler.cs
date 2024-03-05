using System;
using UnityEngine;

public class PlayerHandler : ClientHandler
{
    public PlayerSO playerData => (PlayerSO)clientData;

    public Transform headTransform;

    public static float cameraSpeed = 70f;

    public Vector3 goalPosition;

    public Action<WaypointHandler> OnPlayerGoalWaypointChanged;


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
                var waypointHandler = hitInfo.transform.gameObject.GetComponent<WaypointHandler>();
                Debug.LogWarning("HIT: " + waypointHandler.waypointIndex);
                SetNewGoal(waypointHandler);
            }
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            var playerRotation = transform.eulerAngles;
            var rotationValue = playerRotation.y;
            rotationValue += Time.deltaTime * cameraSpeed;
            playerRotation.y = rotationValue;
            transform.eulerAngles = playerRotation;
        }
        else if (Input.GetKey(KeyCode.LeftArrow)) 
        {
            var playerRotation = transform.eulerAngles;
            var rotationValue = playerRotation.y;
            rotationValue -= Time.deltaTime * cameraSpeed;
            playerRotation.y = rotationValue;
            transform.eulerAngles = playerRotation;
        }

        UpdatePosition();
    }

    public void SetCamera (WaypointHandler waypointHandler)
    {
        Camera.main.transform.SetParent(headTransform);
        Camera.main.transform.localPosition = Vector3.zero;
        Camera.main.transform.localEulerAngles = new Vector3(10f, 130f, 0f);

        transform.position = waypointHandler.transform.position;
        goalPosition = transform.position;
    }

    private void SetNewGoal (WaypointHandler waypointHandler)
    {
        goalPosition = waypointHandler.transform.position;

        OnPlayerGoalWaypointChanged?.Invoke(waypointHandler);
    }

    private void UpdatePosition ()
    {
        Vector3 currentPosition = transform.position;
        transform.position = Vector3.MoveTowards(currentPosition, goalPosition, 0.01f);
    }

}