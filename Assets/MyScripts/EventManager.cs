using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class EventManager : MonoBehaviour
{
    private DatabaseReference reference;

    // Start is called before the first frame update
    void Start()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator EventCo(string text, string user, int level)
    {
        string event_id = user + System.DateTime.Now.Ticks;
        reference.Child("Events").Child(event_id).Child("text").SetValueAsync(text);
        reference.Child("Events").Child(event_id).Child("user").SetValueAsync(user);
        reference.Child("Events").Child(event_id).Child("level").SetValueAsync(level);
        reference.Child("Events").Child(event_id).Child("time").SetValueAsync(System.DateTime.Now.ToString("HH:mm:ss"));
        yield return new WaitForSeconds(2);
        reference.Child("Events").Child(event_id).RemoveValueAsync();
        yield return null;
    }

    public void TriggerEvent(string text, string user = "", int level = 0)
    {
        StartCoroutine(EventCo(text, user, level));
    }
}
