using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandTracker : MonoBehaviour
{
    public string targetRHandName = "R_Hand_MRTK_Rig";
    public string targetLHandName = "L_Hand_MRTK_Rig";
    [HideInInspector] public float deltaTime = 1f;
    [HideInInspector] private bool trackEnabled = true;
    private bool trackRunning = false;
    private Coroutine tracking_co;
    private GameObject trackedLHand;
    private GameObject trackedRHand;
    private HandPos handPos;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (trackedLHand == null)
        {
            //Debug.Log("Finding");
            trackedLHand = GameObject.Find(targetLHandName);
        }
        if (trackedRHand == null)
        {
            //Debug.Log("Finding");
            trackedRHand = GameObject.Find(targetRHandName);
        }
        if (trackEnabled)
        {
            if (!trackRunning)
            {
                tracking_co = StartCoroutine(UpdateHandPosCo());
                trackRunning = true;
            }
        }
        else
        {
            if (trackRunning)
            {
                StopCoroutine(tracking_co);
                trackRunning = false;
            }
        }
    }

    private IEnumerator UpdateHandPosCo()
    {
        while (true)
        {
            if (trackedLHand != null || trackedRHand != null)
            {
                UpdateHandPos();
            }
            else
            {
                handPos = null;
            }
            yield return new WaitForSeconds(deltaTime);
        }

        yield return null;
    }

    private List<HandJoint> GetHandPosHelper(GameObject t)
    {
        List<HandJoint> retList = new List<HandJoint> ();
        if (!t)
        {
            return retList;
        }
        foreach (Transform child in t.transform)
        {
            retList.AddRange(GetHandPosHelper(child.gameObject));
        }
        if (t.transform.childCount == 0)
        {
            //Debug.Log("Add tip");
            retList.Add(new HandJoint(t, "tip"));
        }
        else
        {
            retList.Add(new HandJoint(t));
        }
        //Debug.Log(retList.ToString());
        return retList;
    }

    public void UpdateHandPos()
    {
        //Debug.Log("Updating...");
        handPos = new HandPos(GetHandPosHelper(trackedLHand));
        handPos.Add(GetHandPosHelper(trackedRHand));
        //Debug.Log(handPos.joints.Count);
        //Debug.Log(handPos.joints[5].pos.PosToVec());
    }

    public HandPos GetHandPos()
    {
        return handPos;
    }

    public void PrintHandPos()
    {
        foreach (HandJoint j in handPos.joints)
        {
            Debug.Log(j.part);
            Debug.Log(j.pos.PosToVec());
            Debug.Log(JsonUtility.ToJson(j));
        }
        Debug.Log(JsonUtility.ToJson(handPos));
    }
}
