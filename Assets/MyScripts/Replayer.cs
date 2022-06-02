using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class Replayer : MonoBehaviour
{

    public GameObject HeadPrefab;
    private HeadPosSeriesList headPosRecordings = new HeadPosSeriesList();
    private HeadPosSeries headPosRecording = new HeadPosSeries();
    private GameObject HeadObject;
    private Coroutine replayingCoroutine;
    private bool coroutineRunning = false;
    private bool coroutinePause = false;

    public bool test_start = false;
    public bool test_end = false;
    public bool test_pause = false;
    public bool test_sync_database = true;

    private int currentId = 0;

    public GameObject replayingIndicator;
    private Coroutine replayingIndicatorCoroutine;

    public ReplayerControl control;

    private DatabaseReference reference;

    public void SetCurrentId(int id)
    {
        currentId = id;
    }

    public int GetPlayListLength()
    {
        return headPosRecordings.headPosList.Count;
    }

    public HeadPosInfo GetReplayInfo(int id = 0)
    {
        if (id >= headPosRecordings.headPosList.Count)
        {
            return new HeadPosInfo();
        }
        HeadPosSeries localSeries = headPosRecordings.headPosList[id];
        return localSeries.info;
    }

    public void UpdateHeadPosRecordingsLocal()
    {
        try
        {
            string headPosJson = System.IO.File.ReadAllText(Application.persistentDataPath + "/HeadPosData.json");
            headPosRecordings = JsonUtility.FromJson<HeadPosSeriesList>(headPosJson);
            control.UpdateText();
        }
        catch (System.IO.DirectoryNotFoundException dirEx)
        {
        }
    }

    public void UpdateHeadPosRecordingsOnline()
    {
        try
        {
            reference.Child("recordings")
                .GetValueAsync().ContinueWithOnMainThread(task => {
                    if (task.IsCompleted)
                    {
                        DataSnapshot snapshot = task.Result;
                        System.IO.File.WriteAllText(Application.persistentDataPath + "/HeadPosData.json", snapshot.GetValue(false).ToString());
                        UpdateHeadPosRecordingsLocal();
                    }
                    else if (task.IsFaulted)
                    {
                        Debug.Log("Download from Database failed");
                    }
                });
        }
        catch (System.IO.DirectoryNotFoundException dirEx)
        {
        }
    }

    IEnumerator ReplayingIndicatorCoroutine()
    {
        while (true)
        {
            while (coroutinePause)
            {
                replayingIndicator.SetActive(true);
                yield return null;
            }
            replayingIndicator.SetActive(!replayingIndicator.activeSelf);
            yield return new WaitForSeconds(0.5f);
        }
        yield return null;
    }

    IEnumerator ReplayingCoroutine(float delta_time = 1.0f)
    {
        for (int i = 1; i < headPosRecording.headPosSeries.Count; i++)
        {
            while (coroutinePause)
            {
                yield return null;
            }
            Vector3 localPos = headPosRecording.headPosSeries[i].PosToVec();
            Quaternion localRot = headPosRecording.headPosSeries[i].RotToQuat();
            Vector3 initPos = HeadObject.transform.position;
            Quaternion initRot = HeadObject.transform.rotation;
            float localTime = 0;
            while (localTime < delta_time)
            {
                //Debug.Log("#");
                //Debug.Log(localTime);
                //Debug.Log(delta_time);
                HeadObject.transform.position = Vector3.Lerp(initPos, localPos, localTime / delta_time);
                HeadObject.transform.rotation = Quaternion.Lerp(initRot, localRot, localTime / delta_time);
                localTime += Time.deltaTime;
                yield return null;// new WaitForSeconds(Time.deltaTime);
            }
            //break;
        }
        Destroy(HeadObject);
        StopCoroutine(replayingIndicatorCoroutine);
        replayingIndicator.SetActive(false);
        coroutineRunning = false;
        coroutinePause = false;
        yield return null;
    }

    bool StartReplay()
    {
        if (!coroutineRunning)
        {
            //string headPosJson = System.IO.File.ReadAllText(Application.persistentDataPath + "/HeadPosData.json");
            //headPosRecording = JsonUtility.FromJson<HeadPosSeries>(headPosJson);
            if (currentId >= headPosRecordings.headPosList.Count)
            {
                return false;
            }
            headPosRecording = headPosRecordings.headPosList[currentId];
            Vector3 headPosition = headPosRecording.headPosSeries[0].PosToVec();
            Quaternion headRotation = headPosRecording.headPosSeries[0].RotToQuat();
            HeadObject = Instantiate(HeadPrefab, headPosition, headRotation);
            replayingCoroutine = StartCoroutine(ReplayingCoroutine(headPosRecording.info.deltaTime));
            replayingIndicatorCoroutine = StartCoroutine(ReplayingIndicatorCoroutine());
            coroutineRunning = true;
            return true;
        }
        return false;
    }

    void PauseReplay()
    {
        if (coroutineRunning)
        {
            coroutinePause = !coroutinePause;

        }
    }

    void StopReplay()
    {
        if (coroutineRunning)
        {
            StopCoroutine(replayingCoroutine);
            StopCoroutine(replayingIndicatorCoroutine);
            replayingIndicator.SetActive(false);
            coroutineRunning = false;
            coroutinePause = false;
            Destroy(HeadObject);
        }
    }

    public void ToggleReplay()
    {
        if (!coroutineRunning)
        {
            StartReplay();
        }
        else
        {
            StopReplay();
        }
    }

    public void TogglePause()
    {
        if (coroutineRunning)
        {
            PauseReplay();
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
            StartReplay();
            test_start = false;
        }
        if (test_end)
        {
            StopReplay();
            test_end = false;
        }
        if (test_pause)
        {
            PauseReplay();
            test_pause = false;
        }
        if (test_sync_database)
        {
            UpdateHeadPosRecordingsOnline();
            test_sync_database = false;
        }
    }
}
