using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Recorder : MonoBehaviour
{

    //[SerializeField] private HeadPosSeriesList headPosRecordings = new HeadPosSeriesList();
    [HideInInspector] [SerializeField] 
    private HeadPosSeries headPosRecording;

    [HideInInspector]
    public GameObject target;

    public float recordDeltaTime = 0.2f;
    private Coroutine recordingCoroutine;
    private bool coroutineRunning = false;

    public GameObject recordingIndicator;
    private Coroutine recordingIndicatorCoroutine;

    [HideInInspector]
    public Replayer replayer;
    [HideInInspector]
    public RecordingDatabse database;

    [HideInInspector]
    public HandTracker handTracker;
    public bool trackHands = false;

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
            if (headPosRecording.useHandTrack)
            {
                HandPos localHandPos = handTracker.GetHandPos();
                /*
                if (localHandPos != null)
                {
                    localHandPos.Print();
                }
                */
                headPosRecording.handPosSeries.Add(localHandPos);
                database.InsertOnlinePos(headPosRecording.info, localPos, true, handTracker.GetHandPos());
            }
            else
            {
                database.InsertOnlinePos(headPosRecording.info, localPos);
            }
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
            headPosRecording.useHandTrack = trackHands;
            database.CreateOnlineItem(headPosRecording.info, trackHands);
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
       
        //Debug.Log(System.DateTime.Now.Ticks);
    }
}
