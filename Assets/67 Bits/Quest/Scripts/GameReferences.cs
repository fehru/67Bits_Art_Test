using Cinemachine;
using UnityEngine;

namespace SSBQuests
{
    public class GameReferences : Singleton<GameReferences>
    {
        public static CinemachineVirtualCamera CinemachineVirtualCamera
        {
            get
            {
                if (!_cinemachineCamera) _cinemachineCamera = FindObjectOfType<CinemachineVirtualCamera>();
                return _cinemachineCamera;
            }
            set { _cinemachineCamera = value; }
        }
        private static CinemachineVirtualCamera _cinemachineCamera;
        public static Transform PlayerTransform { get; set; }
    }
}
