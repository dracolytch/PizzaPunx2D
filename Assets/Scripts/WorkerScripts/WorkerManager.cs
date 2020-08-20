using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerManager : MonoBehaviour
{
    public DoughMakerScript DoughBoy;
    public Deliverator Delivery;
    public Supplier Supplies;
    public WorkerScript Worker1;
    public WorkerScript Worker2;
    public WorkerScript Worker3;

    public void UpgradeWorkers(GameState state)
    {
        if (state.HasDoughMachine) DoughBoy.Upgrade();
        if (state.HasSwarm) Delivery.Upgrade();
        if (state.HasSupplyBot) Supplies.Upgrade();

        if (state.NumToppersReplaced > 0) Worker1.Upgrade();
        if (state.NumToppersReplaced > 1) Worker2.Upgrade();
        if (state.NumToppersReplaced > 2) Worker3.Upgrade();
    }
}
