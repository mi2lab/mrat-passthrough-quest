using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Firebase;
using Firebase.Database;
using Firebase.Extensions;

using Firebase.Firestore;

public class DatabaseSync : MonoBehaviour
{
    [HideInInspector]
    public GameObject target;
    public float deltaTime = 1;
    [HideInInspector]
    public string personalId = null;
    //public GameObject syncIndicator;
    private bool synchronizing = false;
    private DatabaseReference reference;
    private Coroutine co;
    HeadPos localPos = new HeadPos();

    public bool useFirestore = false;

    private FirebaseFirestore fs;

    //private Dictionary<int, HeadPos> otherPos = new Dictionary<int, HeadPos>();
    //private LivePos otherPos = new LivePos();
    private bool downSynchronizing = false;
    private Coroutine downCo;
    [HideInInspector]
    public LiveDemonstrator demo;

    [HideInInspector]
    public AccountLogger logger;
    private bool loggerRunning = false;
    private Coroutine loggerCo;

    //public bool test_up_sync = false;
    //public bool test_down_sync = false;
    //public bool test_account_setup = false;

    [HideInInspector]
    public bool trackHands = false;
    [HideInInspector]
    public HandTracker handTracker;

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

                if (useFirestore)
                {
                    Debug.Log(localPos.ToDict());
                    DocumentReference fsRef = fs.Collection("livePos").Document(personalId);
                    fsRef.SetAsync(localPos.ToDict()).ContinueWithOnMainThread(task => {
                        Debug.Log("Added data to the alovelace document in the users collection.");
                    }); ;
                }
                else
                {
                    reference.Child("livePos").Child(personalId).Child("headPos").SetValueAsync(localPosJson);
                    reference.Child("livePos").Child(personalId).Child("useHandTrack").SetValueAsync(trackHands.ToString());
                    if (trackHands)
                    {
                        HandPos handPos = handTracker.GetHandPos();
                        //Debug.Log(handPos.joints.Count);
                        //Debug.Log(handPos.joints[5].pos.PosToVec());
                        handTracker.PrintHandPos();
                        string localHandPosJson = JsonUtility.ToJson(handPos);
                        Debug.Log(localHandPosJson);
                        reference.Child("livePos").Child(personalId).Child("handTrack").SetValueAsync(localHandPosJson);
                    }
                }

                //AccumulatePos(localPosJson);
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

    public void ToggleFirestore()
    {
        useFirestore = !useFirestore;
    }

    public void SetDeltaTime(float t)
    {
        deltaTime = t;
    }

    IEnumerator DownSyncCoroutine()
    {
        while (true)
        {
            if (useFirestore)
            {
                fs.Collection("livePos").GetSnapshotAsync().ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted)
                    {
                        QuerySnapshot snapshot = task.Result;
                        List<string> keyList = new List<string>();
                        foreach (DocumentSnapshot document in snapshot.Documents)
                        {
                            string localKey = document.Id;
                            Debug.Log(localKey);
                            if (localKey != personalId)
                            {
                                Dictionary<string, object> retrievedPosDict = document.ToDictionary();
                                //foreach (KeyValuePair<string, object> pair in retrievedPosDict)
                                //{
                                //    Debug.Log("Key: " + pair.Key);
                                //    Debug.Log("Val: " + retrievedPosDict[pair.Key]);
                                //}
                                HeadPos retrievedPos = new HeadPos(retrievedPosDict);
                                //Debug.Log(retrievedPos);
                                demo.UpdateItem(localKey, retrievedPos, deltaTime);
                                keyList.Add(localKey);
                            }
                        }
                        demo.UpdateDict(keyList);
                        Debug.Log("Read all data from the users collection.");
                    }
                    else if (task.IsFaulted)
                    {
                        Debug.Log("Read from Database failed");
                    }
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
                        foreach (DataSnapshot item in snapshot.Children)
                        {
                            string localKey = item.Key;
                            if (localKey != personalId)
                            {
                                HeadPos retrievedPos = JsonUtility.FromJson<HeadPos>(item.Child("headPos").GetValue(false).ToString());
                                if (item.Child("useHandTrack").GetValue(false).ToString() == "True" )
                                {
                                    HandPos retrievedHandPos = JsonUtility.FromJson<HandPos>(item.Child("handTrack").GetValue(false).ToString());
                                    //Debug.Log(retrievedHandPos.joints.Count);
                                    //Debug.Log(retrievedHandPos.joints[0].ToString());
                                    demo.UpdateItem(localKey, retrievedPos, deltaTime, true, retrievedHandPos);
                                }
                                else
                                {
                                    demo.UpdateItem(localKey, retrievedPos, deltaTime);
                                }
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
            personalId = localId;
            Debug.Log("Result: " + personalId);
        }
        loggerRunning = false;
        yield return null;
    }

    IEnumerator WaitLoginGenerate()
    {
        Debug.Log("Waiting for logger");
        while (logger.GetAccount() == null || logger.GetAccount() == "")
        {
            logger.GenerateUserName();
            yield return new WaitForSeconds(1);
        }
        Debug.Log("Logger finished");
        string localId = logger.GetAccount();
        personalId = localId;
        Debug.Log("Result: " + personalId);
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
            //Debug.Log("Try login");
            logger.CreateAccount();
            StartCoroutine(WaitLogin());
            loggerRunning = true;
        }
    }

    public void ToggleHandTrack()
    {
        trackHands = !trackHands;
    }

    // Start is called before the first frame update
    void Start()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        fs = FirebaseFirestore.DefaultInstance;
        //logger.GenerateUserName();
        StartCoroutine(WaitLoginGenerate());
    }

    public bool isUpSynchronizing()
    {
        return synchronizing;
    }

    public bool isDownSynchronizing()
    {
        return downSynchronizing;
    }

    // Update is called once per frame
    void Update()
    {
        //handTracker.trackEnabled = trackHands;
        handTracker.deltaTime = deltaTime;
    }

    void OnApplicationQuit()
    {
        reference.Child("livePos").Child(personalId).RemoveValueAsync();
        //reference.Child("livePosRecordings").RemoveValueAsync();
    }

}