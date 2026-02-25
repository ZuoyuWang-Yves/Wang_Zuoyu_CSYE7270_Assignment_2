using System.Collections;
using UnityEngine;

public class HoleGoal : MonoBehaviour
{
    public string playerTag = "Player";
    public string notReadyText = "Collect More Cubes First!";
    public float notReadyMessageDuration = 1.2f;

    private Coroutine clearMessageRoutine;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null)
        {
            return;
        }

        if (player.HasEnoughCollectibles())
        {
            player.WinGame();
        }
        else if (player.winText != null)
        {
            player.winText.text = notReadyText;
            ProceduralChordSfx.PlayAction(GameSfxAction.HoleNotReady);

            if (clearMessageRoutine != null)
            {
                StopCoroutine(clearMessageRoutine);
            }

            clearMessageRoutine = StartCoroutine(ClearNotReadyMessage(player));
        }
    }

    private IEnumerator ClearNotReadyMessage(PlayerController player)
    {
        yield return new WaitForSeconds(notReadyMessageDuration);

        if (player != null && player.winText != null && player.winText.text == notReadyText)
        {
            player.winText.text = "";
        }

        clearMessageRoutine = null;
    }
}
