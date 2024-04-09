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
using Newtonsoft.Json;
using System.Collections.Generic;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager instance;

    //Firebase variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth authAPI;
    public FirebaseUser userAPI;

    private string currentUserId = "";

    public Action<string> OnLoginSuccess;
    public Action<string> OnLoginMissing;

    public Action<string> OnUserRegisterSuccess;
    public Action<string> OnAdminRegisterSuccess;

    public Action<string, Color> OnLoginPrintResult;
    public Action<string, Color> OnUserRegisterPrintResult;
    public Action<string, Color> OnAdminRegisterPrintResult;


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

    public void GetUserRuntimeAttribute(string userId, UserRuntimeAttribute userAttribute, Action<string> userAttributeCallback)
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("usersConnected/" + userId + "/" + userAttribute.ToString())
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError(task + ": failed to get runtime attribute.");
                }
                else if (task.IsCompleted)
                {
                    userAttributeCallback.Invoke(task.Result.Value.ToString());
                }
            });
    }

    public void GetUserRuntimeAttributeWithUserIdReturned (string userId, UserRuntimeAttribute userAttribute, Action<string, string> userAttributeCallback)
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("usersConnected/" + userId + "/" + userAttribute.ToString())
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError(task + ": failed to get runtime attribute.");
                }
                else if (task.IsCompleted)
                {
                    userAttributeCallback.Invoke(task.Result.Value.ToString(), userId);
                }
            });
    }

    public void SetUserRuntimeAttribute<T>(string userId, UserRuntimeAttribute userAttribute, T userValue)
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("usersConnected/" + userId + "/" + userAttribute.ToString())
            .SetValueAsync(userValue.ToString()).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError(task + ": failed to set runtime attribute");
                }
            });
    }

    private void GetUserRegisterAttribute(string userId, UserRegisterAttribute userAttribute, Action<string> userAttributeCallback)
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("users/" + userId + "/" + userAttribute.ToString())
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError(task + ": failed to get runtime attribute.");
                }
                else if (task.IsCompleted)
                {
                    userAttributeCallback.Invoke(task.Result.Value.ToString());
                }
            });
    }

    public void SetUserRegisterAttribute<T>(string userId, UserRegisterAttribute userAttribute, T userValue)
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("users/" + userId + "/" + userAttribute.ToString())
            .SetValueAsync(userValue.ToString()).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError(task + ": failed to set runtime attribute");
                }
            });
    }

    public bool RegisterUserRuntimeAttributeChangeValueEvent(string userId, UserRuntimeAttribute userAttribute,
        EventHandler<ValueChangedEventArgs> callback)
    {
        var dataRef = FirebaseDatabase.DefaultInstance
            .GetReference("usersConnected/" + userId + "/" + userAttribute.ToString());

        if (dataRef == null)
        {
            return false;
        }

        dataRef.ValueChanged += callback;
        return true;
    }

    public bool UnregisterUserRuntimeAttributeChangeValueEvent(string userId, UserRuntimeAttribute userAttribute,
        EventHandler<ValueChangedEventArgs> callback)
    {
        var dataRef = FirebaseDatabase.DefaultInstance
            .GetReference("usersConnected/" + userId + "/" + userAttribute.ToString());

        if (dataRef == null)
        {
            return false;
        }

        dataRef.ValueChanged -= callback;
        return true;
    }

    public void OnLoginButtonClicked(string emailText, string passwordText)
    {
        //Call the login coroutine passing the email and password
        StartCoroutine(Login(emailText, passwordText));
    }

    public void OnRegisterButtonClicked(string userNameText, string passwordText, string passwordConfirmText,
        ClientGender userGenderType)
    {
        //Call the register coroutine passing the email, password, and username
        StartCoroutine(UserRegister(userNameText, passwordText, passwordConfirmText, userGenderType));
    }

    public void OnAdminRegisterButtonClicked(string emailText, string passwordText, string matricula, ClientType clientType)
    {
        StartCoroutine(AdminRegister(emailText, passwordText, matricula, clientType));
    }

    private IEnumerator Login(string _email, string _password)
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

            OnLoginPrintResult?.Invoke(message, Color.red);
        }
        else
        {
            userAPI = LoginTask.Result.User;
            currentUserId = userAPI.UserId;

            GetUserRegisterAttribute(currentUserId, UserRegisterAttribute.genero, (string genero) =>
            {
                if (genero == ClientGender.none.ToString())
                {
                    OnLoginMissing?.Invoke(currentUserId);
                }
                else
                {
                    //var resultOutputMessage = "O usuário " + userAPI.DisplayName + " logou com sucesso.";
                    //Debug.Log(resultOutputMessage);
                    OnLoginSuccess?.Invoke(currentUserId);
                }
            });
        }
    }

    private IEnumerator UserRegister(string _username, string _password, string _passwordConfirm, ClientGender _genderType)
    {
        if (currentUserId == "")
        {
            OnUserRegisterPrintResult?.Invoke("UserId unknown", Color.red);
        }
        else if (_username == "")
        {
            OnUserRegisterPrintResult?.Invoke("O nome de usuário está faltando", Color.red);
        }
        else if (_password != _passwordConfirm)
        {
            OnUserRegisterPrintResult?.Invoke("A senha está diferente!", Color.red);
        }
        else
        {
            SetUserRegisterAttribute(currentUserId, UserRegisterAttribute.username, _username);
            SetUserRegisterAttribute(currentUserId, UserRegisterAttribute.genero, _genderType);

            // change the password in the authAPI here...

            FirebaseUser user = authAPI.CurrentUser;
            string newPassword = _password;
            if (user != null)
            {

                OnLoginPrintResult?.Invoke("Redefinindo senha...", Color.white);

                var taskStatus = user.UpdatePasswordAsync(newPassword).ContinueWithOnMainThread(task => {

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
                });


                yield return new WaitUntil(predicate: () => taskStatus.IsCompleted);
                Debug.Log("Password atualizado com sucesso.");
            }

            OnLoginPrintResult?.Invoke("Usuário criado!", Color.green);
            OnUserRegisterSuccess?.Invoke(currentUserId);
        }
    }

    private IEnumerator AdminRegister(string _email, string _password, string _matricula, ClientType _clientType)
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

            OnAdminRegisterPrintResult?.Invoke(message, Color.red);
        }
        else
        {
            //User has now been created
            //Now get the result
            var User = RegisterTask.Result.User;

            var username = "anonymous";

            if (User != null)
            {
                OnAdminRegisterPrintResult?.Invoke("Criando usuário...", Color.white);

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
                    OnAdminRegisterPrintResult?.Invoke("Falha na configuração do nome de usuário!", Color.red);
                }
                else
                {
                    CreateNewUser(User.UserId, username, _matricula, ClientGender.none, _clientType);
                }
            }
        }
    }

    private void CreateNewUser(string _userId, string _userName, string _matricula, ClientGender _clientGender, ClientType _clientType)
    {
        var newUserRegisterData = new UserRegisterData(_userName, _matricula, _clientGender.ToString(), _clientType.ToString());

        var json = JsonConvert.SerializeObject(newUserRegisterData);

        FirebaseDatabase.DefaultInstance
            .GetReference("users/" + _userId + "/")
            .SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError(task + ": failed to set new user to the database");
                }
                else if (task.IsCompleted)
                {
                    OnAdminRegisterPrintResult?.Invoke("Usuário criado!", Color.green);
                }
            });
    }

    public IEnumerator GetUserRegisterData(string _userId, Action<string, UserRegisterData> _userRegisterDataCallback)
    {
        var task = FirebaseDatabase.DefaultInstance
           .GetReference("users/" + _userId + "/")
           .GetValueAsync();

        yield return new WaitUntil(predicate: () => task.IsCompleted);

        DataSnapshot snapshot = task.Result;
        string json = snapshot.GetRawJsonValue();
        _userRegisterDataCallback.Invoke(_userId, JsonConvert.DeserializeObject<UserRegisterData>(json));
    }

    public IEnumerator GetAllUsersRuntimeData(Action<Dictionary<string, UserRuntimeData>> _usersRuntimeData)
    {
        var task = FirebaseDatabase.DefaultInstance
           .GetReference("usersConnected/")
           .GetValueAsync();

        yield return new WaitUntil(predicate: () => task.IsCompleted);

        DataSnapshot snapshot = task.Result;
        string json = snapshot.GetRawJsonValue();
        _usersRuntimeData.Invoke(JsonConvert.DeserializeObject<Dictionary<string, UserRuntimeData>>(json));
    }

    public void SetUserRuntimeData(string _userId, UserRuntimeData _userRuntimeData, Action<string> _userRuntimeDataCallback)
    {
        var json = JsonConvert.SerializeObject(_userRuntimeData);

        FirebaseDatabase.DefaultInstance
           .GetReference("usersConnected/" + _userId + "/")
           .SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
           {
               if (task.IsFaulted)
               {
                   Debug.LogError(task + ": failed to set the user runtime data");
               }
               else if (task.IsCompleted)
               {
                   _userRuntimeDataCallback.Invoke(_userId);
               }
           });
    }

    public void SetWorldStateData (WorldState _stateType, StateData[] _worldStateData)
    {
        var json = JsonConvert.SerializeObject(_worldStateData);

        FirebaseDatabase.DefaultInstance
           .GetReference("worldStateData/" + _stateType.ToString() + "/")
           .SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
           {
               if (task.IsFaulted)
               {
                   Debug.LogError(task + ": failed to set the user runtime data");
               }
               else if (task.IsCompleted)
               {
                   //_userRuntimeDataCallback.Invoke(_userId);
               }
           });
    }

    public IEnumerator GetAllWorldStateData (Action<Dictionary<string, StateData[]>> _worldStateData)
    {
        var task = FirebaseDatabase.DefaultInstance
           .GetReference("worldStateData/")
           .GetValueAsync();

        yield return new WaitUntil(predicate: () => task.IsCompleted);

        DataSnapshot snapshot = task.Result;
        string json = snapshot.GetRawJsonValue();
        _worldStateData.Invoke(JsonConvert.DeserializeObject<Dictionary<string, StateData[]>>(json));
    }

    public void GetUsersConnectedCount(Action<int> usersConnectedCountCallback)
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("usersConnectedCount")
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError(task + ": failed to get runtime attribute.");
                }
                else if (task.IsCompleted)
                {
                    usersConnectedCountCallback.Invoke(int.Parse(task.Result.Value.ToString()));
                }
            });
    }

    public void SetUsersConnectedCount(int count)
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("usersConnectedCount")
            .SetValueAsync(count.ToString()).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError(task + ": failed to set runtime attribute");
                }
            });
    }

    public bool RegisterUsersConnectedCountChangeValueEvent(EventHandler<ValueChangedEventArgs> callback)
    {
        var dataRef = FirebaseDatabase.DefaultInstance
            .GetReference("usersConnectedCount");

        if (dataRef == null)
        {
            return false;
        }

        dataRef.ValueChanged += callback;
        return true;
    }

    public bool UnregisterUsersConnectedCountChangeValueEvent(EventHandler<ValueChangedEventArgs> callback)
    {
        var dataRef = FirebaseDatabase.DefaultInstance
            .GetReference("usersConnectedCount");

        if (dataRef == null)
        {
            return false;
        }

        dataRef.ValueChanged -= callback;
        return true;
    }

    /*public void SetUsersLoggedFlag(bool flagValue)
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("usersLoggedFlag")
            .SetValueAsync(flagValue).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError(task + ": failed to set users logged flag");
                }
            });
    }*/

    /*public void GetUsersLoggedFlag(Action<bool> usersLoggedFlagCallback)
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("usersLoggedFlag")
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError(task + ": failed to get users logged flag.");
                }
                else if (task.IsCompleted)
                {
                    usersLoggedFlagCallback.Invoke(bool.Parse(task.Result.Value.ToString()));
                }
            });
    }*/

    /*public bool RegisterUsersLoggedFlagChangeValueEvent(EventHandler<ValueChangedEventArgs> callback)
    {
        var dataRef = FirebaseDatabase.DefaultInstance
            .GetReference("usersLoggedFlag");

        if (dataRef == null)
        {
            return false;
        }

        dataRef.ValueChanged += callback;
        return true;
    }

    public bool UnregisterUsersLoggedFlagChangeValueEvent(EventHandler<ValueChangedEventArgs> callback)
    {
        var dataRef = FirebaseDatabase.DefaultInstance
            .GetReference("usersLoggedFlag");

        if (dataRef == null)
        {
            return false;
        }

        dataRef.ValueChanged -= callback;
        return true;
    }*/

    public bool RegisterWorldStateChangeValueEvent(EventHandler<ValueChangedEventArgs> callback)
    {
        var dataRef = FirebaseDatabase.DefaultInstance
            .GetReference("worldState");

        if (dataRef == null)
        {
            return false;
        }

        dataRef.ValueChanged += callback;
        return true;
    }

    public bool UnregisterWorldStateChangeValueEvent(EventHandler<ValueChangedEventArgs> callback)
    {
        var dataRef = FirebaseDatabase.DefaultInstance
            .GetReference("worldState");

        if (dataRef == null)
        {
            return false;
        }

        dataRef.ValueChanged -= callback;
        return true;
    }

    public bool RegisterWorldStateArgChangeValueEvent(EventHandler<ValueChangedEventArgs> callback)
    {
        var dataRef = FirebaseDatabase.DefaultInstance
            .GetReference("worldStateArg");

        if (dataRef == null)
        {
            return false;
        }

        dataRef.ValueChanged += callback;
        return true;
    }

    public bool UnregisterWorldStateArgChangeValueEvent(EventHandler<ValueChangedEventArgs> callback)
    {
        var dataRef = FirebaseDatabase.DefaultInstance
            .GetReference("worldStateArg");

        if (dataRef == null)
        {
            return false;
        }

        dataRef.ValueChanged -= callback;
        return true;
    }

    public void SetWorldState(WorldState worldState)
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("worldState")
            .SetValueAsync(worldState.ToString()).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError(task + ": failed to set runtime attribute");
                }
            });
    }

    public void SetWorldStateArg(int arg)
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("worldStateArg")
            .SetValueAsync(arg.ToString()).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError(task + ": failed to set runtime attribute");
                }
            });
    }
}