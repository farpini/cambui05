using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;


    //Screen object variables
    public GameObject backgroundUI;
    public GameObject loginUI;
    public GameObject registerUI;
    public GameObject waypointUI;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    private void Start ()
    {
        AuthManager.instance.OnLogin += HideAllUI;
    }

    //Functions to change the login screen UI
    public void LoginScreen() //Back button
    {
        loginUI.SetActive(true);
        registerUI.SetActive(false);
    }
    public void RegisterScreen() // Regester button
    {
        loginUI.SetActive(false);
        registerUI.SetActive(true);
    }

    public void OpenWaypointTest()
    {
        loginUI.SetActive(false);
        registerUI.SetActive(false);
        waypointUI.SetActive(true);
    }

    public void HideAllUI(string userId)
    {
        backgroundUI.SetActive(false);
        loginUI.SetActive(false);
        registerUI.SetActive(false);
        waypointUI.SetActive(false);

    }
}
