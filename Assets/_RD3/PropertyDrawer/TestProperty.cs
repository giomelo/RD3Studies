using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestProperty : MonoBehaviour
{
    [ProgressBar(0, 100)]
    public float health = 10;
    
    public float minValue = 0;
    public float maxValue = 100;

    [DynamicRange("minValue", "maxValue")]
    public float dynamicValue = 50;
    
    public Mood currentMood;
}
