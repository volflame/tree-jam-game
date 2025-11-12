using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeKeeper : MonoBehaviour
{
    // Start is called before the first frame update
    public Slider slider;
    public float sliderValue = 0.0005f;
    public GameObject popUpText;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        slider.value += sliderValue;
        if (slider.value == 1f)
        {
            popUpText.SetActive(true);
        }
    }
}
