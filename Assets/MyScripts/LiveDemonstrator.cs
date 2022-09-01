using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class LiveDemoGroup
{
    public List<LiveDemoItem> item = new List<LiveDemoItem>();
    public bool running = false;
    public bool trackHands = false;

    public GameObject avatar;
    public bool avatarEnabled = true;

    public void AddDemo(GameObject obj)
    {
        item.Add(new LiveDemoItem(obj));
    }

    public void DiscardOtherThanHead()
    {
        for (int i = item.Count - 1; i > 0; i--)
        {
            item[i].End();
            item.RemoveAt(i);
        }
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

    public void End()
    {
        GameObject.Destroy(obj);
    }
}

public class LiveDemonstrator : MonoBehaviour
{
    [HideInInspector]
    public GameObject prefab;
    private Dictionary<string, LiveDemoGroup> demoDict = new Dictionary<string, LiveDemoGroup>();

    [HideInInspector]
    public GameObject handJointPrefab;
    [HideInInspector]
    public GameObject handJointTipPrefab;

    // replay with avatar, experimental function, see manual
    [HideInInspector]
    public GameObject avatarPrefab = null;

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

    public void UpdateItem(string id, HeadPos pos, float delta_time, bool trackHands = false, HandPos handPos = null, bool useAvatar = true)
    {
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

            if (trackHands && handPos == null)
            {
                demoDict[id].DiscardOtherThanHead();
            }
            else if (trackHands && handPos!= null && (handPos.joints.Count != (demoDict[id].item.Count - 1) || ( !demoDict[id].trackHands))) {
                demoDict[id].DiscardOtherThanHead();
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
                    demoDict[id].AddDemo(local_obj);
                }
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
            //avatar
            if (demoDict[id].avatarEnabled)
            {
                Vector3 head_pos = pos.PosToVec();
                Quaternion head_rot = pos.RotToQuat();
                Vector3 left_pos = new Vector3();
                Quaternion left_rot = new Quaternion();
                Vector3 right_pos = new Vector3();
                Quaternion right_rot = new Quaternion();
                foreach (HandJoint joint in handPos.joints)
                {
                    if (joint.part == "L_Wrist")
                    {
                        left_pos = joint.pos.PosToVec();
                        left_rot = joint.pos.RotToQuat();
                    }
                    else if (joint.part == "R_Wrist")
                    {
                        right_pos = joint.pos.PosToVec();
                        right_rot = joint.pos.RotToQuat();
                    }
                }
                demoDict[id].avatar.GetComponent<AvatarUpdateManager>().UpdateAvatar(head_pos, head_rot, left_pos, left_rot, right_pos, right_rot, head_pos, new Quaternion(), delta_time);
            }
        }
        else
        {
            Debug.Log("Initializing...");
            GameObject local_obj = Instantiate(prefab, pos.PosToVec(), pos.RotToQuat());
            if (useAvatar)
            {
                local_obj.SetActive(false);
            }
            local_obj.transform.GetChild(0).GetComponent<TextMeshPro>().text = id;
            demoDict[id] = new LiveDemoGroup();
            demoDict[id].AddDemo(local_obj);
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
            demoDict[id].avatarEnabled = useAvatar;
            if (useAvatar)
            {
                GameObject avatar = Instantiate(avatarPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
                demoDict[id].avatar = avatar;
            }
        }
    }

    public void UpdateDict(List<string> keys)
    {
        foreach (KeyValuePair<string, LiveDemoGroup> item in demoDict)
        {
            if (!keys.Contains(item.Key))
            {
                Destroy(item.Value.avatar);
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
            Destroy(item.Value.avatar);
            foreach (LiveDemoItem group_item in item.Value.item)
            {
                Destroy(group_item.obj);
                StopCoroutine(group_item.co);
            }
        }
        demoDict.Clear();
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
