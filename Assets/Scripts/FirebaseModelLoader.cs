using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Firebase;
using Firebase.Extensions;
using Firebase.Storage;
using GLTFast;
using System.IO;
using System;
using System.Threading;
using System.Threading.Tasks;

public class FirebaseModelLoader : MonoBehaviour
{

    private FirebaseStorage m_Storage;
    private StorageReference m_StorageRef;
    private StorageReference m_ModelRef;
    private string localUrl;
    private GameObject gameObj;
    private void Awake()
    {
        // FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        // {
        //     if (task.Exception != null)
        //     {
        //         Debug.LogError($"Failed to initialize Firebase with {task.Exception}");
        //         return;
        //     }

        //     OnFirebaseInitialized.Invoke();
        // });
        m_Storage = FirebaseStorage.DefaultInstance;
        m_StorageRef = FirebaseStorage.DefaultInstance.RootReference;
        localUrl = $"file://{Application.persistentDataPath}/coffee_table.glb";
    }

    private void Start()
    {

        //intialize storage reference
        m_ModelRef = m_StorageRef.Child("models/coffee_table.glb");

        Task task = m_ModelRef.GetFileAsync(localUrl,
            new StorageProgress<DownloadState>(state =>
            {
                Debug.Log(String.Format(
                    "Progress: {0} of {1} bytes transfered.",
                    state.BytesTransferred,
                    state.TotalByteCount
                ));
            }), CancellationToken.None);

        task.ContinueWithOnMainThread(resultTask =>
        {
            if (!resultTask.IsFaulted && !resultTask.IsCanceled)
            {
                Debug.Log("Download finished");
            }
        });
    }

    public async void Download()
    {
        var gltf = new GltfImport();
        bool success = await gltf.Load(localUrl);
        // task1.ContinueWithOnMainThread(resultTask =>
        // {
        //     if (!resultTask.IsFaulted && !resultTask.IsCanceled)
        //     {
        //         var newObject = new GameObject("glTF");
        //         gltf.InstantiateMainSceneAsync(newObject.transform).ContinueWith(t =>
        //         {
        //             if (t.Result)
        //             {
        //                 Debug.Log("Instantiated");
        //             }
        //         });
        //     }
        //     else
        //     {
        //         Debug.Log("Couldn't instantiate");
        //     }
        // });
        if (success)
        {
            var newObject = new GameObject("glTF");
            success = await gltf.InstantiateMainSceneAsync(newObject.transform);
            if (success)
            {
                Debug.Log("Instantiated");
                if (newObject.transform.childCount > 0)
                {
                    var created = newObject.transform.GetChild(0).gameObject;
                    newObject.AddComponent<Overlay3DMeasurement>();
                }
                else
                {
                    Debug.LogError("No child found!");
                }
            }
        }
    }
}
