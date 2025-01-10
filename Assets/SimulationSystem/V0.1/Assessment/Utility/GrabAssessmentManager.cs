using Oculus.Interaction;
using SimulationSystem.V0._1.Simulation;

namespace SimulationSystem.V0._1.Assessment.Utility
{
    public static class GrabAssessmentManager
    {
        public static void CheckForGrabError(Grabbable grabbable)
        {
            if (!SimulationManager.instance.currentState.stateGrabbables.Contains(grabbable) &&
                !SimulationManager.instance.currentState.allowedStateGrabbables.Contains(grabbable))
            {
                AssessmentManager.DeductScore(AssessmentType.Grab);
            }
        }
    }
}