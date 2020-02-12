using UnityEngine;

public class UIPlayers : MonoBehaviour
{
    [SerializeField] private UIPlayer playerIconPrefab = null;
    [SerializeField] private Color bgColor = new Color(0.1f, 0.1f, 0.1f);
    [SerializeField] private Color bgReadyColor = Color.green;

    private void Update()
    {
        var manager = PlayerManager.Instance;

        if (manager == null)
            return;

        var players = manager.Players;

        if (players == null)
            return;

        // Add new, remove excess
        ResetInstances(players.Length);

        for (var i = 0; i < players.Length; i++)
        {
            var player = players[i];
            var go = transform.GetChild(i).gameObject;
            var slot = go.GetComponent<UIPlayer>();

            if (player == null || slot == null)
            {
                go.SetActive(false);
                continue;
            }
            else if (!go.activeSelf)
            {
                go.SetActive(true);
            }

            slot.Background.color = !player.IsReady ? bgColor : bgReadyColor;
            slot.Icon.color = player.Color;
            slot.Score.text = player.Score.ToString();
        }
    }

    // TODO: Exists in UIHotbar as well, might just make static helper
    private void ResetInstances(int numPlayers)
    {
        // Spawn slots
        for (var i = transform.childCount; i < numPlayers; i++)
        {
            var slot = Instantiate(playerIconPrefab);
            slot.transform.SetParent(transform);
        }

        // Destroy excess slots
        for (var i = transform.childCount - 1; i >= numPlayers; i--)
            Destroy(transform.GetChild(i).gameObject);
    }
}