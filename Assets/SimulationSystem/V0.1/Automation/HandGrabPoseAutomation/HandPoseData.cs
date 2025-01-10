using System.Collections.Generic;
using Oculus.Interaction.HandGrab;
using UnityEngine;

[CreateAssetMenu]
public class HandPoseData : ScriptableObject
{
    public List<HandGrabInteractableDataCollection> collections;
}

