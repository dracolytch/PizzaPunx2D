using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DataMetrics : MonoBehaviour
{
    public GradeDataManager dataManager;
    public Text textOutput;

    float updateSpeed = 0.5f;
    float elapsed = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed > updateSpeed)
        {
            elapsed = 0f;
            var data = dataManager.reviews.Values.Where(a => a.doNotUse == 0 && a.needsChecking == 0);
            var earlyGood = data.Where(a => a.gameStage == 0 && a.isGood == 1).Count();
            var earlyBad = data.Where(a => a.gameStage == 0 && a.isGood == 0).Count();

            var midGood = data.Where(a => a.gameStage == 1 && a.isGood == 1).Count();
            var midBad = data.Where(a => a.gameStage == 1 && a.isGood == 0).Count();

            var lateGood = data.Where(a => a.gameStage == 2 && a.isGood == 1).Count();
            var lateBad = data.Where(a => a.gameStage == 2 && a.isGood == 0).Count();

            textOutput.text = $"Matrics\r\nEG: {earlyGood}\r\nEB: {earlyBad}\r\nMG: {midGood}\r\nMB: {midBad}\r\nLG: {lateGood}\r\nLB: {lateBad}\r\n";
        }
    }
}
