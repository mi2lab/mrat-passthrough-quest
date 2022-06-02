using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Firebase;
using Firebase.Database;



[System.Serializable]
public class HeadPosSeriesList
{
    public List<HeadPosSeries> headPosList = new List<HeadPosSeries>();
}

[System.Serializable]
public class HeadPosInfo
{
    public int id;
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

    public void FromTransform(Transform t)
    {
        posX = t.position.x;
        posY = t.position.y;
        posZ = t.position.z;
        rotX = t.rotation.eulerAngles.x;
        rotY = t.rotation.eulerAngles.y;
        rotZ = t.rotation.eulerAngles.z;
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

}

public class Recorder : MonoBehaviour
{

    [SerializeField] private HeadPosSeriesList headPosRecordings = new HeadPosSeriesList();
    [SerializeField] private HeadPosSeries headPosRecording;
    public GameObject target;
    public bool test_start;
    public bool test_end;
    public float recordDeltaTime = 0.2f;
    private Coroutine recordingCoroutine;
    private bool coroutineRunning = false;
    private int recordingID = 0;

    public GameObject recordingIndicator;
    private Coroutine recordingIndicatorCoroutine;

    public Replayer replayer;

    private DatabaseReference reference;

    IEnumerator RecordingIndicatorCoroutine()
    {
        while (true)
        {
            recordingIndicator.SetActive(!recordingIndicator.activeSelf);
            yield return new WaitForSeconds(0.5f);
        }
        yield return null;
    }

    IEnumerator RecordingCoroutine()
    {
        while (true)
        {
            HeadPos localPos = new HeadPos(target);
            headPosRecording.headPosSeries.Add(localPos);
            //Debug.Log(localPos.posX);
            yield return new WaitForSeconds(recordDeltaTime);
        }
        yield return null;
    }

    public void StartRecording()
    {
        if (!coroutineRunning)
        {
            HeadPosSeries localHeadPosRecording = new HeadPosSeries();
            headPosRecording = localHeadPosRecording;
            headPosRecording.headPosSeries.Clear();
            headPosRecording.info.deltaTime = recordDeltaTime;
            headPosRecording.info.id = recordingID;
            Debug.Log(System.DateTime.Now.Ticks);
            headPosRecording.info.createTime = System.DateTime.Now.Ticks;
            recordingID++;
            recordingCoroutine = StartCoroutine(RecordingCoroutine());
            recordingIndicatorCoroutine = StartCoroutine(RecordingIndicatorCoroutine());
            coroutineRunning = true;
        }
    }

    public void StopRecording()
    {
        if (coroutineRunning)
        {
            StopCoroutine(recordingCoroutine);
            headPosRecording.info.endTime = System.DateTime.Now.Ticks;
            StopCoroutine(recordingIndicatorCoroutine);
            recordingIndicator.SetActive(false);
            headPosRecordings.headPosList.Add(headPosRecording);
            SaveToJson();
            coroutineRunning = false;
            replayer.UpdateHeadPosRecordingsLocal();
        }
    }

    public void SaveToJson()
    {
        string localRecordings = JsonUtility.ToJson(headPosRecordings);
        reference.Child("recordings").SetValueAsync(localRecordings);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/HeadPosData.json", localRecordings);
    }

    public void SaveToJsonTest()
    {
        string localRecording = JsonUtility.ToJson(headPosRecording);
        reference.Child("test").SetValueAsync(localRecording);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/HeadPosData.json", localRecording);
    }

    public void ToggleRecording()
    {
        if (!coroutineRunning)
        {
            StartRecording();
        }
        else
        {
            StopRecording();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    // Update is called once per frame
    void Update()
    {
        if (test_start)
        {
            StartRecording();
            test_start = false;
        }
        if (test_end)
        {
            StopRecording();
            test_end = false;
        }
        //Debug.Log(System.DateTime.Now.Ticks);
    }
}
