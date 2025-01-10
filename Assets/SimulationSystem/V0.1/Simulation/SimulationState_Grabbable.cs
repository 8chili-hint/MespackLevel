using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using SimulationSystem.V0._1.Assessment.Assessment_Types;
using SimulationSystem.V0._1.Assessment.Utility;
using SimulationSystem.V0._1.Modules.Grab.Utility;
using SimulationSystem.V0._1.Simulation.Manager;
using UnityEngine;

namespace SimulationSystem.V0._1.Simulation
{
    // Handles State Grabbables
    public partial class SimulationState
    {
       
        #region Configure grabbable

        public void ConfigureGrabbable()
        {
            onStateComplete.AddListener(()=> EnableResetOnRelease());
            foreach (var stateGrabbable in stateGrabbables)
            {
                if (SimulationStateGrabbableManager.grabbableComponents.ContainsKey(stateGrabbable)) continue;

                var grabbableComponent = new GrabbableSubComponents();
                
                grabbableComponent.handGrabInteractables.AddRange(stateGrabbable.GetComponentsInChildren<HandGrabInteractable>(true));

                var grabVisualisationList = stateGrabbable.transform.GetComponentsInChildren<GrabVisualizationHandler>(true);
                grabbableComponent.grabVisualizationHoverEvents.AddRange(grabVisualisationList);

                foreach (var grabVisualisation in grabVisualisationList)
                {
                    grabVisualisation.AddOutlineComponents();
                }

                var ghostVisualisationParent = stateGrabbable.transform.Find("Grab visualisation");
                foreach (Transform child in ghostVisualisationParent)
                {
                    grabbableComponent.ghostVisualisations.Add(child.gameObject);
                }
                
                var pointableUnityEventWrapper = stateGrabbable.GetComponent<PointableUnityEventWrapper>();
                
                pointableUnityEventWrapper.WhenSelect.AddListener(arg0 =>
                {
                    foreach (var visualisation in grabbableComponent.ghostVisualisations)
                    {
                        visualisation.gameObject.SetActive(false);
                    }

                    grabbableComponent.isGrabbed = true;
                    
                    if(SimulationManager.instance.currentState.GetComponent<GrabAssessment>()) GrabAssessmentManager.CheckForGrabError(stateGrabbable);
                });
                
                pointableUnityEventWrapper.WhenUnselect.AddListener(arg0 =>
                {
                    grabbableComponent.isGrabbed = false;
                });
                
                SimulationStateGrabbableManager.grabbableComponents.Add(stateGrabbable, grabbableComponent);

                if(!SimulationManager.instance.isAssessmentMode) SimulationStateGrabbableManager.DisableGrabbable(stateGrabbable);
            }
        }

        #endregion
        
        #region Enable Grabbable Functions
        
        private void  EnableStateGrabbables(bool shouldEnable)
        {
            foreach (var stateGrabbable in stateGrabbables)
            {
                if (!shouldEnable)
                {
                    if (SimulationManager.instance.currentStateIndex == SimulationManager.instance.simulationStates.Count - 1) continue;
                    
                    var nextState = SimulationManager.instance.simulationStates[SimulationManager.instance.currentStateIndex + 1];
                    var isGrabbableInNextState = nextState.stateGrabbables.Contains(stateGrabbable);
                   
                    if (!isGrabbableInNextState)
                    {                    
                        SimulationStateGrabbableManager.DisableGrabbable(stateGrabbable);
                    }
                    
                    stateGrabbable.GetComponent<PointableUnityEventWrapper>().WhenSelect.RemoveListener(CheckForNextStateOnGrab);
                }
                else
                {
                    SimulationState previousState;
                    bool isGrabbableInPreviousState = false;
                    if (SimulationManager.instance.currentStateIndex == 0)
                    {
                        previousState = SimulationManager.instance.simulationStates[SimulationManager.instance.currentStateIndex - 1];
                        isGrabbableInPreviousState = previousState.stateGrabbables.Contains(stateGrabbable);
                    }


                    if (!isGrabbableInPreviousState) SimulationStateGrabbableManager.EnableGrabbable(stateGrabbable);

                    stateGrabbable.GetComponent<PointableUnityEventWrapper>().WhenSelect.AddListener(CheckForNextStateOnGrab);
                }
            }
        }
        
        private void CheckForNextStateOnGrab(PointerEvent arg0)
        {
            
            if (nextStateOnObjectGrab)
            {/*
                if (SimulationManager.instance.currentStateIndex == 8 || SimulationManager.instance.currentStateIndex == 7 || SimulationManager.instance.currentStateIndex == 48 || SimulationManager.instance.currentStateIndex == 51)
                {
                    return;
                }*/
               foreach(Grabbable atmep in stateGrabbables)
                { 
                    atmep.GetComponent<PointableUnityEventWrapper>().WhenSelect.RemoveListener(CheckForNextStateOnGrab);
                }
                SimulationManager.instance.NextState();


            }
        }
        
        #endregion



        public void EnableResetOnRelease()
        {
            if (SimulationManager.instance.GrabbableShouldNotResetInItsRespectiveStep)
            {
                foreach (var StateGrabbable in stateGrabbables)
                {
                    StateGrabbable.GetComponent<ObjectMovementHelper>().ResetThisObjectOnRelease = true;
                }
            }
        }


