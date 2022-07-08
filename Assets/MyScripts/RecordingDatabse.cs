using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Firebase;
using Firebase.Database;
using Firebase.Extensions;

[System.Serializable]
public class RecordingDict
{
    private Dictionary<string, HeadPosSeries> dict = new Dictionary<string, HeadPosSeries>();
    private List<string> keyList = new List<string>();

    public HeadPosSeries this[int id]
    {
        get
        {
            if (id < 0 || id >= keyList.Count)
            {
                return null;
            }
            if (!dict.ContainsKey(keyList[id]))
            {
                return null;
            }
            return dict[keyList[id]];
        }
    }

    public void Add(string s, HeadPosSeries recording)
    {
        keyList.Add(s);
        dict.Add(s, recording);
    }

    public int Count()
    {
        return keyList.Count;
    }

    public bool ContainsKey(string s)
    {
        return dict.ContainsKey(s);
    }
}

[System.Serializable]
public class HeadPosInfo
{
    public string id;
    public float deltaTime;
    public long createTime;
    public long endTime;

    public string ToKey()
    {
        return id + createTime.ToString();
    }
}

[System.Serializable]
public class HeadPosSeries
{
    public HeadPosInfo info = new HeadPosInfo();
    public List<HeadPos> headPosSeries = new List<HeadPos>();

    public bool useHandTrack = false;
    public List<HandPos> handPosSeries = new List<HandPos>();
}

[System.Serializable]
public class HandJoint
{
    public HeadPos pos = new HeadPos();
    public string part = "";

    public HandJoint()
    {
        pos = new HeadPos();
        part = "";
    }

    public HandJoint(Transform t, string p = "")
    {
        pos = new HeadPos(t);
        part = p;
    }
    
    public HandJoint(GameObject g, string p = "")
    {
        pos = new HeadPos(g);
        part = p;
    }

}

[System.Serializable]
public class HandPos
{
    public List<HandJoint> joints = new List<HandJoint>();

    public HandPos()
    {
    }

    public HandPos(List<HandJoint> j)
    {
        joints = j;
    }

    public void Add(List<HandJoint> j)
    {
        joints.AddRange(j);
    }

    public void Print()
    {
        Debug.Log("#" + joints.Count);
        foreach (HandJoint joint in joints)
        {
            Debug.Log(joint.pos.PosToVec());
        }

    }
}

[System.Serializable]
public class HeadPos
{
    public float posX;
    public float posY;
    public float posZ;
    public float rotX;
    public float rotY;
    public float rotZ;

    public HeadPos()
    {
        posX = 0;
        posY = 0;
        posZ = 0;
        rotX = 0;
        rotY = 0;
        rotZ = 0;
    }

    public HeadPos(GameObject g)
    {
        FromTransform(g.transform);
    }

    public HeadPos(Transform t)
    {
        FromTransform(t);
    }

    public HeadPos(Dictionary<string, object> d)
    {
        FromDictionary(d);
    }

    public void FromTransform(Transform t)
    {
        posX = t.position.x;
        posY = t.position.y;
        posZ = t.position.z;
        rotX = t.rotation.eulerAngles.x;
        rotY = t.rotation.eulerAngles.y;
        rotZ = t.rotation.eulerAngles.z;
    }

    public void FromDictionary(Dictionary<string, object> d)
    {
        posX = float.Parse(d["posX"].ToString());
        posY = float.Parse(d["posY"].ToString());
        posZ = float.Parse(d["posZ"].ToString());
        rotX = float.Parse(d["rotX"].ToString());
        rotY = float.Parse(d["rotY"].ToString());
        rotZ = float.Parse(d["rotZ"].ToString());
    }

    public Vector3 PosToVec()
    {
        return new Vector3(posX, posY, posZ);
    }

    public Vector3 RotToVec()
    {
        return new Vector3(rotX, rotY, rotZ);
    }

    public Quaternion RotToQuat()
    {
        return Quaternion.Euler(this.RotToVec());
    }

    public Dictionary<string, object> ToDict()
    {
        return new Dictionary<string, object>{
            { "posX", posX},
            { "posY", posY},
            { "posZ", posZ},
            { "rotX", rotX},
            { "rotY", rotY},
            { "rotZ", rotZ}
        };
    }

}
public class RecordingDatabse : MonoBehaviour
{
    [SerializeField] [HideInInspector] public RecordingDict headPosRecordings = new RecordingDict();
    public HashSet<string> recordingTags = new HashSet<string>();
    private DatabaseReference reference;

    public DatabaseSync database;

    public string GetId()
    {
        return database.GetId();
    }

    public void CreateOnlineItem(HeadPosInfo info, bool trackHand = false)
    {
        string infoJson = JsonUtility.ToJson(info);
        reference.Child("recordings").Child(info.ToKey()).Child("info").SetValueAsync(infoJson);
        reference.Child("recordings").Child(info.ToKey()).Child("trackHands").SetValueAsync(trackHand.ToString());
        reference.Child("recordings").Child(info.ToKey()).Child("finished").SetValueAsync("False");
    }

