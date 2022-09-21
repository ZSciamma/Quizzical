// Enables a game object after a specified cooldown

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableAfterWait : MonoBehaviour
{
    [Tooltip("Time until the object is enabled")]
    public float timeUntilAppear;
    public GameObject objectToEnable;

    private float startTime;
    private bool isDone;

    void Awake()
    {
        startTime = Time.time;
        objectToEnable.SetActive(false);
        isDone = false;
    }

    // Make the object appear if timer is elapsed
    void FixedUpdate()
    {
        if (isDone) {
            return;
        }

        if (startTime + timeUntilAppear <= Time.time) {
            // Find button component and enable it
            objectToEnable.SetActive(true);
            isDone = true;
        }
    }
}
