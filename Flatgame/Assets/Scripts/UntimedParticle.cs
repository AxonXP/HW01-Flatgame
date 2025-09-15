using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UntimedParticle : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        GetComponent<ParticleSystem>().Simulate(Time.unscaledDeltaTime, true, false);
    }
}