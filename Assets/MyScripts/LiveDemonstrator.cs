using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class LiveDemonstrator : MonoBehaviour
{
    public class LiveDemoItem
    {
        public GameObject obj;
        public Coroutine co;
        public bool running = false;

        public LiveDemoItem()
        {
            obj = new GameObject();
            co = null;
            running = false;
        }

        public LiveDemoItem(GameObject obj)
        {
            this.obj = obj;
            co = null;
            running = false;
        }

    }

    public GameObject prefab;
    //private Dictionary<int, GameObject> itemDict = new Dictionary<int, GameObject>();
    //private Dictionary<int, Coroutine> coDict = new Dictionary<int, Coroutine>();
    private Dictionary<string, LiveDemoItem> demoDict = new Dictionary<string, LiveDemoItem>();
    //private Dictionary<int, bool> flagDict = new Diction

    IEnumerator moveTowards(GameObject obj, HeadPos tar, string id, float delta_time)
    {
        demoDict[id].running = true;
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
        demoDict[id].running = false;
        //break;
    }

    public void UpdateItem(string id, HeadPos pos, float delta_time)
    {
        if (demoDict.ContainsKey(id))
        {
            if (demoDict[id].running)
            {
                StopCoroutine(demoDict[id].co);
                demoDict[id].running = false;
            }
            Coroutine co = StartCoroutine(moveTowards(demoDict[id].obj, pos, id, delta_time));
            demoDict[id].co = co;
        }
        else
        {
            Debug.Log("Initializing...");
            GameObject local_obj = Instantiate(prefab, pos.PosToVec(), pos.RotToQuat());
            local_obj.transform.GetChild(0).GetComponent<TextMeshPro>().text = id;
            demoDict[id] = new LiveDemoItem(local_obj);
        }
    }

    public void UpdateDict(List<string> keys)
    {
        foreach (KeyValuePair<string, LiveDemoItem> item in demoDict)
        {
            if (!keys.Contains(item.Key))
            {
                Destroy(demoDict[item.Key].obj);
                StopCoroutine(demoDict[item.Key].co);
                demoDict.Remove(item.Key);
            }
        }
    }

    public void Stop()
    {
        foreach (KeyValuePair<string, LiveDemoItem> item in demoDict)
        {
            StopCoroutine(demoDict[item.Key].co);
        }
    }

    public void Destroy()
    {
        foreach (KeyValuePair<string, LiveDemoItem> item in demoDict)
        {
            Destroy(demoDict[item.Key].obj);
            StopCoroutine(demoDict[item.Key].co);
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
