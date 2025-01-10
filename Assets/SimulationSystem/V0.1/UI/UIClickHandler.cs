using Oculus.Interaction;
using SimulationSystem.V0._1.Manager;
using UnityEngine;

namespace SimulationSystem.V0._1.UI
{
    public class UIClickHandler : MonoBehaviour
    {
        [SerializeField] AudioClip clickSoundDown;
        [SerializeField] AudioClip clickSoundUp;

        private void Start()
        {
            if (TryGetComponent(out PointableUnityEventWrapper eventWrapper))
            {
                eventWrapper.WhenSelect.AddListener(OnSelectPlay);
                eventWrapper.WhenUnselect.AddListener(OnReleasePlay);
            }
        }

        public void OnSelectPlay(PointerEvent arg0)
        {
            GameManager.Instance.AudioManager.PlayEffect(clickSoundDown);
        }

        public void OnReleasePlay(PointerEvent arg0)
        {
            GameManager.Instance.AudioManager.PlayEffect(clickSoundUp);
        }
    }
}