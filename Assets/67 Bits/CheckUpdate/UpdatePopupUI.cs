using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using Newtonsoft.Json;

namespace UpgradeSystem
{
    public class UpdatePopupUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject _canvas;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Button _notNowButton;
        [SerializeField] private Button _updateButton;

        private string _jsonDataURL = "https://drive.google.com/uc?export=download&id=1QcshCr7eNeappf6PZW4HPy7wJ1DvaEjy";
        private string _googlePlayURL = "https://play.google.com/store/apps/details?id=com.sixtysevenbits.zombieworld";

        private void Start()
        {
            _canvas.SetActive(false);
            StartCoroutine(DownloadJson());
        }
        private IEnumerator DownloadJson()
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(_jsonDataURL))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string jsonText = webRequest.downloadHandler.text;
                    Debug.Log("JSON baixado: " + jsonText);
                    var buildInfo = JsonConvert.DeserializeObject<BuildInfo>(jsonText);
                    if (!IsSameVersion(buildInfo.Version))
                        ShowPopup();
                }
                else
                {
                    Debug.LogError("Erro ao baixar o arquivo JSON: " + webRequest.error);
                }
            }
        }
        private bool IsSameVersion(string version)
        {
            return Application.version.Equals(version);
        }
        public void ShowPopup()
        {
            _notNowButton.onClick.AddListener(() => {
                HidePopup();
            });

            _updateButton.onClick.AddListener(() => {
                Application.OpenURL(_googlePlayURL);
                HidePopup();
            });

            _canvas.SetActive(true);
        }
        public void HidePopup()
        {
            _canvas.SetActive(false);
            _notNowButton.onClick.RemoveAllListeners();
            _updateButton.onClick.RemoveAllListeners();
        }
    }
}