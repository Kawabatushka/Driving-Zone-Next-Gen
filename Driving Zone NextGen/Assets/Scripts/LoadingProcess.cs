using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingProcess : MonoBehaviour
{
    AsyncOperation asyncOperation;
    [SerializeField] Image loadingBar;
    [SerializeField] TMP_Text percentLoadingInfo;
    [SerializeField] string sceneName;

    void Start()
    {
        StartCoroutine(LoadSceneBar());
    }

    IEnumerator LoadSceneBar()
    {
        yield return new WaitForSeconds(1f);
        asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncOperation.isDone)
        {
            var progress = asyncOperation.progress / 0.9f;
            loadingBar.fillAmount = progress;
            percentLoadingInfo.text = string.Format("{0:0}%", progress * 100f);
            yield return 0;
        }
    }
}