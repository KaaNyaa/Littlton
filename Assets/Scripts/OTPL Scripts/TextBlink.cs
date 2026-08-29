using UnityEngine;
using TMPro;

public class TextBlink : MonoBehaviour
{
    private TextMeshProUGUI text;
    void Start() { text = GetComponent<TextMeshProUGUI>(); }

    void Update()
    {
        // Simple sin wave to fade alpha in and out
        float alpha = (Mathf.Sin(Time.time * 3f) + 1f) / 2f;
        text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
    }
}