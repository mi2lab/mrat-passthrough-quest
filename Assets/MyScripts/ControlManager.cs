using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlManager : MonoBehaviour
{
    private bool show_menu = false;
    public GameObject targetMenu;

    public List<GameObject> menuMapKey;
    public List<GameObject> menuMapVal;
    private int prevMenu = -1;

    public void ToggleMenu()
    {
        targetMenu.SetActive(show_menu);
        show_menu = !show_menu;
    }

    private void Start()
    {
        if (menuMapKey.Count != menuMapVal.Count)
        {
            Debug.LogError("Menu map count doesn't match!");
        }
        else
        {
            for ( int i = 0; i < menuMapKey.Count; i ++)
            {
                int local_id = i;
                //Debug.Log("Hit " + menuMapKey[local_id].name + menuMapVal[local_id].name);
                menuMapKey[local_id].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate () {
                    Debug.Log("Hit " + menuMapVal[local_id].name);
                    this.SwitchMenu(local_id);
                });
            }
        }
    }

    public void SwitchMenu(int id)
    {
        if (prevMenu >= 0)
        {
            menuMapVal[prevMenu].SetActive(false);
        }
        menuMapVal[id].SetActive(true);
        prevMenu = id;
    }

    public void TurnOnMenu(GameObject obj)
    {
        obj.SetActive(true);
    }
}
