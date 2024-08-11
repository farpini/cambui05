using UnityEngine;
using Firebase;
using Firebase.Database;
using System;
using TMPro;

public class MateHandler : ClientHandler
{
    public static int usernameFontMaxSize = 24;
    public static float usernameDistanceForFontMaxSize = 10f;

    protected Animator animator;

    public Transform standLabelTransform;
    public Transform sitLabelTransform;

    public GameObject charRenderer;

    public TextMeshPro usernameLabel;

    public Action<string, int> OnMateWaypointChanged;
    public Action<string, int> OnMateStateChanged;
    public Action<string, int> OnMateFireStateChanged;


    public void Awake ()
    {
        lookTransform = transform;
        hasStateChanged = false;
        //ShowModel(false);
    }

    public void Update ()
    {
        if (!isClientInitialized)
        {
            return;
        }

        var deltaTime = Time.deltaTime;

        UpdatePosition(deltaTime);

        //ChangeAnimator(runtimeData.state);
    }

    private void ChangeAnimator (string state)
    {
        switch (runtimeData.state)
        {
            case "Idle":
            animator.SetInteger("stateValue", 0);
            break;
            case "Walking":
            animator.SetInteger("stateValue", 1);
            break;
            case "Sit":
            animator.SetInteger("stateValue", 2);
            break;
        }
    }

    public override void InitializeClient ()
    {
        isClientInitialized = true;
    }

    public void ChangeModel ()
    {
        switch (registerData.genero)
        {
            case "masculino":
            transform.GetChild(0).GetChild(3).gameObject.SetActive(true);
            animator = transform.GetComponentInChildren<Animator>();
            break;
            case "feminino":
            transform.GetChild(0).GetChild(2).gameObject.SetActive(true);
            animator = transform.GetComponentInChildren<Animator>();
            break;
        }
    }

    public void SetInitAnimation (WaypointHandler initWaypoint)
    {
        if (RuntimeData != null)
        {
            currentWaypoint = initWaypoint;
            ChangeAnimator(runtimeData.state);
            UpdateMateDirection();
        }
    }

    public void OnMateWaypointValueChanged (object sender, ValueChangedEventArgs args)
    {
        OnMateWaypointChanged?.Invoke(UserId, int.Parse(args.Snapshot.Value.ToString()));
    }

    public void OnMateStateValueChanged (object sender, ValueChangedEventArgs args)
    {
        runtimeData.state = args.Snapshot.Value.ToString();
        ChangeAnimator(runtimeData.state);
        UpdateMateDirection();

        //OnMateStateChanged?.Invoke(UserId, int.Parse(args.Snapshot.Value.ToString()));
    }

    public void OnMateFireStateValueChanged (object sender, ValueChangedEventArgs args)
    {
        OnMateFireStateChanged?.Invoke(UserId, int.Parse(args.Snapshot.Value.ToString()));
    }

    public override void SetUsernameLabel ()
    {
        usernameLabel.fontSizeMin = 1f;
        usernameLabel.fontSizeMax = usernameFontMaxSize;
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
        usernameLabel.fontSize = Mathf.Lerp(10f, usernameFontMaxSize, distanceNormalized);
    }

    public override void ShowModel (bool _toShow)
    {
        charRenderer.SetActive(_toShow);
    }

    protected override void OnWaypointPositionReached ()
    {
        UpdateMateDirection();
    }

    private void UpdateMateDirection ()
    {
        if (currentWaypoint == null || runtimeData.state == ClientState.Walking.ToString())
        {
            return;
        }

        if (currentWaypoint.WaypointType == WaypointType.Desk)
        {
            transform.rotation = Quaternion.LookRotation(Vector3.forward);
            SetCamera(false);
        }
        else if (currentWaypoint.WaypointType == WaypointType.Floor)
        {
            if (currentWaypoint.WaypointForceDirection)
            {
                transform.rotation = Quaternion.LookRotation(currentWaypoint.WaypointEnterDirection);
            }
            SetCamera(true);
        }
    }
}