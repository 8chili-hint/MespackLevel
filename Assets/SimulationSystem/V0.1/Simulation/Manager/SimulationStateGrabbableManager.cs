using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using SimulationSystem.V0._1.Modules.Grab.Utility;
using UnityEngine;

namespace SimulationSystem.V0._1.Simulation.Manager
{
    public static class SimulationStateGrabbableManager
    {
        #region Variable Instantiation
        
        public static readonly Dictionary<Grabbable, GrabbableSubComponents> grabbableComponents =
            new Dictionary<Grabbable, GrabbableSubComponents>();

        public static SimulationManager simulationManager;

        #endregion

        #region Public Enable Grabbable Functions

        public static void EnableGrabbable(Grabbable grabbable)
        {
            SwitchGrabbable(grabbableComponents[grabbable], true);
            grabbable.GetComponentInChildren<Collider>().enabled = true;

        }

        public static void DisableGrabbable(Grabbable grabbable)
        {
            //PRev

            /*  DisableAfterUnGrabbedAsync(grabbableComponents[grabbable]);
              grabbable.GetComponentInChildren<Collider>().enabled = false;*/


            //Changed

            DisableAfterUnGrabbedAsync(grabbableComponents[grabbable]);
            //add to sim sys
            if (grabbable.GetComponentInChildren<Collider>())
                grabbable.GetComponentInChildren<Collider>().enabled = false;
        }
     
        public static void ForceDisableGrabbable(Grabbable grabbable)
        {
            SwitchGrabbable(grabbableComponents[grabbable], false);
        }

        #endregion

        #region Switch Grabbable Function

        private static async Task DisableAfterUnGrabbedAsync(GrabbableSubComponents grabbableComponent)
        {
            while (grabbableComponent.isGrabbed)
            {
                await Task.Delay(100);
            }
            SwitchGrabbable(grabbableComponent, false);
        }

        private static void SwitchGrabbable(GrabbableSubComponents grabbableComponent, bool shouldEnable)
        {
            foreach (var handGrabInteractable in grabbableComponent.handGrabInteractables)
            {
                handGrabInteractable.enabled = shouldEnable;
            }

            foreach (var grabVisualizationHoverEvent in grabbableComponent.grabVisualizationHoverEvents)
            {
                grabVisualizationHoverEvent.isGrabbable = shouldEnable;
                if (!shouldEnable) grabVisualizationHoverEvent.ToggleVisualization(false);
            }

            if (SimulationManager.instance.isAssessmentMode) return;

            EnableGrabVisualisations(shouldEnable, grabbableComponent);
        }

        #endregion
 
        #region GrabVisualisation

        public static void EnableGrabVisualisations(bool shouldEnable, GrabbableSubComponents grabbableComponent)
        {
            foreach (var visualisation in grabbableComponent.ghostVisualisations)
            {
                if(!grabbableComponent.isGrabbed) visualisation.gameObject.SetActive(shouldEnable);
            }
        }

        #endregion

        #region Assessment
        
        public static void DisableAllUnAssessedGrabbables()
        {
            foreach (var grabbableComponent in grabbableComponents)
            {
                if (simulationManager.currentStateIndex != 0)
                {
                    if (simulationManager.simulationStates[simulationManager.currentStateIndex - 1]
                        .stateGrabbables.Contains(grabbableComponent.Key))
                    {
                        if (!simulationManager.simulationStates[simulationManager.currentStateIndex]
                            .stateGrabbables.Contains(grabbableComponent.Key))
                        {
                            DisableGrabbable(grabbableComponent.Key);
                            continue;   
                        }
                    }
                }
                
                if(!SimulationManager.instance.currentState.stateGrabbables.Contains(grabbableComponent.Key)) DisableGrabbable(grabbableComponent.Key);
            }
        }
        
        public static void EnableAllUnAssessedGrabbables()
        {
            foreach (var grabbableComponent in grabbableComponents)
            {
                if(grabbableComponent.Value.shouldEnable) EnableGrabbable(grabbableComponent.Key);
            }
        }

        #endregion
    }

    [Serializable]
    public class GrabbableSubComponents
    {
        public readonly List<HandGrabInteractable> handGrabInteractables = new List<HandGrabInteractable>();
        public readonly List<GameObject> ghostVisualisations = new List<GameObject>();

        public readonly List<GrabVisualizationHandler> grabVisualizationHoverEvents =
            new List<GrabVisualizationHandler>();

        public bool isGrabbed;
        public bool shouldEnable = true;
    }
}