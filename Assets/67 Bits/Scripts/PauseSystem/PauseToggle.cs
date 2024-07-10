using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IIS.TimeControl
{
    public class PauseToggle : MonoBehaviour
    {
        [SerializeField] private UnityEvent pauseUnityEvent;
        [SerializeField] private UnityEvent unpauseUnityEvent;
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button continueButton;

        private float _timescaleCache;

        private void OnEnable()
        {
            pauseButton.onClick.AddListener(() => SetPause(true));
            continueButton.onClick.AddListener(() => SetPause(false));
        }

        private void OnDisable()
        {
            pauseButton.onClick.RemoveAllListeners();
            continueButton.onClick.RemoveAllListeners();
        }

        public void SetPause(bool pausing)
        {
            if(pausing)
            {
                GameManager.PlayEvent(GameManager.GameEvent.GamePaused);

                _timescaleCache = Time.timeScale;
                Time.timeScale = 0f;

                pauseMenu.SetActive(true);

                continueButton.interactable = true;
                pauseButton.interactable = false;

                pauseUnityEvent.Invoke();
            }
            else
            {
                GameManager.PlayEvent(GameManager.GameEvent.GamePaused);

                Time.timeScale = _timescaleCache;

                pauseButton.interactable = true;
                continueButton.interactable = false;

                unpauseUnityEvent.Invoke();
            }
        }
    }
}