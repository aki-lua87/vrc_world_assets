
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Counter : UdonSharpBehaviour
{
    [UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(CountData))] private int _countData;
    public Text counterText;
    public int defaultCount = 0;
    public int maxCount = 100;
    public int minCount = -100;

    public void Start()
    {
        Debug.Log($"[aki_lua87] Counter: Start");
        if (Networking.LocalPlayer.IsOwner(this.gameObject))
        {
            Debug.Log($"[aki_lua87] Counter: IsOwner");
            CountData = defaultCount;
            RequestSerialization();
        }
    }

    public void CounterResetButton()
    {
        SetOwner();
        CounterReset();
        // if (!Networking.IsOwner(Networking.LocalPlayer, this.gameObject)) Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        // if (Networking.LocalPlayer.IsOwner(this.gameObject))
        // {
        //     CounterReset();
        // }
        // else
        // {
        //     SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(CounterReset));
        // }
    }

    public void CounterReset()
    {
        CountData = defaultCount;
        RequestSerialization();
    }

    public int CountData
    {
        get => _countData;
        set
        {
            _countData = value;
            DisplayCountData();
        }
    }

    public void CounUpButton()
    {
        SetOwner();
        CountUp();
    }

    public void CountUp()
    {
        if (CountData >= maxCount) return;
        CountData++;
        RequestSerialization();
    }

    public void CountDownButton()
    {
        SetOwner();
        CountDown();
    }

    public void CountDown()
    {
        if (CountData <= minCount) return;
        CountData--;
        RequestSerialization();
    }

    public void DisplayCountData()
    {
        counterText.text = _countData.ToString();   // データ表示更新
    }

    private void SetOwner()
    {
        Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
    }
}
