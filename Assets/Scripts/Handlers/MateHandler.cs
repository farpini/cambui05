using UnityEngine;
using Firebase;
using Firebase.Database;
using System;
using TMPro;

public class MateHandler : ClientHandler
{
    public static int usernamFontMaxSize = 24;
    public static float usernameDistanceForFontMaxSize = 10f;

    public Transform standLabelTransform;
    public Transform sitLabelTransform;

    public TextMeshPro usernameLabel;

    public Action<string, int> OnMateWaypointChanged;

    public void Update ()
    {
        if (!isClientInitialized)
        {
            return;
        }

        UpdatePosition();
        ChangeAnimator();
    }

    public void OnMateWaypointValueChanged (object sender, ValueChangedEventArgs args)
    {
        OnMateWaypointChanged?.Invoke(UserId, int.Parse(args.Snapshot.Value.ToString()));
    }

    public override void SetUsernameLabel ()
    {
        usernameLabel.fontSizeMin = 1f;
        usernameLabel.fontSizeMax = usernamFontMaxSize;
        usernameLabel.text = registerData.username;
    }

    protected override void UpdateMovementLookRotation (Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    public void UpdateMateLabel (Vector3 playerPosition)
    {
        // change the username label parenting according to its state
        if (runtimeData.state == ClientState.Sit.ToString())
        {
            usernameLabel.transform.SetParent(sitLabelTransform.transform, false);
        }
        else
        {
            usernameLabel.transform.SetParent(standLabelTransform.transform, false);
        }

        // rotate the username label to the direction of the player
        var direction = (transform.position - playerPosition).normalized;
        if (direction != Vector3.zero)
        {
            usernameLabel.transform.rotation = Quaternion.LookRotation(direction);
        }
        
        // change the username font size according to the player distance
        var distanceNormalized = Mathf.InverseLerp(0f, usernameDistanceForFontMaxSize, Vector3.Distance(transform.position, playerPosition));
        usernameLabel.fontSize = Mathf.Lerp(10f, usernamFontMaxSize, distanceNormalized);
    }
}