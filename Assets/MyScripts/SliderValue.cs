using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliderValue : MonoBehaviour
{
    public UnityEngine.UI.Text text;
    public DatabaseSync db;

    // Start is called before the first frame update
    void Start()
    {
        UpdateSliderValue(GetComponent<UnityEngine.UI.Slider>().value);

        GetComponent<UnityEngine.UI.Slider>().onValueChanged.AddListener(UpdateSliderValue);
        GetComponent<UnityEngine.UI.Slider>().onValueChanged.AddListener(db.SetDeltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateSliderValue(float v)
    {
        text.text = v.ToString();
    }
}
