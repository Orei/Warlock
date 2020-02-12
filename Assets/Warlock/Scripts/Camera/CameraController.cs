using UnityEngine;

[AddComponentMenu("Warlock/Camera Controller")]
public class CameraController : MonoBehaviour
{
    [SerializeField] private float distance = 10f;
    [SerializeField] private float angle = 45f;

    private void LateUpdate()
    {
        var actor = Actor.Local;

        if (actor == null)
            return;

        LookAt(actor.transform.position);
    }

    private void LookAt(Vector3 position)
    {
        // Rotate by angle and then offset by distance
        transform.rotation = Quaternion.Euler(angle, 0f, 0f);
        transform.position = position - (transform.rotation * Vector3.forward * distance);
    }
}