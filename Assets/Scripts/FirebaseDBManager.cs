using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using TMPro;

public class FirebaseDBManager : MonoBehaviour
{
    // Start is called before the first frame update

    DatabaseReference reference;

    //Waypoint variables
    [Header("Waypoint")]
    public TMP_InputField WaypointField;
    public TMP_Text scoreText;
    public Button waypointButton;

    private void Awake()
    {
        waypointButton.onClick.AddListener(() => AddWaypoint());
    }

    public void AddWaypoint()
    {
        UpdateScore();
    }

    void Start()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        FirebaseDatabase.DefaultInstance
        .GetReference("waypoint")
        .ValueChanged += HandleUpdateScore;
    }

    public void HandleUpdateScore(object sender, ValueChangedEventArgs args)
    {
        DataSnapshot snapshot = args.Snapshot;
        Debug.Log(snapshot.Value);
        scoreText.text = snapshot.Value.ToString();

    }

    public void UpdateScore()
    {
        FirebaseDatabase.DefaultInstance
        .GetReference("waypoint")
        .GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError(task);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                int value = int.Parse(Convert.ToString(snapshot.Value));
                value++;
                reference.Child("waypoint").SetValueAsync(value);

            }
        });
    }
}
