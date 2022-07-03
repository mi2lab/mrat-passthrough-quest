using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public class HandDemonstrator : MonoBehaviour
{

    public float delta_time = 1f;
    public HandTracker tracker;

    IEnumerator HandTrackingCoroutine()
    {
        tracker.UpdateHandPos();
        yield return new WaitForSeconds(delta_time);
    }

    IEnumerator HandDemoCoroutine()
    {
        HandPos pos = tracker.GetHandPos();
        for (int i = 0; i < headPosRecording.headPosSeries.Count; i++)
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
*/