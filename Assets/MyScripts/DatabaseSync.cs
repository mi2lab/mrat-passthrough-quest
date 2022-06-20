using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Firebase;
using Firebase.Database;
using Firebase.Extensions;

using Firebase.Firestore;

/*
[System.Serializable]
public class LivePos
{
    private Dictionary<int, HeadPos> headPosList = new Dictionary<int, HeadPos>();

    public void Add(int id)
    {
        if (!headPosList.ContainsKey(id))
        {
            headPosList.Add(id, new HeadPos());
        }
    }

    public void Update(int id, HeadPos pos)
    {
        if (headPosList.ContainsKey(id))
        {
            headPosList[id] = pos;
        }
    }

    public void Update(Dictionary<int, HeadPos> dict)
    {
        foreach (KeyValuePair<int, HeadPos> pos in dict)
        {
            headPosList[pos.Key] = pos.Value;
        }
    }

    public void Remove(int id)
    {
        if (headPosList.ContainsKey(id))
        {
            headPosList.Remove(id);
        }
    }

    public HeadPos GetValue(int id)
    {
        if (headPosList.ContainsKey(id))
        {
            return headPosList[id];
        }
        else
        {
            return null;
        }
    }

    public List<int> GetKeys()
    {
        return new List<int>(headPosList.Keys);
    }

    public List<int> GetKeys(int id)
    {
        List<int> ret = new List<int>(headPosList.Keys);
        ret.Remove(id);
        return ret;
    }

    public void Clear()
    {
        headPosList.Clear();
    }
}
*/

public class DatabaseSync : MonoBehaviour
{
    public GameObject target;
    public float deltaTime = 1;
    public string personalId = null;
    //public GameObject syncIndicator;
    private bool synchronizing = false;
    private DatabaseReference reference;
    private Coroutine co;
    HeadPos localPos = new HeadPos();

    public bool useFirebase = false;

    private FirebaseFirestore fs;

    //private Dictionary<int, HeadPos> otherPos = new Dictionary<int, HeadPos>();
    //private LivePos otherPos = new LivePos();
    private bool downSynchronizing = false;
    private Coroutine downCo;
    public LiveDemonstrator demo;

    public AccountLogger logger;
    private bool loggerRunning = false;
    private Coroutine loggerCo;
    public string tmpPersonalId = "";

    public bool test_up_sync = false;
    public bool test_down_sync = false;
    public bool test_account_setup = false;

    void AccumulatePos(string pos)
    {
        string key = reference.Child("livePosRecordings").Child(personalId).Push().Key;
        reference.Child("livePosRecordings").Child(personalId).Child(key).Child("time").SetValueAsync(System.DateTime.Now.Ticks.ToString());
        reference.Child("livePosRecordings").Child(personalId).Child(key).Child("val").SetValueAsync(pos);
    }

    IEnumerator UpSyncCoroutine()
    {
        while (true)
        {
            try
            {
                localPos.FromTransform(target.transform);
                string localPosJson = JsonUtility.ToJson(localPos);

                if (useFirebase)
                {
                    Debug.Log(localPos.ToDict());
                    DocumentReference fsRef = fs.Collection("livePos").Document(personalId);
                    fsRef.SetAsync(localPos.ToDict()).ContinueWithOnMainThread(task => {
                        Debug.Log("Added data to the alovelace document in the users collection.");
                    }); ;
                }
                else
                {
                    reference.Child("livePos").Child(personalId).SetValueAsync(localPosJson);
                }

                AccumulatePos(localPosJson);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Error exception");
                Debug.LogWarning(e);
            }
            yield return new WaitForSeconds(deltaTime);
        }

        yield return null;
    }

    public void ToggleSync()
    {
        if (!synchronizing)
        {
            if (personalId != null)
            {
                reference.Child("livePos").Child(personalId).RemoveValueAsync();
            }
            co = StartCoroutine(UpSyncCoroutine());
        }
        else
        {
            StopCoroutine(co);
            reference.Child("livePos").Child(personalId).RemoveValueAsync();
        }
        synchronizing = !synchronizing;
    }

    IEnumerator DownSyncCoroutine()
    {
        while (true)
        {
            if (useFirebase)
            {
                fs.Collection("livePos").GetSnapshotAsync().ContinueWithOnMainThread(task =>
                {
                    QuerySnapshot snapshot = task.Result;
                    List<string> keyList = new List<string>();
                    foreach (DocumentSnapshot document in snapshot.Documents)
                    {
                        string localKey = document.Id;
                        if (localKey != personalId)
                        {
                            Dictionary<string, object> retrievedPosDict = document.ToDictionary();
                            HeadPos retrievedPos = new HeadPos(retrievedPosDict);
                            demo.UpdateItem(localKey, retrievedPos, deltaTime);
                            keyList.Add(localKey);
                        }
                    }
                    Debug.Log("Read all data from the users collection.");
                });
            }
            else
            {
                reference.Child("livePos")
                .GetValueAsync().ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted)
                    {
                        DataSnapshot snapshot = task.Result;

                        List<string> keyList = new List<string>();
                        foreach (DataSnapshot pos in snapshot.Children)
                        {
                            string localKey = pos.Key;
                            if (localKey != personalId)
                            {
                                HeadPos retrievedPos = JsonUtility.FromJson<HeadPos>(pos.GetValue(false).ToString());
                                demo.UpdateItem(localKey, retrievedPos, deltaTime);
                                keyList.Add(localKey);
                            }
                        }
                        demo.UpdateDict(keyList);
                    }
                    else if (task.IsFaulted)
                    {
                        Debug.Log("Read from Database failed");
                    }
                });
            }
            yield return new WaitForSeconds(deltaTime);
        }
        yield return null;
    }

    public void ToggleDownSync()
    {
        if (!downSynchronizing)
        {
            downCo = StartCoroutine(DownSyncCoroutine());
        }
        else
        {
            StopCoroutine(downCo);
            demo.Destroy();
        }
        downSynchronizing = !downSynchronizing;
    }

    void DeleteAccumulation()
    {
        reference.Child("livePosRecordings").Child(personalId).RemoveValueAsync();
    }

    public void ToggleDeleteAccumulation()
    {
        DeleteAccumulation();
    }

    IEnumerator WaitLogin()
    {
        Debug.Log("Waiting for logger");
        while (logger.GetAccount() == null)
        {
            yield return new WaitForSeconds(1);
        }
        Debug.Log("Logger finished");
        string localId = logger.GetAccount();
        if (localId != "")
        {
            //tmpPersonalId = localId;
            personalId = localId;
            Debug.Log("Result: " + personalId);
        }
        loggerRunning = false;
        yield return null;
    }

    public string GetId()
    {
        return personalId;
    }

    public void ToggleLogIn()
    {
        if (!loggerRunning)
        {
            Debug.Log("Try login");
            logger.CreateAccount();
            StartCoroutine(WaitLogin());
            loggerRunning = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        fs = FirebaseFirestore.DefaultInstance;
    }

    // Update is called once per frame
    void Update()
    {
        if (test_up_sync)
        {
            ToggleSync();
            test_up_sync = false;
        }
        if (test_down_sync)
        {
            ToggleDownSync();
            test_down_sync = false;
        }
        if (test_account_setup)
        {
            ToggleLogIn();
            test_account_setup = false;
        }
    }

    void OnApplicationQuit()
    {
        reference.Child("livePos").Child(personalId).RemoveValueAsync();
    }

}