using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Firebase;
using Firebase.Database;
using Firebase.Extensions;

/*
[System.Serializable]
public class LivePos
{
    private Dictionary<int, HeadPos> headPosList = new Dictionary<int, HeadPos>();

    public void Add(int id)
    {
        if (!headPosList.ContainsKey(id))
        {
            headPosList.Add(id, new HeadPos());
        }
    }

    public void Update(int id, HeadPos pos)
    {
        if (headPosList.ContainsKey(id))
        {
            headPosList[id] = pos;
        }
    }

    public void Update(Dictionary<int, HeadPos> dict)
    {
        foreach (KeyValuePair<int, HeadPos> pos in dict)
        {
            headPosList[pos.Key] = pos.Value;
        }
    }

    public void Remove(int id)
    {
        if (headPosList.ContainsKey(id))
        {
            headPosList.Remove(id);
        }
    }

    public HeadPos GetValue(int id)
    {
        if (headPosList.ContainsKey(id))
        {
            return headPosList[id];
        }
        else
        {
            return null;
        }
    }

    public List<int> GetKeys()
    {
        return new List<int>(headPosList.Keys);
    }

    public List<int> GetKeys(int id)
    {
        List<int> ret = new List<int>(headPosList.Keys);
        ret.Remove(id);
        return ret;
    }

    public void Clear()
    {
        headPosList.Clear();
    }
}
*/

public class DatabaseSync : MonoBehaviour
{
    public GameObject target;
    public float deltaTime = 1;
    public int personalId = 1;
    //public GameObject syncIndicator;
    private bool synchronizing = false;
    private DatabaseReference reference;
    private Coroutine co;
    HeadPos localPos = new HeadPos();

    //private Dictionary<int, HeadPos> otherPos = new Dictionary<int, HeadPos>();
    //private LivePos otherPos = new LivePos();
    private bool downSynchronizing = false;
    private Coroutine downCo;
    public LiveDemonstrator demo;

    public bool test_up_sync = false;
    public bool test_down_sync = false;
   

    IEnumerator UpSyncCoroutine()
    {
        while (true)
        {
            try
            {
                localPos.FromTransform(target.transform);
                string localPosJson = JsonUtility.ToJson(localPos);
                reference.Child("livePos").Child(personalId.ToString()).SetValueAsync(localPosJson);
            }
            catch
            {
                Debug.Log("Error exception");
            }
            yield return new WaitForSeconds(deltaTime);
        }

        yield return null;
    }

    public void ToggleSync()
    {
        if (!synchronizing)
        {
            co = StartCoroutine(UpSyncCoroutine());
        }
        else
        {
            StopCoroutine(co);
            reference.Child("livePos").Child(personalId.ToString()).RemoveValueAsync();
        }
        synchronizing = !synchronizing;
    }

    IEnumerator DownSyncCoroutine()
    {
        while (true)
        {
            reference.Child("livePos")
            .GetValueAsync().ContinueWithOnMainThread(task => {
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;

                    List<int> keyList = new List<int>(); 
                    foreach (DataSnapshot pos in snapshot.Children)
                    {
                        int localKey = int.Parse(pos.Key);
                        if (localKey != personalId)
                        {
                            HeadPos retrievedPos = JsonUtility.FromJson<HeadPos>(pos.GetValue(false).ToString());
                            demo.UpdateItem(localKey, retrievedPos, deltaTime);
                            keyList.Add(localKey);
                        }
                    }
                    demo.UpdateDict(keyList);
                }
                else if (task.IsFaulted)
                {
                    Debug.Log("Read from Database failed");
                }
            });
            yield return new WaitForSeconds(deltaTime);
        }
        yield return null;
    }

    public void ToggleDownSync()
    {
        if (!downSynchronizing)
        {
            downCo = StartCoroutine(DownSyncCoroutine());
        }
        else
        {
            StopCoroutine(downCo);
            demo.Destroy();
        }
        downSynchronizing = !downSynchronizing;
    }

    // Start is called before the first frame update
    void Start()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    // Update is called once per frame
    void Update()
    {
        if (test_up_sync)
        {
            ToggleSync();
            test_up_sync = false;
        }
        if (test_down_sync)
        {
            ToggleDownSync();
            test_down_sync = false;
        }
    }
}
