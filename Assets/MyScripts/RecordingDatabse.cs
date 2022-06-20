using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Firebase;
using Firebase.Database;
using Firebase.Extensions;

[System.Serializable]
public class HeadPosSeriesList
{
    public List<HeadPosSeries> headPosList = new List<HeadPosSeries>();
}

[System.Serializable]
public class HeadPosInfo
{
    public string id;
    public float deltaTime;
    public long createTime;
    public long endTime;
}

[System.Serializable]
public class HeadPosSeries
{
    public HeadPosInfo info = new HeadPosInfo();
    public List<HeadPos> headPosSeries = new List<HeadPos>();
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
        posX = (float)d["posX"];
        posY = (float)d["posY"];
        posZ = (float)d["posZ"];
        rotX = (float)d["rotX"];
        rotY = (float)d["rotY"];
        rotZ = (float)d["rotZ"];
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
    [SerializeField] [HideInInspector] public HeadPosSeriesList headPosRecordings = new HeadPosSeriesList();
    public HashSet<long> recordingTags = new HashSet<long>();
    private DatabaseReference reference;

    public DatabaseSync database;

    public string GetId()
    {
        return database.GetId();
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
                    if (recordingTags.Count != headPosRecordings.headPosList.Count)
                    {
                        recordingTags.Clear();
                        for (int i = 0; i < headPosRecordings.headPosList.Count; i++)
                        {
                            recordingTags.Add(headPosRecordings.headPosList[i].info.createTime);
                        }
                    }
                    DataSnapshot snapshot = task.Result;
                    HeadPosSeriesList retrievedRecordings = JsonUtility.FromJson<HeadPosSeriesList>(snapshot.GetValue(false).ToString());
                    for (int i = 0; i < retrievedRecordings.headPosList.Count; i++)
                    {
                        if (!recordingTags.Contains(retrievedRecordings.headPosList[i].info.createTime))
                        {
                            headPosRecordings.headPosList.Add(retrievedRecordings.headPosList[i]);
                            recordingTags.Add(retrievedRecordings.headPosList[i].info.createTime);
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
        headPosRecordings = new HeadPosSeriesList();
        SaveOnline();
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
