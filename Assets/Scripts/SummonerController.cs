using UnityEngine;

public class SummonerController : MonoBehaviour
{
    // Keyboard setting
    public KeyCode summonKey = KeyCode.Return;

    // Summon Manager
    public ObstacleSummonManager summonManager;

    private void Update()
    {
        if (!Input.GetKeyDown(summonKey))
        {
            return;
        }

        if (summonManager != null)
        {
            summonManager.TrySpawnObstacle();
        }
    }

    // Optional hook for a UI Button OnClick event.
    public void SummonFromButton()
    {
        if (summonManager != null)
        {
            summonManager.TrySpawnObstacle();
        }
    }
}
