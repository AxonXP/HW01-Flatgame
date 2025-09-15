using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeFreezer : MonoBehaviour
{
    public void FreezeTime(float duration, float power)
    {
        StopAllCoroutines();
        StartCoroutine(DoTimeFreeze(duration, power));
    }
    private IEnumerator DoTimeFreeze(float duration, float power)
    {
        Time.timeScale = power;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1;
    }
    public void StopTimeFreeze()
    {
        StopAllCoroutines();
        Time.timeScale = 1;
    }
}