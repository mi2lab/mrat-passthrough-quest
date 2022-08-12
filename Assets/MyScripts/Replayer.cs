using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Replayer : MonoBehaviour
{
    [HideInInspector]
    public GameObject HeadPrefab;
    private HeadPosSeries headPosRecording = new HeadPosSeries();
    private GameObject HeadObject;
    private Coroutine replayingCoroutine;
    private bool coroutineRunning = false;
    private bool coroutinePause = false;

    private int currentId = 0;

    public GameObject replayingIndicator;
    private Coroutine replayingIndicatorCoroutine;

    [HideInInspector]
    public ReplayerControl control;
    [HideInInspector]
    public RecordingDatabse database;

    public GameObject snapshotContainer;
    public GameObject snapshotPrefab;
    public Material lineMaterial;
    private Coroutine snapshotCoroutine;
    private LineRenderer lineRenderer;
    private bool snapshotCoroutineRunning = false;

    private LiveDemoGroup demo = new LiveDemoGroup();
    [HideInInspector]
    public GameObject handJointTipPrefab;
    [HideInInspector]
    public GameObject handJointPrefab;

    // replay with avatar, experimental function, see manual
    private GameObject avatarPrefab = null; 
    private bool useAvatar = false;

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

    IEnumerator moveTowards(GameObject obj, HeadPos tar, float delta_time)
    {
        Vector3 localPos = tar.PosToVec();
        Quaternion localRot = tar.RotToQuat();
        Vector3 initPos = obj.transform.position;
        Quaternion initRot = obj.transform.rotation;
        float localTime = 0;
        while (localTime < delta_time)
        {
            obj.transform.position = Vector3.Lerp(initPos, localPos, localTime / delta_time);
            obj.transform.rotation = Quaternion.Lerp(initRot, localRot, localTime / delta_time);
            localTime += Time.deltaTime;
            yield return null;
        }
    }

    public void UpdateItem(HeadPos pos, float delta_time, bool trackHands = false, HandPos handPos = null)
    {
        if (demo.running)
        {
            foreach (LiveDemoItem item in demo.item)
            {
                StopCoroutine(item.co);
            }
            demo.running = false;
        }
        if (trackHands && handPos == null)
        {
            demo.DiscardOtherThanHead();
        }
        else if (trackHands && handPos != null && (handPos.joints.Count != (demo.item.Count - 1) || (!demo.trackHands)))
        {
            demo.DiscardOtherThanHead();
            foreach (HandJoint joint in handPos.joints)
            {
                HeadPos jointPos = joint.pos;
                Debug.Log(jointPos.PosToVec());
                GameObject local_obj;
                if (joint.part == "tip")
                {
                    local_obj = Instantiate(handJointTipPrefab, jointPos.PosToVec(), jointPos.RotToQuat());
                }
                else
                {
                    local_obj = Instantiate(handJointPrefab, jointPos.PosToVec(), jointPos.RotToQuat());
                }
                demo.AddDemo(local_obj);
            }
            demo.trackHands = true;
        }
        else if (!trackHands && demo.trackHands)
        {
            demo.DiscardOtherThanHead();
            demo.trackHands = false;
        }

        demo.running = true;
        //head
        Coroutine co = StartCoroutine(moveTowards(demo.item[0].obj, pos, delta_time));
        demo.item[0].co = co;
        //hands
        for (int i = 1; i < demo.item.Count; i++)
        {
            LiveDemoItem item = demo.item[i];
            Coroutine hand_co = StartCoroutine(moveTowards(item.obj, handPos.joints[i - 1].pos, delta_time));
            item.co = hand_co;
        }
        //avatar
        if (demo.avatarEnabled)
        {
            Vector3 head_pos = pos.PosToVec();
            Quaternion head_rot = pos.RotToQuat();
            Vector3 left_pos = new Vector3();
            Quaternion left_rot = new Quaternion();
            Vector3 right_pos = new Vector3();
            Quaternion right_rot = new Quaternion();
            if (handPos != null)
            {
                foreach (HandJoint joint in handPos.joints)
                {
                    if (joint.part == "L_Middle_1")
                    {
                        left_pos = joint.pos.PosToVec();
                        left_rot = joint.pos.RotToQuat();
                    }
                    else if (joint.part == "R_Middle_1")
                    {
                        right_pos = joint.pos.PosToVec();
                        right_rot = joint.pos.RotToQuat();
                    }
                }
            }
            demo.avatar.GetComponent<AvatarUpdateManager>().UpdateAvatar(head_pos, head_rot, left_pos, left_rot, right_pos, right_rot, head_pos, new Quaternion(), delta_time);
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

    IEnumerator ReplayingCoroutine()
    {
        for (int i = 1; i < headPosRecording.headPosSeries.Count; i++)
        {
            while (coroutinePause)
            {
                yield return null;
            }
            UpdateItem(headPosRecording.headPosSeries[i], headPosRecording.info.deltaTime, headPosRecording.useHandTrack, headPosRecording.handPosSeries[i]);
            yield return new WaitForSeconds(headPosRecording.info.deltaTime);
        }
        demo.running = false;
        if (demo.avatarEnabled)
        {
            Destroy(demo.avatar);
        }
        foreach (LiveDemoItem group_item in demo.item)
        {
            Destroy(group_item.obj);
            StopCoroutine(group_item.co);
        }
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
        if (snapshotCoroutineRunning)
        {
            StopCoroutine(snapshotCoroutine);
            ClearSnapshot();
            snapshotCoroutineRunning = false;
        }
        if (!coroutineRunning)
        {
            if (currentId >= database.headPosRecordings.Count())
            {
                return false;
            }
            headPosRecording = database.headPosRecordings[currentId];
            Vector3 headPosition = headPosRecording.headPosSeries[0].PosToVec();
            Quaternion headRotation = headPosRecording.headPosSeries[0].RotToQuat();

            GameObject local_obj = Instantiate(HeadPrefab, headPosition, headRotation);
            if (useAvatar)
            {
                local_obj.SetActive(false);
            }
            local_obj.transform.GetChild(0).GetComponent<TextMeshPro>().text = headPosRecording.info.id;
            demo = new LiveDemoGroup();
            demo.AddDemo(local_obj);
            if (headPosRecording.useHandTrack)
            {
                foreach (HandJoint joint in headPosRecording.handPosSeries[0].joints)
                {
                    HeadPos jointPos = joint.pos;
                    GameObject local_hand_obj;
                    if (joint.part == "tip")
                    {
                        local_hand_obj = Instantiate(handJointTipPrefab, jointPos.PosToVec(), jointPos.RotToQuat());
                    }
                    else
                    {
                        local_hand_obj = Instantiate(handJointPrefab, jointPos.PosToVec(), jointPos.RotToQuat());
                    }
                    demo.AddDemo(local_hand_obj);
                }
                demo.trackHands = true;
            }

            if (useAvatar)
            {
                demo.avatarEnabled = true;
                GameObject avatar = Instantiate(avatarPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
                demo.avatar = avatar;
            }
            else
            {
                demo.avatarEnabled = false;
            }

            replayingCoroutine = StartCoroutine(ReplayingCoroutine());
            replayingIndicatorCoroutine = StartCoroutine(ReplayingIndicatorCoroutine());
            coroutineRunning = true;
            return true;
        }
        return false;
    }

    public bool isPaused()
    {
        return coroutinePause;
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
            if (demo.avatarEnabled)
            {
                Destroy(demo.avatar);
            }
            foreach (LiveDemoItem group_item in demo.item)
            {
                Destroy(group_item.obj);
                StopCoroutine(group_item.co);
            }
            StopCoroutine(replayingIndicatorCoroutine);
            replayingIndicator.SetActive(false);
            coroutineRunning = false;
            coroutinePause = false;
        }
    }

    void ClearSnapshot()
    {
        lineRenderer.positionCount = 0;
        foreach (Transform child in snapshotContainer.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    IEnumerator SnapshotCoroutine(float display_time = 5f, float keep_time = 5f)
    {
        snapshotCoroutineRunning = true;
        ClearSnapshot();
        Queue<GameObject> snapQueue = new Queue<GameObject>(headPosRecording.headPosSeries.Count - 1);
        Queue<Vector3> pointQueue = new Queue<Vector3>();
        Queue<Vector3> checkQueue = new Queue<Vector3>();
        Debug.Log(headPosRecording.headPosSeries.Count);
        for (int i = 0; i < headPosRecording.headPosSeries.Count; i++)
        {
            Vector3 localPos = headPosRecording.headPosSeries[i].PosToVec();
            Quaternion localRot = headPosRecording.headPosSeries[i].RotToQuat();

            void spawnObj()
            {
                GameObject localObj = Instantiate(snapshotPrefab, localPos, localRot);
                localObj.transform.SetParent(snapshotContainer.transform);
                snapQueue.Enqueue(localObj);
            }
            Vector3 prevPos;
            if (checkQueue.Count >= 3)
            {
                prevPos = checkQueue.Dequeue();
                if (Vector3.Distance(prevPos, localPos) > 0.15)
                {
                    spawnObj();
                    checkQueue.Clear();
                }
            }
            else if (checkQueue.Count == 1)
            {
                spawnObj();
            }
            checkQueue.Enqueue(localPos);

            pointQueue.Enqueue(localPos);
            lineRenderer.positionCount = pointQueue.Count;
            lineRenderer.SetPositions(pointQueue.ToArray());
            yield return new WaitForSeconds(display_time / headPosRecording.headPosSeries.Count);
        }
        yield return new WaitForSeconds(3f);
        while (snapQueue.Count > 0)
        {
            GameObject obj = snapQueue.Dequeue();
            Destroy(obj);
        }
        while (pointQueue.Count > 0)
        {
            pointQueue.Dequeue();
            lineRenderer.positionCount = pointQueue.Count;
            lineRenderer.SetPositions(pointQueue.ToArray());
            yield return new WaitForSeconds(display_time / headPosRecording.headPosSeries.Count);
        }
        snapshotCoroutineRunning = false;
        lineRenderer.positionCount = 0;
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

    void Start()
    {
        lineRenderer = snapshotContainer.AddComponent<LineRenderer>();
        lineRenderer.material = lineMaterial;
        //lineRenderer.material.SetColor("_Color", new Color(255f, 255f, 0f, 0.3f));
        //lineRenderer.startColor = Color.yellow;
        //lineRenderer.endColor = Color.yellow;
        lineRenderer.startWidth = 0.15f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
        //lineRenderer.numCornerVertices = 10;
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
    }

    void Update()
    {
        
    }
}