        public void EnableGrabbable(Grabbable grabbable)
        {
            SimulationStateGrabbableManager.EnableGrabbable(grabbable);
        }

        public void DisableGrabbable(Grabbable grabbable)
        {
            SimulationStateGrabbableManager.ForceDisableGrabbable(grabbable);
        }


        private void setGrabbableHelperEvents()
        {
            foreach (var GrabbableHelper in GrabbableHelper)
            {

                if (GrabbableHelper.SelectBehaviour == GrabbableHelperStateBehaviourContainer.Enable)
                {

                    if (GrabbableHelper.SelectEventTrigger == GrabbableHelperStateEventsContainer.OnStateStart)
                    {
                        onStateStart.AddListener
                        (() =>
                        {
                            if (GrabbableHelper.ApplyInAssessmentModeToo)
                            {

                                EnableGrabbable(GrabbableHelper.Grabbable);

                            }
                            else
                            {
                                if (!SimulationManager.instance.isAssessmentMode)
                                {
                                    EnableGrabbable(GrabbableHelper.Grabbable);
                                }
                                else
                                {
                                    return;
                                }

                            }
                        }


                        );
                    }
                    else if (GrabbableHelper.SelectEventTrigger == GrabbableHelperStateEventsContainer.OnstateEnd)
                    {
                        onStateComplete.AddListener
                         (() =>
                         {
                             if (GrabbableHelper.ApplyInAssessmentModeToo)
                             {

                                 EnableGrabbable(GrabbableHelper.Grabbable);

                             }
                             else
                             {
                                 if (!SimulationManager.instance.isAssessmentMode)
                                 {
                                     EnableGrabbable(GrabbableHelper.Grabbable);
                                 }
                                 else
                                 {
                                     return;
                                 }

                             }
                         }


                        );
                    }
                    else if (GrabbableHelper.SelectEventTrigger == GrabbableHelperStateEventsContainer.Both)
                    {
                        onStateStart.AddListener(() =>
                        {
                            if (GrabbableHelper.ApplyInAssessmentModeToo)
                            {

                                EnableGrabbable(GrabbableHelper.Grabbable);

                            }
                            else
                            {
                                if (!SimulationManager.instance.isAssessmentMode)
                                {
                                    EnableGrabbable(GrabbableHelper.Grabbable);
                                }
                                else
                                {
                                    return;
                                }

                            }
                        }


                        );

                        onStateComplete.AddListener
                             (() =>
                             {
                                 if (GrabbableHelper.ApplyInAssessmentModeToo)
                                 {

                                     EnableGrabbable(GrabbableHelper.Grabbable);

                                 }
                                 else
                                 {
                                     if (!SimulationManager.instance.isAssessmentMode)
                                     {
                                         EnableGrabbable(GrabbableHelper.Grabbable);
                                     }
                                     else
                                     {
                                         return;
                                     }

                                 }
                             }


                            );
                    }



                }
                else if (GrabbableHelper.SelectBehaviour == GrabbableHelperStateBehaviourContainer.Disable)
                {

                    if (GrabbableHelper.SelectEventTrigger == GrabbableHelperStateEventsContainer.OnStateStart)
                    {
                        onStateStart.AddListener
                        (() =>
                        {
                            if (GrabbableHelper.ApplyInAssessmentModeToo)
                            {

                                DisableGrabbable(GrabbableHelper.Grabbable);

                            }
                            else
                            {
                                if (!SimulationManager.instance.isAssessmentMode)
                                {
                                    DisableGrabbable(GrabbableHelper.Grabbable);
                                }
                                else
                                {
                                    return;
                                }

                            }
                        }


                        );
                    }
                    else if (GrabbableHelper.SelectEventTrigger == GrabbableHelperStateEventsContainer.OnstateEnd)
                    {
                        onStateComplete.AddListener
                         (() =>
                         {
                             if (GrabbableHelper.ApplyInAssessmentModeToo)
                             {

                                 DisableGrabbable(GrabbableHelper.Grabbable);

                             }
                             else
                             {
                                 if (!SimulationManager.instance.isAssessmentMode)
                                 {
                                     DisableGrabbable(GrabbableHelper.Grabbable);
                                 }
                                 else
                                 {
                                     return;
                                 }

                             }
                         }


                        );
                    }
                    else if (GrabbableHelper.SelectEventTrigger == GrabbableHelperStateEventsContainer.Both)
                    {
                        onStateStart.AddListener(() =>
                        {
                            if (GrabbableHelper.ApplyInAssessmentModeToo)
                            {

                                DisableGrabbable(GrabbableHelper.Grabbable);

                            }
                            else
                            {
                                if (!SimulationManager.instance.isAssessmentMode)
                                {
                                    DisableGrabbable(GrabbableHelper.Grabbable);
                                }
                                else
                                {
                                    return;
                                }

                            }
                        }


                        );

                        onStateComplete.AddListener
                             (() =>
                             {
                                 if (GrabbableHelper.ApplyInAssessmentModeToo)
                                 {

                                     DisableGrabbable(GrabbableHelper.Grabbable);

                                 }
                                 else
                                 {
                                     if (!SimulationManager.instance.isAssessmentMode)
                                     {
                                         DisableGrabbable(GrabbableHelper.Grabbable);
                                     }
                                     else
                                     {
                                         return;
                                     }

                                 }
                             }


                            );
                    }



                }

            }
        } 
    }
}