using UnityEngine;
using System.Collections;

public class RotatingLoadingCircle : MonoBehaviour
{
    public float rotationSpeed = 360f; // Degrees per second
    public bool clockwise = true; // Set to false for counterclockwise rotation

    private Coroutine rotationCoroutine;

    private void OnEnable()
    {
        // Start the rotation coroutine when the script is enabled
        rotationCoroutine = StartCoroutine(RotateCircle());
    }

    private void OnDisable()
    {
        // Stop the rotation coroutine when the script is disabled
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
            rotationCoroutine = null;
        }
    }

    private IEnumerator RotateCircle()
    {
        while (true) // This will run until the coroutine is stopped
        {
            // Calculate rotation amount
            float rotationAmount = rotationSpeed * Time.deltaTime;
            
            // Apply rotation based on the clockwise boolean
            if (clockwise)
            {
                transform.Rotate(Vector3.forward, -rotationAmount);
            }
            else
            {
                transform.Rotate(Vector3.forward, rotationAmount);
            }

            yield return null; // Wait for the next frame
        }
    }
}