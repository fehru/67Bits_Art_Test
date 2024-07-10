using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitializationScene : MonoBehaviour
{
    [SerializeField] private bool goToNextScene;
    private void Awake()
    {
        if (goToNextScene) SceneController.Instance.LoadNextScene(0);
    }
}
