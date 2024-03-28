using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Storage;
using GLTFast;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;

public class FirebaseManager : MonoBehaviour
{
    FirebaseAuth m_FirebaseAuth;

    public FirebaseAuth firebaseAuth => m_FirebaseAuth;

    FirebaseUser m_FirebaseUser;

    public FirebaseUser firebaseUser => m_FirebaseUser;

    public UnityEvent OnFirebaseInitialized = new UnityEvent();

    FirebaseDatabase m_FirebaseDatabase;

    public FirebaseDatabase firebaseDatabase => m_FirebaseDatabase;

    DatabaseReference m_DatabaseRootRef;

    public DatabaseReference databaseRoofReference => m_DatabaseRootRef;

    FirebaseStorage m_FirebaseStorage;

    public FirebaseStorage firebaseStorage => m_FirebaseStorage;

    StorageReference m_StorageRef;

    public StorageReference storageReference => m_StorageRef;

    private string modelsDirectoryPath;

    private void Awake()
    {
        StartCoroutine(CheckAndFixDependenciesAsync());
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
        m_FirebaseAuth.StateChanged -= AuthStateChanged;
    }

    IEnumerator CheckAndFixDependenciesAsync()
    {
        var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();

        yield return new WaitUntil(() => dependencyTask.IsCompleted);

        DependencyStatus dependencyStatus = dependencyTask.Result;

        if (dependencyStatus == DependencyStatus.Available)
        {
            m_FirebaseAuth = FirebaseAuth.DefaultInstance;

            m_FirebaseDatabase = FirebaseDatabase.DefaultInstance;
            m_DatabaseRootRef = FirebaseDatabase.DefaultInstance.RootReference;

            m_FirebaseStorage = FirebaseStorage.DefaultInstance;
            m_StorageRef = FirebaseStorage.DefaultInstance.RootReference;
            modelsDirectoryPath = $"{Application.persistentDataPath}/models";

            StartCoroutine(CheckAutoLoginCoroutine());

            m_FirebaseAuth.StateChanged += AuthStateChanged;
            AuthStateChanged(this, null);

            OnFirebaseInitialized.Invoke();
        }
        else
        {
            Debug.LogError("Could not resolve all firebase dependencies: " + dependencyStatus);
        }
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (m_FirebaseAuth.CurrentUser != m_FirebaseUser)
        {
            bool signedIn = m_FirebaseUser != m_FirebaseAuth.CurrentUser && m_FirebaseAuth.CurrentUser != null;

            if (!signedIn && m_FirebaseUser != null)
            {
                Debug.Log("Signed out " + m_FirebaseUser.UserId);
            }

            m_FirebaseUser = m_FirebaseAuth.CurrentUser;

            if (signedIn)
            {
                Debug.Log("Signed in " + m_FirebaseUser.UserId);
            }
        }
    }

    IEnumerator CheckAutoLoginCoroutine()
    {
        yield return new WaitForEndOfFrame();
        if (m_FirebaseUser != null)
        {
            var reloadTask = m_FirebaseUser.ReloadAsync();

            yield return new WaitUntil(() => reloadTask.IsCompleted);

            AutoLogin();
        }
        else
        {
            Login();
        }
    }

    private void AutoLogin()
    {
        if (m_FirebaseUser != null)
        {
            //Change scene
        }
        else
        {
            Login();
        }
    }

    public void Login()
    {
        StartCoroutine(LoginUserCoroutine("antonylouis0x@gmail.com", "Loclienhadonganh2002"));
    }

    public void Register()
    {

    }

    IEnumerator LoginUserCoroutine(string email, string password)
    {
        var loginTask = m_FirebaseAuth.SignInWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {

            FirebaseException exception = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError authError = (AuthError)exception.ErrorCode;

            string failedMessage = "Login failed! Because ";

            switch (authError)
            {
                case AuthError.InvalidEmail:
                    failedMessage += "Email is invalid";
                    break;
                case AuthError.WrongPassword:
                    failedMessage += "Wrong Password";
                    break;
                case AuthError.MissingEmail:
                    failedMessage += "Email is missing";
                    break;
                case AuthError.MissingPassword:
                    failedMessage = "Password is missing";
                    break;
                default:
                    failedMessage = "Login failed";
                    break;
            }

            Debug.Log(failedMessage);
        }
        else
        {
            m_FirebaseUser = loginTask.Result.User;

            Debug.Log($"{loginTask.Result.User.Email} logged in successfully");
        }
    }

    IEnumerator RegisterUserCoroutine(string email, string password)
    {
        var registerTask = m_FirebaseAuth.CreateUserWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(() => registerTask.IsCompleted);

        if (registerTask.Exception != null)
        {
            Debug.LogError($"Failed to register task with {registerTask.Exception}");
            yield break;
        }
        else
        {
            Debug.Log($"Registered successfully {registerTask.Result.User.Email}");
        }
    }

    public async Task<GameObject> LoadModelAsync(string modelPath)
    {
        var localUrl = $"file://{Application.persistentDataPath}/{modelPath}";

        if (!Directory.Exists(modelsDirectoryPath))
        {
            Directory.CreateDirectory(modelsDirectoryPath);
        }

        var gtlf = new GltfImport();
        var success = await gtlf.Load(localUrl);
        if (success)
        {
            // Load model from local storage
            var newParent = new GameObject("glTF");
            success = await gtlf.InstantiateMainSceneAsync(newParent.transform);
            if (success)
            {
                Debug.Log("Instantiated");
                if (newParent.transform.childCount > 0)
                {
                    var created = newParent.transform.GetChild(0).gameObject;
                    return created;
                }
                else
                {
                    Debug.LogError("No child found!");
                }
            }
        }
        else
        {
            // Download model from firebase to local storage then load it
            var modelRef = m_StorageRef.Child(modelPath);

            var downloadTask = modelRef.GetFileAsync(localUrl);
            await downloadTask;

            if (!downloadTask.IsFaulted && !downloadTask.IsCanceled)
            {
                Debug.Log("Download success");

                //Load model from local storage
                success = await gtlf.Load(localUrl);

                if (success)
                {
                    var newParent = new GameObject("glTF");
                    success = await gtlf.InstantiateMainSceneAsync(newParent.transform);
                    if (success)
                    {
                        Debug.Log("Instantiated");
                        if (newParent.transform.childCount > 0)
                        {
                            var created = newParent.transform.GetChild(0).gameObject;
                            return created;
                        }
                        else
                        {
                            Debug.LogError("No child found!");
                        }
                    }
                }
            }
        }
        return null;
    }
}
