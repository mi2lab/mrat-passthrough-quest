using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Events;

using Firebase;
using Firebase.Database;
using Firebase.Extensions;

using TMPro;

public class AccountLogger : MonoBehaviour
{

    private DatabaseReference reference;
    private TouchScreenKeyboard overlayKeyboard;
    private bool finished = true;
    private bool availabilityCheckFinished = true;
    private Coroutine inputCo;
    [HideInInspector] public string inputText = "";
    [HideInInspector] public bool inputLegal = false;
    public TextMeshPro targetText;

    public string testInput = "";
    public bool testFinished = false;

    public void ToggleFinished()
    {
        if (!finished)
        {
            finished = true;
        }
    }

    private IEnumerator KeyboardWaiting()
    {
        Debug.Log("Waiting for keyboard to finish");
        finished = false;
        while (!finished)
        {
            yield return null;
        }
        Debug.Log("Keyboard finished");
        CheckAvailability(inputText);
        yield return null;
    }

    public string GetAccount()
    {
        Debug.Log("Status: " + finished + " " + availabilityCheckFinished + " " + inputLegal);
        if (finished && availabilityCheckFinished)
        {
            if (inputLegal)
            {
                Debug.Log("Legal checked: " + inputText);
                targetText.text = "Welcome: " + inputText;
                return inputText;
            }
            else
            {
                targetText.text = "Error, username not available";
                return "";
            }
        }
        return null;
    }
    
    public void CreateAccount()
    {
        Debug.Log("Opening keyboard");
        inputLegal = false;
        inputText = "";
        overlayKeyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
        StartCoroutine(KeyboardWaiting());
    }

    private IEnumerator WaitAvailabilityCheck()
    {
        while(!availabilityCheckFinished)
        {
            yield return null;
        }
        yield return null;
    }

    private void CheckAvailability(string account)
    {
        availabilityCheckFinished = false;
        if (reference != null)
        {
            reference.Child("livePos")
            .GetValueAsync().ContinueWithOnMainThread(task => {
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    foreach (DataSnapshot pos in snapshot.Children)
                    {
                        if (account == pos.Key)
                        {
                            availabilityCheckFinished = true;
                            return;
                        }
                    }
                    inputLegal = true;
                    //availabilityCheckFinished = true;
                }
                else if (task.IsFaulted)
                {
                    Debug.Log("Read from Database failed");
                }
            });
        }
        availabilityCheckFinished = true;
        Debug.Log("Availability checked");
        return;
    }

    private List<string> nameList = new List<string>
        {
        "Albedo",
        "Amber",
        "Bennett",
        "Diluc",
        "Eula",
        "Fischl",
        "Rosaria",
        "Venti",
        "Corhyn",
        "Sellen",
        "Fia",
        "Gideon",
        "Melina",
        "Marika",
        "Radagon",
        "Ranni",
        "Rogier",
        "Anri",
        "Andre",
        "Siegward",
        "Geralt",
        "Triss",
        "Yennefer",
        "Dandelion"
        };

    public void GenerateUserName()
    {
        if (availabilityCheckFinished && !inputLegal)
        {
            Debug.Log("Checking: " + inputText);
            inputText = nameList[Random.Range(0, nameList.Count)];
            CheckAvailability(inputText);
        }
        Debug.Log(inputText + " " + availabilityCheckFinished + " " + inputLegal);
    }

    // Start is called before the first frame update
    void Start()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.platform != RuntimePlatform.WindowsEditor)
        {
            if (overlayKeyboard != null && !finished)
            {
                inputText = overlayKeyboard.text;
                targetText.text = inputText;
            }
        }
        if (testFinished)
        {
            inputText = testInput;
            Debug.Log(inputText);
            ToggleFinished();
            testFinished = false;
        }
    }
}
