using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ReplayerControl : MonoBehaviour
{
    public Replayer replayer;
    public TextMeshPro idText;
    public TextMeshPro endTimeText;

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

    void UpdateAll()
    {
        UpdateText();
        UpdateReplayerId();
    }

    public void UpdateText()
    {
        HeadPosInfo localInfo = replayer.GetReplayInfo(currentId);
        if (replayer.GetPlayListLength() == 0)
        {
            idText.text = "0 / 0";
        }
        else
        {
            idText.text = (localInfo.id + 1).ToString() + " / " + replayer.GetPlayListLength().ToString();
        }
        System.DateTime localDate = new System.DateTime((long)localInfo.endTime);
        string endTime = localDate.ToString("G");
        endTimeText.text = endTime;
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
