using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : Singleton<SceneController>
{
    public void LoadScene(string snece, float waitTime)
    {
        if(waitTime > 0)
            StartCoroutine(LoadSceneAfter(snece, waitTime));
        else SceneManager.LoadScene(snece);
    }
    public void LoadScene(int sneceId, float waitTime)
    {
        if (waitTime > 0)
            StartCoroutine(LoadSceneAfter(sneceId, waitTime));
        else SceneManager.LoadScene(sneceId);
    }
    public void LoadNextScene(float waitTime)
    {
        int sceneToLoad = SceneManager.GetActiveScene().buildIndex + 1;
        if (sceneToLoad >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning("No scene avaliable for loading");
            return;
        }
        LoadScene(sceneToLoad, waitTime);
    }
    public void LoadPreviousScene(float waitTime)
    {
        int sceneToLoad = SceneManager.GetActiveScene().buildIndex - 1;
        if (sceneToLoad < 0)
        {
            Debug.LogWarning("No scene avaliable for loading");
            return;
        }
        LoadScene(sceneToLoad, waitTime);
    }
    public IEnumerator LoadSceneAfter(string sneceName, float time)
    {
        yield return new WaitForSeconds(time);
        SceneManager.LoadScene(sneceName);
    }
    public IEnumerator LoadSceneAfter(int sneceId, float time)
    {
        yield return new WaitForSeconds(time);
        SceneManager.LoadScene(sneceId);
    }
    public void RestartScene(float waitTime)
    {
        string sceneToLoad = SceneManager.GetActiveScene().name;
        LoadScene(sceneToLoad, waitTime);
    }
}