    public void InsertOnlinePos(HeadPosInfo info, HeadPos pos, bool trackHand = false, HandPos handPos = null)
    {
        string posJson = JsonUtility.ToJson(pos);
        string key = reference.Child("recordings").Child(info.ToKey()).Child("recordings").Push().Key;
        reference.Child("recordings").Child(info.ToKey()).Child("recordings").Child(key).Child("head").SetValueAsync(posJson);
        if (trackHand)
        {
            string handPosJson = JsonUtility.ToJson(handPos);
            reference.Child("recordings").Child(info.ToKey()).Child("recordings").Child(key).Child("hand").SetValueAsync(handPosJson);
        }
    }

    public void FinishOnlineItem(HeadPosInfo info)
    {
        string infoJson = JsonUtility.ToJson(info);
        reference.Child("recordings").Child(info.ToKey()).Child("info").SetValueAsync(infoJson);
        reference.Child("recordings").Child(info.ToKey()).Child("finished").SetValueAsync("True");
    }

    public void Save()
    {
        //SaveLocal();
        SaveOnline();
    }

    /*
    public void SaveLocal()
    {
        string localRecordings = JsonUtility.ToJson(headPosRecordings);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/HeadPosData.json", localRecordings);
    }
    */

    public void SaveOnline()
    {
        string localRecordings = JsonUtility.ToJson(headPosRecordings);
        reference.Child("recordings").SetValueAsync(localRecordings);
    }

    /*
    public void ReadLocal()
    {
        try
        {
            string headPosJson = System.IO.File.ReadAllText(Application.persistentDataPath + "/HeadPosData.json");
            headPosRecordings = JsonUtility.FromJson<HeadPosSeriesList>(headPosJson);
        }
        catch (System.IO.DirectoryNotFoundException dirEx)
        {
        }
    }
    */

    public void ReadOnline()
    {
        reference.Child("recordings")
            .GetValueAsync().ContinueWithOnMainThread(task => {
                if (task.IsCompleted)
                {
                    /*
                    if (recordingTags.Count != headPosRecordings.dict.Count)
                    {
                        recordingTags.Clear();
                        foreach (KeyValuePair<string, HeadPosSeries> pair in headPosRecordings.dict)
                        {
                            recordingTags.Add(pair.Key);
                        }
                    }
                    */
                    DataSnapshot snapshot = task.Result;
                    foreach (DataSnapshot recording in snapshot.Children)
                    {
                        if (!headPosRecordings.ContainsKey(recording.Key) && recording.Child("finished").GetValue(false).ToString() == "True")
                        {
                            HeadPosSeries localRecordings = new HeadPosSeries();
                            localRecordings.info = JsonUtility.FromJson<HeadPosInfo>(recording.Child("info").GetValue(false).ToString());
                            bool trackHands = recording.Child("trackHands").GetValue(false).ToString() == "True";
                            localRecordings.useHandTrack = trackHands;
                            if (trackHands)
                            {
                                foreach (DataSnapshot posItem in recording.Child("recordings").Children)
                                {
                                    HeadPos pos = JsonUtility.FromJson<HeadPos>(posItem.Child("head").GetValue(false).ToString());
                                    localRecordings.headPosSeries.Add(pos);
                                    if (posItem.Child("hand").GetValue(false).ToString() != "")
                                    {
                                        HandPos handPos = JsonUtility.FromJson<HandPos>(posItem.Child("hand").GetValue(false).ToString());
                                        localRecordings.handPosSeries.Add(handPos);
                                    }
                                    else
                                    {
                                        localRecordings.handPosSeries.Add(null);
                                    }
                                    
                                }
                            }
                            else
                            {
                                foreach (DataSnapshot posItem in recording.Child("recordings").Children)
                                {
                                    HeadPos pos = JsonUtility.FromJson<HeadPos>(posItem.Child("head").GetValue(false).ToString());
                                    localRecordings.headPosSeries.Add(pos);
                                }
                            }
                            
                            headPosRecordings.Add(recording.Key, localRecordings);
                            //recordingTags.Add(recording.Key);
                        }
                    }
                }
                else if (task.IsFaulted)
                {
                    Debug.Log("Read from Database failed");
                }
            });
    }

    /*
    public void DownloadAndUpdate()
    {
        reference.Child("recordings")
                .GetValueAsync().ContinueWithOnMainThread(task => {
                    if (task.IsCompleted)
                    {
                        DataSnapshot snapshot = task.Result;
                        System.IO.File.WriteAllText(Application.persistentDataPath + "/HeadPosData.json", snapshot.GetValue(false).ToString());
                        ReadLocal();
                    }
                    else if (task.IsFaulted)
                    {
                        Debug.Log("Download from Database failed");
                    }
                });
    }
    */
    public void Delete()
    {
        //DeleteLocal();
        DeleteOnline();
    }

    /*
    public void DeleteLocal()
    {
        headPosRecordings = new HeadPosSeriesList();
        SaveLocal();
    }
    */

    public void DeleteOnline()
    {
        headPosRecordings = new RecordingDict();
        reference.Child("recordings").SetValueAsync(null);
        //SaveOnline();
    }

    // Start is called before the first frame update
    void Start()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
