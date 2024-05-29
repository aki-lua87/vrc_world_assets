
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class PickUpEventSender : UdonSharpBehaviour
{
    [SerializeField] private UdonBehaviour targetUdonBehaviour;

    public override void OnPickupUseDown()
    {
        targetUdonBehaviour.SendCustomEvent("OnPickupUseDown_Event");
    }

    public override void OnPickupUseUp()
    {
        targetUdonBehaviour.SendCustomEvent("OnPickupUseUp_Event");
    }
}
