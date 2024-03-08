using UnityEngine;
using Firebase;
using Firebase.Database;
using System;

public class MateHandler : ClientHandler
{
    public TextMesh usernameLabel;

    public Action<string, int> OnMateWaypointChanged;


    public void Update ()
    {
        if (!isClientInitialized)
        {
            return;
        }

        UpdatePosition();
    }

    public void OnMateWaypointValueChanged (object sender, ValueChangedEventArgs args)
    {
        OnMateWaypointChanged?.Invoke(UserId, int.Parse(args.Snapshot.Value.ToString()));
    }

    public override void SetUsernameLabel ()
    {
        usernameLabel.text = registerData.username;
    }
}