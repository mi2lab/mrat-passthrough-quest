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

    public GameObject snapshotContainer;
    public GameObject snapshotPrefab;
    public Material lineMaterial;
    private Coroutine snapshotCoroutine;
    private LineRenderer lineRenderer;
    private bool snapshotCoroutineRunning = false;

    public void SetCurrentId(int id)
    {
        currentId = id;
    }

    public int GetPlayListLength()
    {
        return database.headPosRecordings.Count();
    }

    public HeadPosInfo GetReplayInfo(int id = 0)
    {
        if (id >= database.headPosRecordings.Count())
        {
            return new HeadPosInfo();
        }
        HeadPosSeries localSeries = database.headPosRecordings[id];
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
            if (currentId >= database.headPosRecordings.Count())
            {
                return false;
            }
            headPosRecording = database.headPosRecordings[currentId];
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

    IEnumerator SnapshotCoroutine(float display_time = 5f, float keep_time = 5f)
    {
        snapshotCoroutineRunning = true;
        //if (snapshotContainer.GetComponent<LineRenderer>())
        //{
        //    Destroy(snapshotContainer.GetComponent<LineRenderer>());
        //}
        lineRenderer.positionCount = 0;
        //lineRenderer.SetPositions(null);
        
        foreach (Transform child in snapshotContainer.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        Queue<GameObject> snapQueue = new Queue<GameObject>(headPosRecording.headPosSeries.Count - 1);
        Queue<Vector3> pointQueue = new Queue<Vector3>();
        Debug.Log(headPosRecording.headPosSeries.Count);
        //int keep_length = Mathf.RoundToInt(keep_time / headPosRecording.info.deltaTime);
        for (int i = 0; i < headPosRecording.headPosSeries.Count; i++)
        {
            //if (i > keep_length) {
            //    GameObject obj = snapQueue.Dequeue();
            //    Destroy(obj);
            //    pointQueue.Dequeue();
            //}
            Vector3 localPos = headPosRecording.headPosSeries[i].PosToVec();
            Quaternion localRot = headPosRecording.headPosSeries[i].RotToQuat();
            GameObject localObj = Instantiate(snapshotPrefab, localPos, localRot);
            localObj.transform.SetParent(snapshotContainer.transform);
            snapQueue.Enqueue(localObj);
            pointQueue.Enqueue(localPos);
            Debug.Log(pointQueue.Count);
            lineRenderer.positionCount = pointQueue.Count;
            lineRenderer.SetPositions(pointQueue.ToArray());
            yield return new WaitForSeconds(display_time / headPosRecording.headPosSeries.Count);
        }
        yield return new WaitForSeconds(3f);
        while (snapQueue.Count > 0)
        {
            GameObject obj = snapQueue.Dequeue();
            Destroy(obj);
            pointQueue.Dequeue();
            lineRenderer.positionCount = pointQueue.Count;
            lineRenderer.SetPositions(pointQueue.ToArray());
            yield return new WaitForSeconds(display_time / headPosRecording.headPosSeries.Count);
        }
        snapshotCoroutineRunning = false;
        lineRenderer.positionCount = 0;
        //yield return null;
        //yield return new WaitForSeconds(10f) ;
        //Destroy(snapshotContainer.GetComponent<LineRenderer>());
        yield return null;
    }

    public bool ReplaySnapshot()
    {
        if (snapshotCoroutineRunning)
        {
            StopCoroutine(snapshotCoroutine);
        }
        if (currentId >= database.headPosRecordings.Count())
        {
            return false;
        }
        headPosRecording = database.headPosRecordings[currentId];
        snapshotCoroutine = StartCoroutine(SnapshotCoroutine());
        return true;
    }

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = snapshotContainer.AddComponent<LineRenderer>();
        lineRenderer.material = lineMaterial;
        //lineRenderer.startColor = Color.yellow;
        //lineRenderer.endColor = Color.yellow;
        lineRenderer.startWidth = 0.15f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
