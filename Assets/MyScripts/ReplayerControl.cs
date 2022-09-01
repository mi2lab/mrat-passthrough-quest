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

    public GameObject textGrid;
    public GameObject textItemPrefab;
    private int prevSelected = -1;

    [HideInInspector]
    public int currentId = 0;

    public void IncreaseId()
    {
        SetId(currentId + 1);
        //UpdateAll();
    }

    public void DecreaseId()
    {
        //currentId = currentId >= 0 ? currentId - 1 : currentId;
        SetId(currentId - 1);
        //UpdateAll();
    }

    public void SetId(int id)
    {
        if (id >= 0 && id < replayer.GetPlayListLength())
        {
            DeselectPrev();
            currentId = id;
            PanelUpdateText();
            UpdateReplayerId();
            SelectItem(currentId);
            replayer.ReplaySnapshot();
        }
    }

    public void UpdateAll()
    {
        //UpdateText();
        PanelUpdateText();
        UpdateReplayerId();
    }

    public void UpdateText()
    {
        foreach (Transform child in textGrid.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        for (int i = 0; i < replayer.GetPlayListLength(); i ++)
        {
            AddTextItem(i);
        }
        SelectItem(currentId);
    }

    void AddTextItem(int id)
    {
        HeadPosInfo localInfo = replayer.GetReplayInfo(id);
        //idText.text = (currentId + 1).ToString() + " / " + replayer.GetPlayListLength().ToString();
        System.DateTime localDate = new System.DateTime((long)localInfo.endTime);
        string endTime = localDate.ToString("G");
        GameObject obj = new GameObject();
        obj = Instantiate(textItemPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
        obj.transform.GetChild(0).gameObject.SetActive(false);
        obj.transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text = (id + 1).ToString();
        obj.transform.GetChild(2).GetComponent<UnityEngine.UI.Text>().text = localInfo.id;
        obj.transform.GetChild(3).GetComponent<UnityEngine.UI.Text>().text = endTime;
        UnityEngine.UI.Button btn = obj.GetComponent<UnityEngine.UI.Button>();
        //Debug.Log(endTime);
        btn.onClick.AddListener(delegate () {
            if (!replayer.isReplaying())
            {
                this.DeselectPrev();
                this.SelectSelf(obj);
                this.PanelUpdateText();
                replayer.SetCurrentId(currentId);
                replayer.ReplaySnapshot();
            }
            //prevSelected = id;
        });
        obj.transform.SetParent(textGrid.transform);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.localScale = new Vector3(1f, 1f, 1f);
        rt.localRotation = new Quaternion(0, 0, 0, 0);
        rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, 0);
    }

    void SelectSelf(GameObject obj)
    {
        obj.transform.GetChild(0).gameObject.SetActive(true);
        currentId = int.Parse(obj.transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text) - 1;
        //SetId(int.Parse(obj.transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text) - 1);
        //Debug.Log(currentId);
    }

    void SelectItem(int id)
    {
        if (id >= 0 && id < textGrid.transform.childCount)
        {
            Debug.Log(textGrid.transform.childCount);
            textGrid.transform.GetChild(id).GetChild(0).gameObject.SetActive(true);
        }
    }

    void DeselectPrev()
    {
        if (currentId >= 0 && currentId < textGrid.transform.childCount)
        {
            //Debug.Log("Deselect " + currentId);
            textGrid.transform.GetChild(currentId).GetChild(0).gameObject.SetActive(false);
        }
    }

    public void PanelUpdateText()
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
