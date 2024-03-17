using System;
using System.Collections;
using System.Collections.Generic;
// using Firebase.Database;
// using Firebase.Extensions;
using JetBrains.Annotations;
using UnityEngine;


[Serializable]
public class DataToSave
{
    public string cloudAnchorId;

    public DataToSave(string cloudAnchorId)
    {
        this.cloudAnchorId = cloudAnchorId;
    }
}

public class FirebaseRealtimeDatabaseTest : MonoBehaviour
{
    // private const string ANCHOR_KEY = "ANCHOR_KEY";
    // private FirebaseDatabase m_Database;

    // DatabaseReference m_DBRef;

    // private void Awake()
    // {
    //     m_DBRef = FirebaseDatabase.DefaultInstance.RootReference;
    // }

    // private void Start()
    // {
    //     LoadDataFn();
    // }

    // public void SaveDataFn(string cloudAnchorId)
    // {
    //     var dts = new DataToSave(cloudAnchorId);
    //     string json = JsonUtility.ToJson(dts);
    //     m_DBRef.Child(ANCHOR_KEY).SetRawJsonValueAsync(json);
    //     //SetValueAsync();
    //     //SetRawJsonValueAsync(json); + JsonUtility.ToJson()
    //     //Push()
    //     //UpdateChildrenAsync();
    //     //RunTransaction();
    // }

    // public void LoadDataFn()
    // {
    //     StartCoroutine(LoadDataCoroutine());
    // }

    // public void ExampleListenForEvents()
    // {
    //     m_DBRef.ValueChanged += HandleValueChanged;
    // }

    // public void HandleValueChanged(object sender, ValueChangedEventArgs args)
    // {
    //     if (args.DatabaseError != null)
    //     {
    //         Debug.LogError(args.DatabaseError.Message);
    //         return;
    //     }
    //     // Do something
    // }

    // IEnumerator LoadDataCoroutine()
    // {
    //     var serverData = m_DBRef.Child(ANCHOR_KEY).GetValueAsync();
    //     yield return new WaitUntil(predicate: () => serverData.IsCompleted);

    //     DataSnapshot snapshot = serverData.Result;
    //     string jsonData = snapshot.GetRawJsonValue();

    //     if (jsonData != null)
    //     {
    //         var dts = JsonUtility.FromJson<DataToSave>(jsonData);
    //         Debug.Log(dts.cloudAnchorId);
    //     }
    //     else
    //     {
    //         Debug.LogError("No data");
    //     }
    // }
}
