using UnityEngine;
using Firebase;
using Firebase.Database;

public class MateHandler : ClientHandler
{
    public MateSO mateData => (MateSO)clientData;

    public Vector3 goalPosition = Vector3.zero;


    public void Initialize (MateSO _mateData)
    {
        clientData = _mateData;

        mateData.OnWaypointChangedValue = OnWaypointChanged;
    }

    public void Update ()
    {
        UpdatePosition();
    }

    private void SetNewGoal (WaypointHandler waypointHandler)
    {
        goalPosition = waypointHandler.transform.position;

        //OnPlayerGoalWaypointChanged?.Invoke(waypointHandler);
        //if (playerData.clientState == ClientState.Sit) OnClientStateChanged?.Invoke(playerData.clientState);
    }

    private void UpdatePosition ()
    {
        Vector3 currentPosition = transform.position;
        transform.position = Vector3.MoveTowards(currentPosition, goalPosition, 0.01f);
    }

    private void SetNewDesk (DeskHandler deskHandler)
    {
        goalPosition = deskHandler.transform.position;

        //OnPlayerDeskChanged?.Invoke(deskHandler);
        //OnClientStateChanged?.Invoke(playerData.clientState);
    }

    private void OnWaypointChanged (object sender, ValueChangedEventArgs args)
    {
        int waypointIdx = int.Parse(args.Snapshot.Value.ToString());

        SetNewGoal(UserManager.instance.waypoints[waypointIdx]);
    }
    
}