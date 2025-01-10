using System;
using System.Collections;
using System.Collections.Generic;
using SimulationSystem.V0._1.Assessment;
using SimulationSystem.V0._1.Assessment.Interface;
using SimulationSystem.V0._1.Modules.Detect;
using SimulationSystem.V0._1.Simulation.Manager;
using UnityEngine;
using UnityEngine.UIElements;

namespace SimulationSystem.V0._1.Simulation
{
    public partial class SimulationState
    {
        private GameObject rayObject;
        private void GetRay()
        {
            var objectToDetect = objectToDetectList[0];
            rayObject = objectToDetect.gameObjectsToDetect[0];
        }
        IEnumerator StartRaycast()
        {
            if (rayObject == null)
            {
                yield break;
            }
            else
            {
                while (true)
                {
                    Ray ray = new Ray(rayObject.transform.position, rayObject.transform.forward * 4);
                    Debug.DrawRay(rayObject.transform.position, rayObject.transform.forward * 2, Color.red);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {
                        if (hit.collider.gameObject.name == objectToDetectList[0].detectObject.name)
                        {
                            Debug.Log("Hit object : " + hit.collider.gameObject.name);
                            hit.transform.gameObject.GetComponent<DetectObject>().OnGazeInitiated();
                        }
                        else
                        {
                            Debug.Log(" Did not hit");
                            objectToDetectList[0].detectObject.GetComponent<DetectObject>().OnGazeEnded();
                        }
                    }
                    yield return null;
                }
            }
            
        }
    }
}