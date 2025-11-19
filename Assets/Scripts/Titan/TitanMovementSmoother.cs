using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TitanMovementSmoother : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 smoothedVelocity;

    // This value is taken from the reference material, providing significant smoothing.
    [SerializeField]
    private float smoothFactor = 0.0435f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        smoothedVelocity = rb.velocity;
    }

    void FixedUpdate()
    {
        // Smooth the velocity using Lerp
        smoothedVelocity = Vector3.Lerp(smoothedVelocity, rb.velocity, smoothFactor);

        // Apply the smoothed velocity back to the rigidbody
        rb.velocity = smoothedVelocity;
    }
}
