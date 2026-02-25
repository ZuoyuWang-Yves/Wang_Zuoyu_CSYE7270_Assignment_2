using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public int requiredCollectibles = 7;

    private Rigidbody rb;
    private int count;
    private bool hasWon;

    public Text countText;
    public Text winText;

    public Text replayText;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        count = 0;
        hasWon = false;
        setCountText();
        winText.text = "";


        if (replayText != null)
        {
            replayText.gameObject.SetActive(false);
        }

    }

    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");


        Vector3 movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);

        rb.AddForce(movement * speed);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Collectibles"))
        {
            other.gameObject.SetActive(false);  
            count ++;
            setCountText();
            ProceduralChordSfx.PlayAction(GameSfxAction.CollectiblePickup);
        }
    }


    void setCountText()
    {
        countText.text = "Cube Count: " + count.ToString() + "/" + requiredCollectibles.ToString();
    }

    public bool HasEnoughCollectibles()
    {
        return count >= requiredCollectibles;
    }

    public void WinGame()
    {
        if (hasWon)
        {
            return;
        }

        if (replayText != null)
        {
            replayText.gameObject.SetActive(true);
        }


        hasWon = true;
        winText.text = "You Win!";
        ProceduralChordSfx.ReplayRecordedSong();
    }

}
