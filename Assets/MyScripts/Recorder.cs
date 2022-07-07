using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Recorder : MonoBehaviour
{

    //[SerializeField] private HeadPosSeriesList headPosRecordings = new HeadPosSeriesList();
    [SerializeField] private HeadPosSeries headPosRecording;
    public GameObject target;
    public bool test_start;
    public bool test_end;
    public float recordDeltaTime = 0.2f;
    private Coroutine recordingCoroutine;
    private bool coroutineRunning = false;

    public GameObject recordingIndicator;
    private Coroutine recordingIndicatorCoroutine;

    public Replayer replayer;
    public RecordingDatabse database;

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
            database.InsertOnlinePos(headPosRecording.info, localPos);
            //Debug.Log(localPos.posX);
            yield return new WaitForSeconds(recordDeltaTime);
        }
        yield return null;
    }

    public bool isRecording()
    {
        return coroutineRunning;
    }

    public void StartRecording()
    {
        if (!coroutineRunning)
        {
            //HeadPosSeries localHeadPosRecording = new HeadPosSeries();
            headPosRecording = new HeadPosSeries();
            headPosRecording.headPosSeries.Clear();
            headPosRecording.info.deltaTime = recordDeltaTime;
            headPosRecording.info.id = database.GetId();
            headPosRecording.info.createTime = System.DateTime.Now.Ticks;
            database.CreateOnlineItem(headPosRecording.info);
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
            Debug.Log(headPosRecording.info.createTime);
            database.headPosRecordings.Add(headPosRecording.info.ToKey(), headPosRecording);
            database.FinishOnlineItem(headPosRecording.info);
            //database.Save();
            replayer.UpdateReplayPanel();
            coroutineRunning = false;
            //database.UpdateHeadPosRecordingsLocal();
        }
    }

    // Start is called before the first frame update
    void Start()
    {

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
