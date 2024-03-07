using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using System.Threading.Tasks;
using System;
using UnityEngine.Analytics;
using Newtonsoft.Json;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager instance;

    //Firebase variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth authAPI;    
    public FirebaseUser userAPI;

    private string currentUserId = "";

    // Eventos
    public Action<string, string, string, GenderType, ClientType> OnNewRegister;

    public Action<string> OnLoginSuccess;
    public Action<string> OnLoginMissing;

    public Action<string, bool> OnLoginPrintResult;
    public Action<string, bool> OnUserRegisterPrintResult;
    public Action<string, bool> OnAdminRegisterPrintResult;

    public Action<string> OnUserRegisterSuccess;
    public Action<string> OnAdminRegisterSuccess;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
           
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
        authAPI = FirebaseAuth.DefaultInstance;
    }

    public void GetUserAttribute (string userId, UserAttribute userAttribute, Action<string> userAttributeCallback)
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("users/" + userId + "/atributos/" + userAttribute.ToString())
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError(task);
                }
                else if (task.IsCompleted)
                {
                    userAttributeCallback.Invoke(task.Result.Value.ToString());
                }
            });
    }

    public void SetUserAttribute<T> (string userId, UserAttribute userAttribute, T userValue)
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("users/" + userId + "/atributos/" + userAttribute.ToString())
            .SetValueAsync(userValue.ToString());
    }

    public void GetUser (string userId, Action<UserData> userCallback)
    {
        StartCoroutine(GetUserDictionary(userId, userCallback));
    }

    private IEnumerator GetUserDictionary (string userId, Action<UserData> userCallback)
    {
        var serverData = FirebaseDatabase.DefaultInstance.
            GetReference("users/" + userId).GetValueAsync();
        yield return new WaitUntil(predicate: () => serverData.IsCompleted);

        DataSnapshot snapshot = serverData.Result;
        string jsonData = snapshot.GetRawJsonValue();

        if (jsonData != null)
        {
            userCallback.Invoke(JsonConvert.DeserializeObject<UserData>(jsonData));
        }
        else
        {
            Debug.LogError("Cannot find userId " + userId + " in the database");
        }
    }

    public void SetUser (string userId, UserData userData)
    {
        var json = JsonConvert.SerializeObject(userData);
        FirebaseDatabase.DefaultInstance.
            GetReference("users/" + userId).SetRawJsonValueAsync(json);
    }

    public void OnLoginButtonClicked (string emailText, string passwordText)
    {
        //Call the login coroutine passing the email and password
        StartCoroutine(Login(emailText, passwordText));
    }

    public void OnRegisterButtonClicked (string userNameText, string passwordText, string passwordConfirmText, 
        GenderType userGenderType)
    {
        //Call the register coroutine passing the email, password, and username
        StartCoroutine(UserRegister(userNameText, passwordText, passwordConfirmText, userGenderType));
    }

    public void OnAdminRegisterButtonClicked (string emailText, string passwordText, string matricula, ClientType clientType)
    {
        StartCoroutine(AdminRegister(emailText, passwordText, matricula, clientType));
    }

    private IEnumerator Login (string _email, string _password)
    {
        //Call the Firebase authAPI signin function passing the email and password
        Task<AuthResult> LoginTask = authAPI.SignInWithEmailAndPasswordAsync(_email, _password);
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

            OnLoginPrintResult?.Invoke(message, false);
        }
        else
        {
            userAPI = LoginTask.Result.User;
            currentUserId = userAPI.UserId;

            GetUserAttribute (currentUserId, UserAttribute.genero, (string genero) =>
            {
                if (genero == GenderType.none.ToString())
                {
                    OnLoginMissing?.Invoke(currentUserId);
                }
                else
                {
                    var resultOutputMessage = "O usuário " + userAPI.DisplayName + " logou com sucesso.";
                    Debug.Log(resultOutputMessage);
                    OnLoginSuccess?.Invoke(currentUserId);
                }
            });
        }
    }

    private IEnumerator UserRegister (string _username, string _password, string _passwordConfirm, GenderType _genderType)
    {
        if (currentUserId == "")
        {
            OnUserRegisterPrintResult?.Invoke("UserId unknown", false);
        }
        else if (_username == "")
        {
            OnUserRegisterPrintResult?.Invoke("O nome de usuário está faltando", false);
        }
        else if (_password != _passwordConfirm)
        {
            OnUserRegisterPrintResult?.Invoke("A senha está diferente!", false);
        }
        else 
        {
            
            SetUserAttribute(currentUserId, UserAttribute.username, _username);
            SetUserAttribute(currentUserId, UserAttribute.genero, _genderType);

            // change the password in the authAPI here...

            FirebaseUser user = authAPI.CurrentUser;
            string newPassword = _password;
            if (user != null) {
                user.UpdatePasswordAsync(newPassword).ContinueWith(task => {
                    if (task.IsCanceled)
                   {
                        Debug.LogError("UpdatePasswordAsync foi cancelado.");
                        return;
                    }
                    if (task.IsFaulted)
                    {
                        Debug.LogError("UpdatePasswordAsync encontrou um erro: " + task.Exception);
                        return;
                    }

                    Debug.Log("Password atualizado com sucesso.");
                });
            }

            OnLoginPrintResult?.Invoke("Usuário criado!", true);
            OnUserRegisterSuccess?.Invoke(currentUserId);
        }

        yield return null;
    }


    private IEnumerator AdminRegister (string _email, string _password, string _matricula, ClientType _clientType)
    {
        //Call the Firebase auth signin function passing the email and password
        Task<AuthResult> RegisterTask = authAPI.CreateUserWithEmailAndPasswordAsync(_email, _password);
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

            OnAdminRegisterPrintResult?.Invoke(message, false);
        }
        else
        {
            //User has now been created
            //Now get the result
            var User = RegisterTask.Result.User;

            var username = "anonymous";

            if (User != null)
            {
                //Create a user profile and set the username
                UserProfile profile = new UserProfile { DisplayName = username };

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
                    OnAdminRegisterPrintResult?.Invoke("Falha na configuração do nome de usuário!", false);
                }
                else
                {
                    OnNewRegister?.Invoke(User.UserId, username, _matricula, GenderType.none, _clientType);
                    OnAdminRegisterPrintResult?.Invoke("Usuário criado!", true);
                }
            }
        }
    }
}