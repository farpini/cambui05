using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Threading;
using System;


public class AuthManager : MonoBehaviour
{
    //Firebase variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;    
    public FirebaseUser User;

    //Login variables
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;
    public TMP_Text confirmLoginText;

    //Register variables
    [Header("Register")]
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;

    public UIManager UIManager;

    void Awake()
    {
        //Check that all of the necessary dependencies for Firebase are present on the system
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                //If they are avalible Initialize Firebase
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Não foi possível resolver todas as dependências do Firebase: " + dependencyStatus);
            }
        });
    }

    private void InitializeFirebase()
    {
        Debug.Log("Configurando o Firebase Auth");
        //Set the authentication instance object
        auth = FirebaseAuth.DefaultInstance;
    }

    //Function for the login button
    public void LoginButton()
    {
        //Call the login coroutine passing the email and password
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }
    //Function for the register button
    public void RegisterButton()
    {
        //Call the register coroutine passing the email, password, and username
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }

    private IEnumerator Login(string _email, string _password)
    {
        //Call the Firebase auth signin function passing the email and password
        Task<AuthResult> LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Falha ao registrar tarefa com {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "O Login falhou!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Está faltando o email!";
                    break;
                case AuthError.MissingPassword:
                    message = "Está faltando a senha!";
                    break;
                case AuthError.WrongPassword:
                    message = "A senha está errada!";
                    break;
                case AuthError.InvalidEmail:
                    message = "O email está inválido!";
                    break;
                case AuthError.UserNotFound:
                    message = "A conta não existe!";
                    break;
            }
            warningLoginText.text = message;
        }
        else
        {
            //User is now logged in
            //Now get the result
            User = LoginTask.Result.User;
            Debug.LogFormat("O usuário logou com sucesso {0} ({1})", User.DisplayName, User.Email);
            warningLoginText.text = "";
            confirmLoginText.text = "Logado!";
            UIManager.OpenWaypointTest();
        }
    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
        {
            //If the username field is blank show a warning
            warningRegisterText.text = "O nome de usuário está faltando";
        }
        else if(passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            //If the password does not match show a warning
            warningRegisterText.text = "A senha está diferente!";
        }
        else 
        {
            //Call the Firebase auth signin function passing the email and password
            Task<AuthResult> RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            //Wait until the task completes
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                //If there are errors handle them
                Debug.LogWarning(message: $"Falha ao registrar tarefa com {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "O registro falhou!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Está faltando o email!";
                        break;
                    case AuthError.MissingPassword:
                        message = "Está faltando a senha!";
                        break;
                    case AuthError.WeakPassword:
                        message = "A senha está fraca!";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "O email já está em uso!";
                        break;
                }
                warningRegisterText.text = message;
            }
            else
            {
                //User has now been created
                //Now get the result
                User = RegisterTask.Result.User;

                if (User != null)
                {
                    //Create a user profile and set the username
                    UserProfile profile = new UserProfile{DisplayName = _username};

                    //Call the Firebase auth update user profile function passing the profile with the username
                    Task ProfileTask = User.UpdateUserProfileAsync(profile);
                    //Wait until the task completes
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        //If there are errors handle them
                        Debug.LogWarning(message: $"Falha ao registrar tarefa com {ProfileTask.Exception}");
                        FirebaseException firebaseEx = ProfileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                        warningRegisterText.text = "Falha na configuração do nome de usuário!";
                    }
                    else
                    {
                        //Username is now set
                        //Now return to login screen
                        confirmLoginText.text = "Usuário criado!";
                        UIManager.instance.LoginScreen();
                        warningRegisterText.text = "";
                    }
                }
            }
        }
    }
}