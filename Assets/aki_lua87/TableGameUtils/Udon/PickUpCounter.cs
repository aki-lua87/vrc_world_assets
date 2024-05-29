
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PickUpCounter : UdonSharpBehaviour
{
    [SerializeField] private int initialCount = 0;
    public Text counterText;
    const float longPressTimeThreashold = 1.5f;
    private float pressingTime;
    private bool isPressingState = false;

    [UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(CountData))] private int _countData;
    public int CountData
    {
        get => _countData;
        set
        {
            _countData = value;
            DisplayCountData();
        }
    }

    void Start()
    {
        if (Networking.LocalPlayer.IsOwner(this.gameObject))
        {
            CountData = initialCount;
        }
        pressingTime = float.NaN;
    }
    void Update()
    {
        if (isPressingState)
        {
            pressingTime += Time.deltaTime;
            if (pressingTime > longPressTimeThreashold)
            {
                pressingTime = 0f;
                CounterResetEvent();
            }
        }
    }

    public void OnPickupUseDown_Event()
    {
        isPressingState = true;
        CounUpEvent();
    }

    public void OnPickupUseUp_Event()
    {
        isPressingState = false;
        pressingTime = 0f;
    }

    public void DisplayCountData()
    {
        counterText.text = _countData.ToString();
    }

    public void CounterReset()
    {
        CountData = initialCount;
        RequestSerialization();
    }

    public void CounterResetEvent()
    {
        if (!Networking.IsOwner(Networking.LocalPlayer, this.gameObject)) Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        if (Networking.LocalPlayer.IsOwner(this.gameObject))
        {
            CounterReset();
        }
        else
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(CounterReset));
        }
    }

    public void CountUp()
    {
        CountData++;
        RequestSerialization();
    }

    public void CounUpEvent()
    {
        if (Networking.LocalPlayer.IsOwner(this.gameObject))
        {
            CountUp();
        }
        else
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(CountUp));
        }
    }
}
