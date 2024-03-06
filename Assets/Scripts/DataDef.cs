using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class ClientSO : ScriptableObject
{
    public string userId;
    public int avatarId;
    public string userName;
    public int actualWaypoint;
    public int roomId;
    public ClientState clientState;
    public ClientType clientRole;
    public List<string> messages;

    public Action<int> OnChangeWaypoint;
    public Action<ClientState> OnChangeClientState;

}

[Serializable]
public class PlayerSO : ClientSO
{



}

[Serializable]
public class MateSO : ClientSO
{



}

public enum ClientState
{
    Stand = 0,
    Sit = 1
}


public class MateHandler : ClientHandler
{
    public MateSO mateData => (MateSO)clientData;




    public void Initialize (MateSO _mateData)
    {
        clientData = _mateData;
    }

}

public class UsersManager : MonoBehaviour
{
    public GameObject prefabAvatar1;
    public GameObject prefabAvatar2;



    public PlayerSO playerData;

    public List<MateSO> matesData;



    public void OnLoginSuccess (int avatarId)
    {
        var obj = Instantiate(new GameObject());
        var playerHandler = obj.GetComponent<PlayerHandler>();

        playerData = ScriptableObject.CreateInstance<PlayerSO>();
        playerData.avatarId = 0;

        playerHandler.Initialize((PlayerSO)playerData);

        playerData.OnChangeWaypoint += OnPlayerWaypointChanged;
        playerData.OnChangeClientState += OnClientStateChanged;
    }

    public void OnMateLogged (int userId)
    {
        foreach (MateSO mate in matesData)
        {
            var obj = Instantiate(new GameObject());
            var mateHandler = obj.GetComponent<MateHandler>();

            ScriptableObject m = ScriptableObject.CreateInstance<MateSO>();
            m = mate;
            mate.avatarId = 0;

            mateHandler.Initialize(mate);

            mate.OnChangeWaypoint += OnPlayerWaypointChanged;
            mate.OnChangeClientState += OnClientStateChanged;
        }
    }

    private void OnPlayerWaypointChanged (int waypoint)
    {
        FirebaseDatabase.DefaultInstance.GetReference("teste/atributos/waypoint").SetValueAsync(waypoint);
        // send waypoint to firebase.



    }

    private void OnClientStateChanged (ClientState clientState)
    {
        FirebaseDatabase.DefaultInstance.GetReference("teste/atributos/state").SetValueAsync(clientState);
    }
}


public class Board : MonoBehaviour
{
    public BoardSO boardData;



}

public class BoardSO : ScriptableObject
{
    public string urlVideo;
}

public class SceneManager
{
    public PlayerSO playerData;


    public void OpenUIForPlayVideo ()
    {
        if (playerData.clientRole == ClientType.professor)
        {
            //var boardObject.
        }


    }

    public void ChooseWayPonint (int waypointNumber)
    {
        playerData.actualWaypoint = waypointNumber;
    }
}