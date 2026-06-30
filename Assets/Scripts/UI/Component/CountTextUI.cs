using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CountTextUI : MonoBehaviour
{
    [SerializeField]
    protected ObservableValue source;

    [SerializeField]
    protected TMP_Text tmp;

    protected void Start()
    {
        Initialize();

        tmp.text = source.CurrentValue.ToString();

        source.OnValueChanged += Refresh;
    }

    protected void OnDestroy()
    {
        source.OnValueChanged -= Refresh;
    }

    protected virtual void Refresh(float current, float max)
    {
        tmp.text = current.ToString();
    }

    protected virtual void Initialize() { }
}
