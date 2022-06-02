using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Firebase;
using Firebase.Database;

public class DatabaseSync : MonoBehaviour
{
    public GameObject target;
    public float deltaTime = 1;
    //public GameObject syncIndicator;
    private bool synchronizing = false;
    private DatabaseReference reference;
    private Coroutine co;
    HeadPos localPos = new HeadPos();

    IEnumerator SyncCoroutine()
    {
        while (true)
        {
            try
            {
                localPos.FromTransform(target.transform);
                string localPosJson = JsonUtility.ToJson(localPos);
                reference.Child("currentPos").SetValueAsync(localPosJson);
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
            co = StartCoroutine(SyncCoroutine());
        }
        else
        {
            StopCoroutine(co);
        }
        synchronizing = !synchronizing;
    }

    // Start is called before the first frame update
    void Start()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
