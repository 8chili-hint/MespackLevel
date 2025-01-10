using System;
using System.Collections.Generic;
using Oculus.Interaction;
using SimulationSystem.V0._1.Assessment;
using SimulationSystem.V0._1.Assessment.Assessment_Types;
using SimulationSystem.V0._1.Assessment.Interface;
using SimulationSystem.V0._1.Modules.Detect;
using SimulationSystem.V0._1.Simulation.Manager;
using SimulationSystem.V0._1.UI;
using SimulationSystem.V0._1.Utility.Event;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

namespace SimulationSystem.V0._1.Simulation
{
    public partial class SimulationState : MonoBehaviour
    {
        
        #region Variable Declaration

        [SerializeField] public enum StateType
        {
            DetectWithGrab,
            Prompt,
            Grab,
            UI,
            DetectWithHand,
            Gaze
        }
        
        [Header("State Type")]
        
        public StateType stateType;
        
        [Header("[Prompt]")] 

        public AudioClip audioPrompt;
        public string textPrompt;
        public VideoClip videoPrompt;

        [Header("[Events]")] 
        [Space]
        
        public UnityEvent onStateStart = new UnityEvent();
        public UnityEvent onStateComplete = new UnityEvent();
        public List<DelayedEvent> onStateStartDelayedEvents;
        
        [Header("[Changeable States]")]
        public bool playStateInAssessmentMode = true;
        public List<StateUpdater> instructorUpdater = new List<StateUpdater>();

        [Header("[Grabbables]")] 
        public List<Grabbable> stateGrabbables;
        [HideInInspector] public List<Grabbable> allowedStateGrabbables;
        
        [Header("[Assessment]")] 
        [HideInInspector] public bool isAssessed;
        [HideInInspector] public bool shouldShowPrompt = true;
        [HideInInspector] public float maxScore = 0;
        [NonSerialized] public float currentScore;
        [NonSerialized] public AssessmentStatus assessmentStatus = AssessmentStatus.Success;
        [NonSerialized] public string errorMessage = " ";
        
        [Header("[Detect StateType]")] 
        public List<ObjectToDetectPerState> objectToDetectList;
        public bool LeftHand;
        public bool RightHand;
        private List<DetectAbstract> _stateDetects = new List<DetectAbstract>();
        public List<ObjectToDetectPerState> cachedObjectToDetectList;
        [HideInInspector] public DetectObject DetectObject;


        [Header("[Prompt StateType]")]
        [HideInInspector] public bool nextStateOnPromptEnd;
        
        [Header("[Grab StateType]")]
        public bool nextStateOnObjectGrab;

        [Header("[UI StateType]")] 
        [HideInInspector] public UIAnimationHandler uiParentAnimationHandler;
        [HideInInspector] public PokeInteractable buttonPokeInteractable;
        [HideInInspector] public PointableUnityEventWrapper buttonPointableUnityEventWrapper;

        private float _timer;
        //public bool Settings;
        [Header("[Control StateWise Grabbable Toggle]")]
        [SerializeField]public List<GrabbableHelperSettings> GrabbableHelper;

        #endregion


        #region MonoBehaviour

        private void Awake()
        {
            instructorUpdater.ForEach(g => g.changeableState = this);
            if (GetComponents<IAssessment>().Length != 0)
            {
                isAssessed = true;
            }
        
        }

        private void Start()
        {
            currentScore = maxScore;
            AddListners();
            
            //transform.GetChild(0);
        }
        public void AddListners()
        {
            onStateStart.AddListener(() =>
            {
                AssessmentStatepromptEnable();
                

                foreach (var delayedEvent in onStateStartDelayedEvents)
                {
                    delayedEvent.InvokeDelayedEvent();
                }

                EnableStateGrabbables(true);

                if (stateType == StateType.DetectWithGrab || stateType == StateType.DetectWithHand) SwitchStateDetects(true);

                if (stateType == StateType.UI && (playStateInAssessmentMode && SimulationManager.instance.isAssessmentMode))
                {
                    if (TryGetComponent<UIAssessment>(out UIAssessment UA))
                    {
                        UA.DisplayWrongUI();
                    }
                }
                if(stateType == StateType.Gaze)
                {
                    SwitchStateDetects(true);
                    GetRay();
                    StartCoroutine(StartRaycast());
                }

                foreach (var stateGrabbable in stateGrabbables)
                {
                    stateGrabbable.GetComponent<PointableUnityEventWrapper>().WhenSelect.AddListener((PointerEvent arg0) =>
                    {
                        if (stateType == StateType.Grab) { CheckForNextStateOnGrab(arg0); }
                        SimulationStateGrabbableManager.EnableGrabVisualisations(false, SimulationStateGrabbableManager.grabbableComponents[stateGrabbable]);
                    });
                }
            });

            onStateComplete.AddListener(() =>
            {

              
                foreach (var grabbable in stateGrabbables)
                {
                    SimulationStateGrabbableManager.EnableGrabVisualisations(false, SimulationStateGrabbableManager.grabbableComponents[grabbable]);
                }

                if (!SimulationManager.instance.isAssessmentMode) EnableStateGrabbables(false);

                if (stateType == StateType.DetectWithGrab || stateType == StateType.DetectWithHand) SwitchStateDetects(false);

                if (stateType == StateType.Gaze)
                {
                    SwitchStateDetects(false);
                    StopCoroutine(StartRaycast());
                }
            });

            if (stateType == StateType.UI)
            {
                buttonPointableUnityEventWrapper = buttonPokeInteractable.GetComponent<PointableUnityEventWrapper>();
                SetupStateUI();
            }
        }




        #endregion



    }

    [Serializable]
    public class StateUpdater
    {   
        [HideInInspector]
        public SimulationState changeableState;

        [TextArea]
        public string id;
        public AudioClip newInsAudio;
        public VideoClip newInsVideo;
        
        public void UpdateStatePrompt()
        {
            if(newInsAudio)
                changeableState.audioPrompt = newInsAudio;
            if(newInsVideo)
                changeableState.videoPrompt = newInsVideo;
        }


    }
    
    
    [Serializable]
    public struct ObjectToDetectPerState
    {
        public DetectObject detectObject;
        [HideInInspector]public bool detectForAllGameObjectsToDetect;
        public List<GameObject> gameObjectsToDetect;
        [HideInInspector]public List<string> gameObjectToDetectNames;
        public bool shouldMoveToNextState;
       
    }


    [Serializable]
    public struct GrabbableHelperSettings
    {
        public Grabbable Grabbable;
        public GrabbableHelperStateEventsContainer SelectEventTrigger;
        public GrabbableHelperStateBehaviourContainer SelectBehaviour;
        public bool ApplyInAssessmentModeToo;
    }

    [Serializable]
    public enum GrabbableHelperStateEventsContainer
    {
        OnStateStart, OnstateEnd, Both
    }

    [Serializable]
    public enum GrabbableHelperStateBehaviourContainer
    {
        Enable, Disable
    }
}