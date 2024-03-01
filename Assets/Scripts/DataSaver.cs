using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Firebase.Database;
using TMPro;
using System.Runtime.Remoting;
using Google.MiniJSON;
using Newtonsoft.Json;
using static UnityEngine.Rendering.DebugUI;


public class DataSaver : MonoBehaviour
{
    DatabaseReference dbRef;
    public TMP_InputField atributoField;
    public TMP_InputField valueField;
    public TMP_Text confirmText;
    public TMP_InputField waypointField;
    public TMP_Text warningText;
    public string userId = "";
    public UserData userDataGlobal;

    private void Awake()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        if (dbRef == null)
        {
            Debug.Log("is null");
        }
    }

    private void Start ()
    {
        AuthManager.instance.OnNewRegister += NewRegister;
        AuthManager.instance.OnLogin += OnLogin;
    }

    private void OnApplicationQuit ()
    {
        if (AuthManager.instance != null)
        {
            AuthManager.instance.OnNewRegister -= NewRegister;
            AuthManager.instance.OnLogin -= OnLogin;
        }

    }

    public void RemoveDataFn()
    {
        if (atributoField.text == "")
        {
            confirmText.text = "";
            warningText.text = "Coloque um atributo!";
        }
        else
        {
            var removeclass = atributoField.text;

            FirebaseDatabase.DefaultInstance.GetReference("users/" + userId + "/atributos/" + removeclass).RemoveValueAsync();
            userDataGlobal.atributos.Remove(removeclass);
            warningText.text = "";
            confirmText.text = "Atributo " + removeclass + " removido!";
        }
    }

    public void AddDataFn()
    {
        if (atributoField.text == "")
        {
            confirmText.text = "";
            warningText.text = "Coloque um atributo!";
        }
        else
        {
            if (valueField.text == "")
            {
                valueField.text = "0";
            }

            var newvalue = valueField.text;
            var newclass = atributoField.text;

            FirebaseDatabase.DefaultInstance.GetReference("users/" + userId + "/atributos/" + newclass).SetValueAsync(newvalue);
            userDataGlobal.atributos.Add(newclass, newvalue);
            warningText.text = "";
            confirmText.text = "Atributo " + newclass + " adicionado";

        }
    }

        public void SaveDataFn()
    {

        if (waypointField.text == "")
        {
            waypointField.text = "0";
        }

        var waypoint = int.Parse(waypointField.text);
        FirebaseDatabase.DefaultInstance.GetReference("users/" + userId + "/atributos/waypoint").SetValueAsync(waypoint);
        warningText.text = "";
        confirmText.text = "Escrito: " + waypoint.ToString();
        
    }

    public void LoadDataFn()
    {
        StartCoroutine(LoadDataEnum());
    }

    IEnumerator LoadDataEnum()
    {
        var serverData = FirebaseDatabase.DefaultInstance.GetReference("users/" + userId + "/atributos/waypoint").GetValueAsync();
        Debug.Log(userId);
        yield return new WaitUntil(predicate: () => serverData.IsCompleted);

        print("process is complete");

        DataSnapshot snapshot = serverData.Result;

        if (snapshot != null && snapshot.Value != null)
        {
            var waypoint = snapshot.Value;
            warningText.text = "";
            confirmText.text = "Lido: " + waypoint.ToString();
        }
        else
        {
            confirmText.text = "";
            warningText.text = "Erro no banco de dados!";
        }
    }

    private void NewRegister (string _userId, string _userName, Gendertype _gender, Type _type)
    {
        userId = _userId;

        var atributos = new Dictionary<string, object>();
        atributos.Add("username", _userName);
        atributos.Add("waypoint", 0);
        atributos.Add("sala", 0);
        atributos.Add("genero", _gender.ToString());
        atributos.Add("tipo", _type.ToString());

        userDataGlobal = new UserData
        {
                atributos = atributos
        };

            var json = JsonConvert.SerializeObject(userDataGlobal);
            FirebaseDatabase.DefaultInstance.GetReference("users/" + userId).SetRawJsonValueAsync(json);
    }

    private void OnLogin(string _userId)
    {
        userId = _userId;
        StartCoroutine(LoadUserData());
    }

    IEnumerator LoadUserData()
    {
        var serverData = FirebaseDatabase.DefaultInstance.GetReference("users/" + userId).GetValueAsync();
        yield return new WaitUntil(predicate: () => serverData.IsCompleted);

        DataSnapshot snapshot = serverData.Result;
        string jsonData = snapshot.GetRawJsonValue();

        if (jsonData != null)
        {
            print("server data found");

            userDataGlobal = JsonConvert.DeserializeObject<UserData>(jsonData);
        }
        else
        {
            print("no data found");
        }
    }

}

[Serializable]
public class UserData
{
    public Dictionary<string, object> atributos;
}