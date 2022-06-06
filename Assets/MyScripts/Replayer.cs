using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Replayer : MonoBehaviour
{

    public GameObject HeadPrefab;
    //private HeadPosSeriesList headPosRecordings = new HeadPosSeriesList();
    private HeadPosSeries headPosRecording = new HeadPosSeries();
    private GameObject HeadObject;
    private Coroutine replayingCoroutine;
    private bool coroutineRunning = false;
    private bool coroutinePause = false;

    private int currentId = 0;

    public GameObject replayingIndicator;
    private Coroutine replayingIndicatorCoroutine;

    public ReplayerControl control;
    public RecordingDatabse database;

    public void SetCurrentId(int id)
    {
        currentId = id;
    }

    public int GetPlayListLength()
    {
        return database.headPosRecordings.headPosList.Count;
    }

    public HeadPosInfo GetReplayInfo(int id = 0)
    {
        if (id >= database.headPosRecordings.headPosList.Count)
        {
            return new HeadPosInfo();
        }
        HeadPosSeries localSeries = database.headPosRecordings.headPosList[id];
        return localSeries.info;
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

    public bool isReplaying()
    {
        return coroutineRunning;
    }

    public void UpdateReplayPanel()
    {
        control.UpdateAll();
    }

    public void UpdateReplay()
    {
        database.ReadOnline();
        UpdateReplayPanel();
    }

    public void DeleteReplay()
    {
        database.Delete();
        UpdateReplayPanel();
    }

    public bool StartReplay()
    {
        if (!coroutineRunning)
        {
            //string headPosJson = System.IO.File.ReadAllText(Application.persistentDataPath + "/HeadPosData.json");
            //headPosRecording = JsonUtility.FromJson<HeadPosSeries>(headPosJson);
            if (currentId >= database.headPosRecordings.headPosList.Count)
            {
                return false;
            }
            headPosRecording = database.headPosRecordings.headPosList[currentId];
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

    public void PauseReplay()
    {
        if (coroutineRunning)
        {
            coroutinePause = !coroutinePause;

        }
    }

    public void StopReplay()
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}
