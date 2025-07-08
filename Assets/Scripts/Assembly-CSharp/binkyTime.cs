using UnityEngine;
using System.Collections;

public class PhysicsStepper : MonoBehaviour
{
    public static PhysicsStepper instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // optional if you want it persistent
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Step()
    {
        StartCoroutine(StepPhysicsTick());
    }

    private IEnumerator StepPhysicsTick()
    {
        Time.timeScale = 1f;
        yield return new WaitForFixedUpdate();
        Time.timeScale = 0f;
    }
}