using UnityEngine;

// TODO: All UI is fast (as in written, not as in performance), stupid and unsafe
public class UIHealthbars : MonoBehaviour
{
    [SerializeField] private GameObject healthbarPrefab = null;
    [SerializeField] private Vector3 offset = Vector3.up * 2f;

    private void LateUpdate()
    {
        var actors = Actor.Actors;

        ResetInstances(actors.Count);

        for (var i = 0; i < actors.Count; i++)
        {
            var actor = actors[i];

            if (actor == null)
                continue;

            var child = transform.GetChild(i);
            var bar = child.GetComponent<UIHealthbar>();
            var position = Camera.main.WorldToScreenPoint(actor.transform.position + offset);

            bar.fillImage.fillAmount = (actor.Life.Health / actor.Life.MaxHealth);
            bar.transform.position = position;
        }
    }

    // TODO: Exists in UIHotbar as well, might just make static helper
    private void ResetInstances(int numActors)
    {
        // Spawn slots
        for (var i = transform.childCount; i < numActors; i++)
        {
            var slot = Instantiate(healthbarPrefab);
            slot.transform.SetParent(transform);
        }

        // Destroy excess slots
        for (var i = transform.childCount - 1; i >= numActors; i--)
            Destroy(transform.GetChild(i).gameObject);
    }
}