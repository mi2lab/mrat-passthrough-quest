using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ReplayerControl : MonoBehaviour
{
    public Replayer replayer;
    public TextMeshPro idText;
    public TextMeshPro endTimeText;
    public TextMeshPro userText;

    public int currentId = 0;

    public void IncreaseId()
    {
        currentId = currentId < replayer.GetPlayListLength() - 1 ? currentId + 1 : currentId;
        UpdateAll();
    }

    public void DecreaseId()
    {
        currentId = currentId >= 0 ? currentId - 1 : currentId;
        UpdateAll();
    }

    public void SetId(int id)
    {
        if (id >= 0 && id < replayer.GetPlayListLength())
        {
            currentId = id;
            UpdateAll();
        }
    }

    public void UpdateAll()
    {
        UpdateText();
        UpdateReplayerId();
    }

    public void UpdateText()
    {
        if (replayer.GetPlayListLength() == 0)
        {
            currentId = 0;
            idText.text = "0 / 0";
            endTimeText.text = "";
        }
        else
        {
            if (currentId >= replayer.GetPlayListLength())
            {
                currentId = replayer.GetPlayListLength() - 1;
            }
            HeadPosInfo localInfo = replayer.GetReplayInfo(currentId);
            idText.text = (currentId + 1).ToString() + " / " + replayer.GetPlayListLength().ToString();
            System.DateTime localDate = new System.DateTime((long)localInfo.endTime);
            string endTime = localDate.ToString("G");
            endTimeText.text = endTime;
            userText.text = localInfo.id;
        }
    }

    public void UpdateReplayerId()
    {
        replayer.SetCurrentId(currentId);
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
