using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SplashScreen : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private CanvasGroup logoGroup; // Logo Image here
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private float stayDuration = 2.0f;
    [SerializeField] private string nextSceneName = "StartScreen";

    void Start()
    {
        if (logoGroup != null)
        {
            StartCoroutine(FadeSequence());
        }
    }

    IEnumerator FadeSequence()
    {
        float counter = 0f;
        while (counter < fadeDuration)
        {
            counter += Time.deltaTime;
            logoGroup.alpha = Mathf.Lerp(0, 1, counter / fadeDuration);
            yield return null;
        }

        yield return new WaitForSeconds(stayDuration);

        if (SceneChanger.Instance != null)
        {
            SceneChanger.Instance.MoveToScene(nextSceneName);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }
}