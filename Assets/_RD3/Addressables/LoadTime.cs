using System;
using TMPro;
using UnityEngine;

public class LoadTime : MonoBehaviour
{
    private TextMeshProUGUI _timeText;

    private void Awake()
    {
        _timeText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        _timeText.text = Time.realtimeSinceStartup.ToString();
    }
}
