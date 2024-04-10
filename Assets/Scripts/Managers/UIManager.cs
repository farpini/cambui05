using Microsoft.MixedReality.Toolkit.Experimental.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public NonNativeKeyboard XRKeyboard;

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
    public Button adminClearButton;

    [Header("BottomPanel")]
    public GameObject panelObject;
    public TMP_Text msgText;

    [Header("PraticeRoom")]
    public TMP_Text scoreText;

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

        panelObject.SetActive(false);
        toggleMale.isOn = true;
        toggleFemale.isOn = false;
        toggleProfessor.isOn = true;
        toggleStudent.isOn = false;
    }

    private void Start()
    {
        loginEmailField.onSelect.AddListener((string s) => OnLoginEmailFieldSelected());
        loginPasswordField.onSelect.AddListener((string s) => OnLoginPasswordFieldSelected());

        registerUsernameField.onSelect.AddListener((string s) => OnRegisterUsernameFieldSelected());
        registerPasswordField.onSelect.AddListener((string s) => OnRegisterPasswordFieldSelected());
        registerPasswordConfirmField.onSelect.AddListener((string s) => OnRegisterPasswordConfirmFieldSelected());

        adminMatriculaField.onSelect.AddListener((string s) => OnAdminMatriculaFieldSelected());
        adminEmailField.onSelect.AddListener((string s) => OnAdminEmailFieldSelected());
        adminPasswordField.onSelect.AddListener((string s) => OnAdminPasswordFieldSelected());

        loginButton.onClick.AddListener(() => OnLoginButtonClicked());
        loginRegisterButton.onClick.AddListener(() => OnLoginRegisterButtonClicked());

        registerButton.onClick.AddListener(() => OnRegisterButtonClicked());
        registerBackButton.onClick.AddListener(() => OnBackButtonClicked());

        adminRegisterButton.onClick.AddListener(() => OnAdminRegisterButtonClicked());
        adminBackButton.onClick.AddListener(() => OnBackButtonClicked());
        adminClearButton.onClick.AddListener(() => OnClearButtonClicked());

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

        UserManager.instance.OnWorldStateDataChanged += ShowWorldStateMessage;
        UserManager.instance.OnScoreChanged += ShowScoreText;
    }

    private void OnDestroy()
    {
        loginEmailField.onSelect.RemoveAllListeners();
        loginPasswordField.onSelect.RemoveAllListeners();

        registerUsernameField.onSelect.RemoveAllListeners();
        registerPasswordField.onSelect.RemoveAllListeners();
        registerPasswordConfirmField.onSelect.RemoveAllListeners();

        adminMatriculaField.onSelect.RemoveAllListeners();
        adminEmailField.onSelect.RemoveAllListeners();
        adminPasswordField.onSelect.RemoveAllListeners();

        loginButton.onClick.RemoveAllListeners();
        loginRegisterButton.onClick.RemoveAllListeners();

        registerButton.onClick.RemoveAllListeners();
        registerBackButton.onClick.RemoveAllListeners();

        adminRegisterButton.onClick.RemoveAllListeners();
        adminBackButton.onClick.RemoveAllListeners();
        adminClearButton.onClick.RemoveAllListeners();

        toggleMale.onValueChanged.RemoveAllListeners();
        toggleFemale.onValueChanged.RemoveAllListeners();
        toggleProfessor.onValueChanged.RemoveAllListeners();
        toggleStudent.onValueChanged.RemoveAllListeners();
    }

    private void OnGenderToggleChanged(bool value)
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

    private void OnTypeToggleChanged(bool value)
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

    private void OnLoginButtonClicked()
    {
        FirebaseManager.instance.OnLoginButtonClicked(loginEmailField.text, loginPasswordField.text);
    }

    private void OnLoginRegisterButtonClicked()
    {
        OpenAdminUI();
    }

    private void OnRegisterButtonClicked()
    {
        FirebaseManager.instance.OnRegisterButtonClicked(registerUsernameField.text, registerPasswordField.text,
            registerPasswordConfirmField.text, toggleMale.isOn ? ClientGender.masculino : ClientGender.feminino);
    }

    private void OnAdminRegisterButtonClicked()
    {
        FirebaseManager.instance.OnAdminRegisterButtonClicked(adminEmailField.text, adminPasswordField.text, adminMatriculaField.text,
            toggleProfessor.isOn ? ClientType.professor : ClientType.aluno);
    }

    private void OnBackButtonClicked()
    {
        OpenLoginUI();
    }

    private void OnClearButtonClicked()
    {
        FirebaseManager.instance.ClearUserConnectedData();
    }

    private void PrintLoginResult(string loginResult, Color color)
    {
        loginResultText.text = loginResult;
        loginResultText.color = color;
    }

    private void PrintUserRegisterResult(string registerResult, Color color)
    {
        registerUserResultText.text = registerResult;
        registerUserResultText.color = color;
    }

    private void PrintAdminRegisterResult(string registerResult, Color color)
    {
        registerAdminResultText.text = registerResult;
        registerAdminResultText.color = color;
        adminEmailField.text = "";
        adminMatriculaField.text = "";
        adminPasswordField.text = "";
    }

    private void OpenLoginUI(string userId = "")
    {
        backgroundUI.SetActive(true);
        loginUI.SetActive(true);
        adminUI.SetActive(false);
        registerUI.SetActive(false);
    }

    private void OpenRegisterUI(string userId = "")
    {
        backgroundUI.SetActive(true);
        loginUI.SetActive(false);
        registerUI.SetActive(true);
        adminUI.SetActive(false);
    }

    private void OpenAdminUI(string userId = "")
    {
        backgroundUI.SetActive(true);
        loginUI.SetActive(false);
        registerUI.SetActive(false);
        adminUI.SetActive(true);
    }

    private void HideAllUI(string userId = "")
    {
        backgroundUI.SetActive(false);
        loginUI.SetActive(false);
        registerUI.SetActive(false);
        adminUI.SetActive(false);
    }

    private void OnLoginEmailFieldSelected()
    {
        NonNativeKeyboard.Instance.InputField = loginEmailField;
        NonNativeKeyboard.Instance.PresentKeyboard();
        Vector3 direction = loginEmailField.transform.forward;
        direction.y = 0f;
        direction.Normalize();
        float verticaloffset = -1f;
        Vector3 targetPosition = loginEmailField.transform.position + direction * 0.5f + Vector3.up * verticaloffset;
        NonNativeKeyboard.Instance.SetScaleSizeValues(1.8f, 1.8f, 0f, 10f);
        NonNativeKeyboard.Instance.RepositionKeyboard(targetPosition);
    }

    private void OnLoginPasswordFieldSelected()
    {
        NonNativeKeyboard.Instance.InputField = loginPasswordField;
        NonNativeKeyboard.Instance.PresentKeyboard();
        Vector3 direction = loginPasswordField.transform.forward;
        direction.y = 0f;
        direction.Normalize();
        float verticaloffset = -1f;
        Vector3 targetPosition = loginPasswordField.transform.position + direction * 0.5f + Vector3.up * verticaloffset;
        NonNativeKeyboard.Instance.SetScaleSizeValues(1.8f, 1.8f, 0f, 10f);
        NonNativeKeyboard.Instance.RepositionKeyboard(targetPosition);

    }

    private void OnRegisterUsernameFieldSelected()
    {
        NonNativeKeyboard.Instance.InputField = registerUsernameField;
        NonNativeKeyboard.Instance.PresentKeyboard();
        Vector3 direction = registerUsernameField.transform.forward;
        direction.y = 0f;
        direction.Normalize();
        float verticaloffset = -1f;
        Vector3 targetPosition = registerUsernameField.transform.position + direction * 0.5f + Vector3.up * verticaloffset;
        NonNativeKeyboard.Instance.SetScaleSizeValues(1.8f, 1.8f, 0f, 10f);
        NonNativeKeyboard.Instance.RepositionKeyboard(targetPosition);
    }

    private void OnRegisterPasswordFieldSelected()
    {
        NonNativeKeyboard.Instance.InputField = registerPasswordField;
        NonNativeKeyboard.Instance.PresentKeyboard();
        Vector3 direction = registerPasswordField.transform.forward;
        direction.y = 0f;
        direction.Normalize();
        float verticaloffset = -1f;
        Vector3 targetPosition = registerPasswordField.transform.position + direction * 0.5f + Vector3.up * verticaloffset;
        NonNativeKeyboard.Instance.SetScaleSizeValues(1.8f, 1.8f, 0f, 10f);
        NonNativeKeyboard.Instance.RepositionKeyboard(targetPosition);
    }

    private void OnRegisterPasswordConfirmFieldSelected()
    {
        NonNativeKeyboard.Instance.InputField = registerPasswordConfirmField;
        NonNativeKeyboard.Instance.PresentKeyboard();
        Vector3 direction = registerPasswordConfirmField.transform.forward;
        direction.y = 0f;
        direction.Normalize();
        float verticaloffset = -1f;
        Vector3 targetPosition = registerPasswordConfirmField.transform.position + direction * 0.5f + Vector3.up * verticaloffset;
        NonNativeKeyboard.Instance.SetScaleSizeValues(1.8f, 1.8f, 0f, 10f);
        NonNativeKeyboard.Instance.RepositionKeyboard(targetPosition);
    }

    private void OnAdminMatriculaFieldSelected()
    {
        NonNativeKeyboard.Instance.InputField = adminMatriculaField;
        NonNativeKeyboard.Instance.PresentKeyboard();
        Vector3 direction = adminMatriculaField.transform.forward;
        direction.y = 0f;
        direction.Normalize();
        float verticaloffset = -1f;
        Vector3 targetPosition = adminMatriculaField.transform.position + direction * 0.5f + Vector3.up * verticaloffset;
        NonNativeKeyboard.Instance.SetScaleSizeValues(1.8f, 1.8f, 0f, 10f);
        NonNativeKeyboard.Instance.RepositionKeyboard(targetPosition);
    }

    private void OnAdminEmailFieldSelected()
    {
        NonNativeKeyboard.Instance.InputField = adminEmailField;
        NonNativeKeyboard.Instance.PresentKeyboard();
        Vector3 direction = adminEmailField.transform.forward;
        direction.y = 0f;
        direction.Normalize();
        float verticaloffset = -1f;
        Vector3 targetPosition = adminEmailField.transform.position + direction * 0.5f + Vector3.up * verticaloffset;
        NonNativeKeyboard.Instance.SetScaleSizeValues(1.8f, 1.8f, 0f, 10f);
        NonNativeKeyboard.Instance.RepositionKeyboard(targetPosition);
    }

    private void OnAdminPasswordFieldSelected()
    {
        NonNativeKeyboard.Instance.InputField = adminPasswordField;
        NonNativeKeyboard.Instance.PresentKeyboard();
        Vector3 direction = adminPasswordField.transform.forward;
        direction.y = 0f;
        direction.Normalize();
        float verticaloffset = -1f;
        Vector3 targetPosition = adminPasswordField.transform.position + direction * 0.5f + Vector3.up * verticaloffset;
        NonNativeKeyboard.Instance.SetScaleSizeValues(1.8f, 1.8f, 0f, 10f);
        NonNativeKeyboard.Instance.RepositionKeyboard(targetPosition);
    }

    private void ShowWorldStateMessage(WorldState worldState, StateData stateData)
    {
        if (stateData.stateMsgToShow)
        {
            panelObject.SetActive(true);
            msgText.text = stateData.stateMsg;
            if (stateData.stateMsgDuration < 1f)
            {
                return;
            }

            StartCoroutine(CountDownToHideMessage(stateData.stateMsgDuration));
        }
        else
        {
            panelObject.SetActive(false);
        }
    }

    private void ShowScoreText(string text)
    {
        scoreText.gameObject.SetActive(true);

        scoreText.text = "Pontuação:\n" + text;
    }


    private IEnumerator CountDownToHideMessage(float messageDuration)
    {
        yield return new WaitForSeconds(messageDuration);
        panelObject.SetActive(false);
    }
}
