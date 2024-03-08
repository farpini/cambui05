using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    //Screen object variables
    [Header("Windows")]
    public GameObject backgroundUI;
    public GameObject loginUI;
    public GameObject adminUI;
    public GameObject registerUI;

    [Header("LoginUI")]
    public TMP_InputField loginEmailField;
    public TMP_InputField loginPasswordField;
    public TMP_Text loginResultText;
    public Button loginButton;
    public Button loginRegisterButton;

    [Header("RegisterUI")]
    public TMP_InputField registerUsernameField;
    public TMP_InputField registerPasswordField;
    public TMP_InputField registerPasswordConfirmField;
    public TMP_Text registerUserResultText;
    public Button registerButton;
    public Button registerBackButton;
    public Toggle toggleMale;
    public Toggle toggleFemale;

    [Header("AdminUI")]
    public TMP_InputField adminMatriculaField;
    public TMP_InputField adminEmailField;
    public TMP_InputField adminPasswordField;
    public TMP_Text registerAdminResultText;
    public Toggle toggleStudent;
    public Toggle toggleProfessor;
    public Button adminRegisterButton;
    public Button adminBackButton;


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

        OpenLoginUI();

        toggleMale.isOn = true;
        toggleFemale.isOn = false;
        toggleProfessor.isOn = true;
        toggleStudent.isOn = false;
    }

    private void Start ()
    {
        loginButton.onClick.AddListener(() => OnLoginButtonClicked());
        loginRegisterButton.onClick.AddListener(() => OnLoginRegisterButtonClicked());

        registerButton.onClick.AddListener(() => OnRegisterButtonClicked());
        registerBackButton.onClick.AddListener(() => OnBackButtonClicked());

        adminRegisterButton.onClick.AddListener(() => OnAdminRegisterButtonClicked());
        adminBackButton.onClick.AddListener(() => OnBackButtonClicked());

        toggleMale.onValueChanged.AddListener((bool v) => OnGenderToggleChanged(v));
        toggleFemale.onValueChanged.AddListener((bool v) => OnGenderToggleChanged(v));

        toggleProfessor.onValueChanged.AddListener((bool v) => OnTypeToggleChanged(v));
        toggleStudent.onValueChanged.AddListener((bool v) => OnTypeToggleChanged(v));

        FirebaseManager.instance.OnLoginSuccess += HideAllUI;
        FirebaseManager.instance.OnLoginMissing += OpenRegisterUI;

        FirebaseManager.instance.OnUserRegisterSuccess += OpenLoginUI;

        FirebaseManager.instance.OnLoginPrintResult += PrintLoginResult;
        FirebaseManager.instance.OnUserRegisterPrintResult += PrintUserRegisterResult;
        FirebaseManager.instance.OnAdminRegisterPrintResult += PrintAdminRegisterResult;
    }

    private void OnDestroy ()
    {
        loginButton.onClick.RemoveAllListeners();
        loginRegisterButton.onClick.RemoveAllListeners();

        registerButton.onClick.RemoveAllListeners();
        registerBackButton.onClick.RemoveAllListeners();

        adminRegisterButton.onClick.RemoveAllListeners();
        adminBackButton.onClick.RemoveAllListeners();

        toggleMale.onValueChanged.RemoveAllListeners();
        toggleFemale.onValueChanged.RemoveAllListeners();
        toggleProfessor.onValueChanged.RemoveAllListeners();
        toggleStudent.onValueChanged.RemoveAllListeners();
    }

    private void OnGenderToggleChanged (bool value)
    {
        if (toggleMale.isOn)
        {
            toggleFemale.isOn = false;
        }
        else
        {
            toggleMale.isOn = false;
        }
    }

    private void OnTypeToggleChanged (bool value)
    {
        if (toggleProfessor.isOn)
        {
            toggleStudent.isOn = false;
        }
        else
        {
            toggleProfessor.isOn = false;
        }
    }

    private void OnLoginButtonClicked ()
    {
        FirebaseManager.instance.OnLoginButtonClicked(loginEmailField.text, loginPasswordField.text);
    }

    private void OnLoginRegisterButtonClicked ()
    {
        OpenAdminUI();
    }

    private void OnRegisterButtonClicked ()
    {
        FirebaseManager.instance.OnRegisterButtonClicked(registerUsernameField.text, registerPasswordField.text, 
            registerPasswordConfirmField.text, toggleMale.isOn ? ClientGender.masculino : ClientGender.feminino);
    }

    private void OnAdminRegisterButtonClicked ()
    {
        FirebaseManager.instance.OnAdminRegisterButtonClicked(adminEmailField.text, adminPasswordField.text, adminMatriculaField.text,
            toggleProfessor.isOn ? ClientType.professor : ClientType.aluno);
    }

    private void OnBackButtonClicked ()
    {
        OpenLoginUI();
    }

    private void PrintLoginResult (string loginResult, Color color)
    {
        loginResultText.text = loginResult;
        loginResultText.color = color;
    }

    private void PrintUserRegisterResult (string registerResult, Color color)
    {
        registerUserResultText.text = registerResult;
        registerUserResultText.color = color;
    }

    private void PrintAdminRegisterResult (string registerResult, Color color)
    {
        registerAdminResultText.text = registerResult;
        registerAdminResultText.color = color;
        adminEmailField.text = "";
        adminMatriculaField.text = "";
        adminPasswordField.text = "";
    }

    private void OpenLoginUI (string userId = "")
    {
        backgroundUI.SetActive(true);
        loginUI.SetActive(true);
        adminUI.SetActive(false);
        registerUI.SetActive(false);
    }

    private void OpenRegisterUI (string userId = "")
    {
        backgroundUI.SetActive(true);
        loginUI.SetActive(false);
        registerUI.SetActive(true);
        adminUI.SetActive(false);
    }

    private void OpenAdminUI (string userId = "")
    {
        backgroundUI.SetActive(true);
        loginUI.SetActive(false);
        registerUI.SetActive(false);
        adminUI.SetActive(true);
    }

    private void HideAllUI (string userId = "")
    {
        backgroundUI.SetActive(false);
        loginUI.SetActive(false);
        registerUI.SetActive(false);
        adminUI.SetActive(false);
    }
}