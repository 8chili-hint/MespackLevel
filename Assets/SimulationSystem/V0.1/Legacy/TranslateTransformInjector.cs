using Oculus.Interaction;
using UnityEngine;

namespace SimulationSystem.V0._1.Legacy
{
    public class TranslateTransformInjector : MonoBehaviour
    {
        [SerializeField] private OneGrabTranslateTransformer translator;
        [SerializeField] private OneGrabTranslateTransformer.OneGrabTranslateConstraints newConstraints;

        public void UpdateConstraints()
        {
            translator.InjectOptionalConstraints(newConstraints);
        }
    }
}