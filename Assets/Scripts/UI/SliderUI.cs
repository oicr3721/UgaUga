using UnityEngine;
using UnityEngine.UI;

public class SliderUI : MonoBehaviour
{
    [SerializeField]
    private ObservableValue source;

    [SerializeField]
    private Slider slider;

    private void Awake()
    {
        source.OnValueChanged += Refresh;

        Refresh(
            source.CurrentValue,
            source.MaxValue
        );
    }

    private void OnDestroy()
    {
        source.OnValueChanged -= Refresh;
    }

    private void Refresh(
        float current,
        float max
    )
    {
        slider.value =
            current / max;
    }
}