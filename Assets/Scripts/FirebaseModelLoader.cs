using UnityEngine;
using Firebase.Storage;
using System.IO;
using System.Threading.Tasks;

public class FirebaseModelLoader : MonoBehaviour
{

    FirebaseStorage m_Storage;

    StorageReference m_StorageRef;

    readonly string modelsDirectoryPath = $"{Application.persistentDataPath}/models";

    protected void Awake()
    {
        m_Storage = FirebaseStorage.DefaultInstance;
        m_StorageRef = FirebaseStorage.DefaultInstance.RootReference;
    }
}
