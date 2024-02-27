using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Firebase.Database;
using TMPro;

[Serializable]
public class dataToSave
{
    public int waypoint;
}
public class DataSaver : MonoBehaviour
{
    public dataToSave dts;
    public string userId = "PLK60QqNdJSbUVhoxPVcKBLBwDW2";
    DatabaseReference dbRef;
    public TMP_InputField WaypointField;
    public TMP_Text waypointText;

    private void Awake()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        if (dbRef == null)
        {
            Debug.Log("is null");
        }
    }

    public void SaveDataFn()
    {
        if (WaypointField.text == "") 
        {
            WaypointField.text = "0";
        }
        dts = new dataToSave
        {
            waypoint = int.Parse(WaypointField.text)

        };

        string json = JsonUtility.ToJson(dts);
        dbRef.Child("users").Child(userId).SetRawJsonValueAsync(json);
    }
 
    public void LoadDataFn()
    {
        StartCoroutine(LoadDataEnum());
    }

    IEnumerator LoadDataEnum()
    {
        var serverData = dbRef.Child("users").Child(userId).GetValueAsync();
        yield return new WaitUntil(predicate: () => serverData.IsCompleted);

        print("process is complete");

        DataSnapshot snapshot = serverData.Result;
        string jsonData = snapshot.GetRawJsonValue();

        if (jsonData != null)
        {
            print("server data found");

            dts = JsonUtility.FromJson<dataToSave>(jsonData);
            
            if (dts != null)
            {
                // Debug.Log(dts.waypoint);
                waypointText.text = dts.waypoint.ToString();
            }
        }
        else
        {
            print("no data found");
        }

    }
}