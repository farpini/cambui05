using Microsoft.MixedReality.Toolkit.Experimental.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
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

    [Header("TopPanel")]
    public GameObject topPanelObject;
    public GameObject panelObject;
    public TMP_Text msgInstructionsText;
    public Button msgButton;
    public TMP_Text msgChatText;
    public Button msgSendButton;
    public TMP_InputField msgInputText;
    public TMP_InputField msgWriteChatText;
    public TMP_Dropdown msgDestinationDrop;
    public TMP_Dropdown msgPreDropdown;
    public GameObject msgChatWindowPanel;
    public GameObject msgChatListPanel;


    [Header("PraticeRoom")]
    public GameObject scoreObject;
    public TMP_Text scoreUsernameText;
    public TMP_Text scoreResultText;


    private Queue<string> chatMessages;
    private int chatMessageMax;

    private void Awake ()
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

        chatMessages = new Queue<string>();

        chatMessageMax = 8;

        OpenLoginUI();

        topPanelObject.SetActive(false);
        panelObject.SetActive(false);
        toggleMale.isOn = true;
        toggleFemale.isOn = false;
        toggleProfessor.isOn = true;
        toggleStudent.isOn = false;
    }

    private void Start ()
    {
        loginEmailField.onSelect.AddListener((string s) => OnLoginEmailFieldSelected());
        loginPasswordField.onSelect.AddListener((string s) => OnLoginPasswordFieldSelected());

        registerUsernameField.onSelect.AddListener((string s) => OnRegisterUsernameFieldSelected());
        registerPasswordField.onSelect.AddListener((string s) => OnRegisterPasswordFieldSelected());
        registerPasswordConfirmField.onSelect.AddListener((string s) => OnRegisterPasswordConfirmFieldSelected());

        adminMatriculaField.onSelect.AddListener((string s) => OnAdminMatriculaFieldSelected());
        adminEmailField.onSelect.AddListener((string s) => OnAdminEmailFieldSelected());
        adminPasswordField.onSelect.AddListener((string s) => OnAdminPasswordFieldSelected());
        msgWriteChatText.onSelect.AddListener((string s) => OnChatFieldSelected());

        loginButton.onClick.AddListener(() => OnLoginButtonClicked());
        loginRegisterButton.onClick.AddListener(() => OnLoginRegisterButtonClicked());

        registerButton.onClick.AddListener(() => OnRegisterButtonClicked());
        registerBackButton.onClick.AddListener(() => OnBackButtonClicked());

        adminRegisterButton.onClick.AddListener(() => OnAdminRegisterButtonClicked());
        adminBackButton.onClick.AddListener(() => OnBackButtonClicked());
        adminClearButton.onClick.AddListener(() => OnClearButtonClicked());

        msgButton.onClick.AddListener(() => OnMessageButtonClicked());
        msgSendButton.onClick.AddListener(() => OnMessageSendButtonClicked());
        msgPreDropdown.onValueChanged.AddListener((int value) => OnPreMsgSelectionChanged());

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
        UserManager.instance.OnDestinationMsgUsernamesChanged += UpdateDestinationMsgUsernames;
        UserManager.instance.OnReceivedMessage += OnMessageReceived;
    }

    private void OnDestroy ()
    {
        loginEmailField.onSelect.RemoveAllListeners();
        loginPasswordField.onSelect.RemoveAllListeners();

        registerUsernameField.onSelect.RemoveAllListeners();
        registerPasswordField.onSelect.RemoveAllListeners();
        registerPasswordConfirmField.onSelect.RemoveAllListeners();

        adminMatriculaField.onSelect.RemoveAllListeners();
        adminEmailField.onSelect.RemoveAllListeners();
        adminPasswordField.onSelect.RemoveAllListeners();
        msgWriteChatText.onSelect.RemoveAllListeners();

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
        NonNativeKeyboard.Instance.gameObject.SetActive(false);
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

    private void OnMessageButtonClicked ()
    {
        msgChatWindowPanel.gameObject.SetActive(!msgChatWindowPanel.activeSelf);
    }

    private void OnMessageSendButtonClicked ()
    {
        UserManager.instance.OnSendMsgButtonClicked(msgInputText.text, msgDestinationDrop.options[msgDestinationDrop.value].text,
            msgDestinationDrop.value);
        msgInputText.text = "";
        NonNativeKeyboard.Instance.gameObject.SetActive(false);
        msgPreDropdown.value = 0;
    }

    private void OnBackButtonClicked ()
    {
        OpenLoginUI();
    }

    private void OnClearButtonClicked ()
    {
        FirebaseManager.instance.ClearUserConnectedData();
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
        topPanelObject.SetActive(true);
    }

    private void OnLoginEmailFieldSelected ()
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

    private void OnLoginPasswordFieldSelected ()
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

    private void OnRegisterUsernameFieldSelected ()
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

    private void OnRegisterPasswordFieldSelected ()
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

    private void OnRegisterPasswordConfirmFieldSelected ()
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

    private void OnAdminMatriculaFieldSelected ()
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

    private void OnAdminEmailFieldSelected ()
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

    private void OnAdminPasswordFieldSelected ()
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

    private void OnChatFieldSelected ()
    {
        msgWriteChatText.text = "";
        NonNativeKeyboard.Instance.transform.SetParent(msgWriteChatText.transform);
        NonNativeKeyboard.Instance.InputField = msgWriteChatText;
        NonNativeKeyboard.Instance.PresentKeyboard();
        //Vector3 direction = msgWriteChatText.transform.forward;
        //direction.y = 0f;
        //direction.Normalize();
        //float verticaloffset = -1f;
        //Vector3 targetPosition = msgWriteChatText.transform.position + direction * 0.5f + Vector3.up * verticaloffset;
        //NonNativeKeyboard.Instance.SetScaleSizeValues(2f, 2f, 0f, 10f);
        //NonNativeKeyboard.Instance.RepositionKeyboard(targetPosition);
        var rectTrasnform = NonNativeKeyboard.Instance.GetComponent<RectTransform>();
        rectTrasnform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        rectTrasnform.localRotation = Quaternion.identity;
        rectTrasnform.localPosition = new Vector3(320f, -100f, 0f);
    }

    private void ShowWorldStateMessage (WorldState worldState, StateData stateData)
    {
        if (stateData.stateMsgToShow)
        {
            panelObject.SetActive(true);
            msgInstructionsText.text = stateData.stateMsg;
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

    private void ShowScoreText (string usernameText, string resultText)
    {
        scoreObject.SetActive(true);
        scoreUsernameText.text = usernameText;
        scoreResultText.text = resultText;
    }


    private IEnumerator CountDownToHideMessage (float messageDuration)
    {
        yield return new WaitForSeconds(messageDuration);
        panelObject.SetActive(false);
    }

    private void UpdateDestinationMsgUsernames (List<string> destinationOptions)
    {
        msgDestinationDrop.ClearOptions();
        msgDestinationDrop.AddOptions(destinationOptions);
        msgDestinationDrop.value = 0;
    }

    private void OnMessageReceived (string message)
    {
        if (chatMessages.Count == chatMessageMax)
        {
            chatMessages.Dequeue();
        }

        chatMessages.Enqueue(message);

        PrintChatMessages();

        /*
        var newMessageComponent = Instantiate(msgTemplate);
        newMessageComponent.SetMessage(message);
        newMessageComponent.transform.SetParent(msgChatListPanel.transform, false);
        newMessageComponent.transform.SetAsLastSibling();
        */
    }

    private void OnPreMsgSelectionChanged ()
    {
        var preMsgValue = msgPreDropdown.value;

        if (preMsgValue == 0)
        {
            msgInputText.text = "";
        }
        else
        {
            msgInputText.text = msgPreDropdown.options[preMsgValue].text;
        }
    }

    private void PrintChatMessages ()
    {
        string msg = "";

        Queue<string> auxQueue = new Queue<string>(chatMessages);

        while (auxQueue.Count > 0)
        {
            msg += auxQueue.Dequeue() + "\n";
        }

        msgChatText.text = msg;
    }
}
