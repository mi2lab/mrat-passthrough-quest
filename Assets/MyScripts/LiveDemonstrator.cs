using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class LiveDemonstrator : MonoBehaviour
{
    public class LiveDemoGroup
    {
        public List<LiveDemoItem> item = new List<LiveDemoItem>();
        public bool running = false;
        public bool trackHands = false;

        public void AddDemo(GameObject obj)
        {
            item.Add(new LiveDemoItem(obj));
        }

        public void DiscardOtherThanHead()
        {
            LiveDemoItem tmp = item[0];
            item.Clear();
            item.Add(tmp);
        }

    }

    public class LiveDemoItem
    {
        public GameObject obj;
        public Coroutine co;

        public LiveDemoItem()
        {
            obj = new GameObject();
            co = null;
        }

        public LiveDemoItem(GameObject obj)
        {
            this.obj = obj;
            co = null;
        }
    }

    public GameObject prefab;
    //private Dictionary<int, GameObject> itemDict = new Dictionary<int, GameObject>();
    //private Dictionary<int, Coroutine> coDict = new Dictionary<int, Coroutine>();
    private Dictionary<string, LiveDemoGroup> demoDict = new Dictionary<string, LiveDemoGroup>();
    //private Dictionary<int, bool> flagDict = new Diction

    public GameObject handJointPrefab;
    public GameObject handJointTipPrefab;
    IEnumerator moveTowards(GameObject obj, HeadPos tar, float delta_time)
    {
        Vector3 localPos = tar.PosToVec();
        Quaternion localRot = tar.RotToQuat();
        Vector3 initPos = obj.transform.position;
        Quaternion initRot = obj.transform.rotation;
        float localTime = 0;
        while (localTime < delta_time)
        {
            //Debug.Log("#");
            //Debug.Log(localTime);
            //Debug.Log(delta_time);
            obj.transform.position = Vector3.Lerp(initPos, localPos, localTime / delta_time);
            obj.transform.rotation = Quaternion.Lerp(initRot, localRot, localTime / delta_time);
            localTime += Time.deltaTime;
            yield return null;// new WaitForSeconds(Time.deltaTime);
        }
        //break;
    }

    public void UpdateItem(string id, HeadPos pos, float delta_time, bool trackHands = false, HandPos handPos = null)
    {
        Debug.Log("Updating item");
        if (demoDict.ContainsKey(id))
        {
            if (demoDict[id].running)
            {
                foreach (LiveDemoItem item in demoDict[id].item)
                {
                    StopCoroutine(item.co);
                }
                demoDict[id].running = false;
            }
            if (trackHands && !demoDict[id].trackHands)
            {
                //Debug.Log("Hand track off in dict, turn it on...");
                int count = 0;
                foreach (HandJoint joint in handPos.joints)
                {
                    //Debug.Log(++count);
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
                    demoDict[id].AddDemo(local_obj);
                }
                //Debug.Log("Hand track items added finished");
                demoDict[id].trackHands = true;
            }
            else if (!trackHands && demoDict[id].trackHands)
            {
                demoDict[id].DiscardOtherThanHead();
                demoDict[id].trackHands = false;
            }
            demoDict[id].running = true;
            //head
            Coroutine co = StartCoroutine(moveTowards(demoDict[id].item[0].obj, pos, delta_time));
            demoDict[id].item[0].co = co;
            //hands
            for (int i = 1; i < demoDict[id].item.Count; i ++)
            {
                LiveDemoItem item = demoDict[id].item[i];
                Coroutine hand_co = StartCoroutine(moveTowards(item.obj, handPos.joints[i - 1].pos, delta_time));
                item.co = hand_co;
            }
        }
        else
        {
            Debug.Log("Initializing...");
            GameObject local_obj = Instantiate(prefab, pos.PosToVec(), pos.RotToQuat());
            local_obj.transform.GetChild(0).GetComponent<TextMeshPro>().text = id;
            demoDict[id] = new LiveDemoGroup();
            demoDict[id].AddDemo(local_obj);
            //Debug.Log("Initialize with trackHands: " + trackHands);
            if (trackHands)
            {
                foreach (HandJoint joint in handPos.joints)
                {
                    HeadPos jointPos = joint.pos;
                    Debug.Log(jointPos.PosToVec());
                    GameObject local_hand_obj;
                    if (joint.part == "tip")
                    {
                        local_hand_obj = Instantiate(handJointTipPrefab, jointPos.PosToVec(), jointPos.RotToQuat());
                    }
                    else
                    {
                        local_hand_obj = Instantiate(handJointPrefab, jointPos.PosToVec(), jointPos.RotToQuat());
                    }
                    demoDict[id].AddDemo(local_hand_obj);
                }
                demoDict[id].trackHands = true;
            }
        }
    }

    public void UpdateDict(List<string> keys)
    {
        foreach (KeyValuePair<string, LiveDemoGroup> item in demoDict)
        {
            if (!keys.Contains(item.Key))
            {
                foreach (LiveDemoItem group_item in item.Value.item)
                {
                    Destroy(group_item.obj);
                    StopCoroutine(group_item.co);
                }
                demoDict.Remove(item.Key);
            }
        }
    }

    public void Stop()
    {
        foreach (KeyValuePair<string, LiveDemoGroup> item in demoDict)
        {
            foreach (LiveDemoItem group_item in item.Value.item)
            {
                StopCoroutine(group_item.co);
            }
        }
    }

    public void Destroy()
    {
        foreach (KeyValuePair<string, LiveDemoGroup> item in demoDict)
        {
            foreach (LiveDemoItem group_item in item.Value.item)
            {
                Destroy(group_item.obj);
                StopCoroutine(group_item.co);
            }
        }
        demoDict.Clear();
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
