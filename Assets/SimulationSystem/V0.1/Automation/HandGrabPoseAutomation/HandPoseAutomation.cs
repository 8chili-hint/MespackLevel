#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Oculus.Interaction.Input;
using UnityEngine;
using System.Linq;
using SimulationSystem.V0._1.Utility.Miscellanous;
using SimulationSystem.V0._1.Simulation;
using SimulationSystem.V0._1.Modules.Detect;
using Oculus.Interaction;
using UnityEditor;
using Oculus.Interaction.HandGrab.Editor;
using Oculus.Interaction.HandGrab;

public class HandPoseAutomation : MonoBehaviour
{
    public bool allowPlayModeRecording;
    public Hand rightRigHand;
    public Hand leftRigHand;
    public HandPoseData collectionsData;
    public Transform rigTransform;
    public Vector3 offsetFromRig;
    public List<Rigidbody> rigidBodies; 

    private HandGrabPoseWizard _wizardWindow;
    private int _index;
    private bool _allowLeftToRecord;

    private void Start()
    {
        if(allowPlayModeRecording)
        {
            _index = 0;
            _allowLeftToRecord = true;
            _wizardWindow = EditorWindow.GetWindow<HandGrabPoseWizard>();
            collectionsData.collections = new List<HandGrabInteractableDataCollection>();

            if(rigidBodies.Count > 0)
            {
                rigidBodies[_index].transform.position = rigTransform.position
                                                            + offsetFromRig;
            }
        }
    }

    private void Update()
    {
        if(allowPlayModeRecording && _index < rigidBodies.Count)
        {
            bool rightTrigger = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RTouch);
            bool leftTrigger = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.LTouch);

            if (leftTrigger && _allowLeftToRecord)
            {
                _allowLeftToRecord = false;
                _wizardWindow.Hand = rightRigHand;
                _wizardWindow.SetRigidbody(rigidBodies[_index]);
                _wizardWindow.RecordPose();
            }

            if (rightTrigger && !_allowLeftToRecord)
            {
                _allowLeftToRecord = true;
                _wizardWindow.Hand = leftRigHand;
                _wizardWindow.RecordPose();

                _wizardWindow.SaveToAssetExposed();
                collectionsData.collections.Add(_wizardWindow.GetPoseCollection());
                _wizardWindow.SetPoseCollection(null);

                rigidBodies[_index].gameObject.SetActive(false);
                _index++;
                if (_index < rigidBodies.Count)
                {
                    rigidBodies[_index].transform.position = rigTransform.position
                        + offsetFromRig;
                }
            }
        }
    }

    [ContextMenu("Load Poses from collections")]
    private void LoadPoses()
    {
        _wizardWindow = EditorWindow.GetWindow<HandGrabPoseWizard>();

        for (int i = 0; i < rigidBodies.Count; i++)
        {
            _wizardWindow.SetRigidbody(rigidBodies[i]);
            _wizardWindow.SetPoseCollection(collectionsData.collections[i]);
            _wizardWindow.LoadFromAssetExposed();

            Transform tmp = rigidBodies[i].transform;
            Transform leftInt = tmp.GetChild(4);
            Transform rightInt = tmp.GetChild(5);
            leftInt.transform.parent = tmp.GetChild(2);
            rightInt.transform.parent = tmp.GetChild(2);
        }
    }

}
#endif
