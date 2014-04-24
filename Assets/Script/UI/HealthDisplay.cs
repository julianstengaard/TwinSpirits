using UnityEngine;
using System.Collections;

public class HealthDisplay : MonoBehaviour
{
    public int PlayerNumber;
    public GameObject[] Hearts;

    public Material HeartFullMaterial;
    public Material HeartHalfMaterial;
    public Material HeartEmptyMaterial;

    private Hero _player;

	// Use this for initialization
	void Start () {
        //Find player
        FindPlayer();
	}

	void FixedUpdate ()
	{
	    if (_player != null)
	        UpdateHearts();
	    else
	        FindPlayer();
	}

    private void FindPlayer()
    {
        _player = GameObject.Find("UI").GetComponent<SpiritMeterUI>().GetPlayer(PlayerNumber);
    }

    private void UpdateHearts()
    {
        float healthDisplayed = 0f;
        
        bool done = false;
        foreach (var heart in Hearts)
        {
            if (done) {
                heart.renderer.enabled = false;
                continue;
            } 
                
            heart.renderer.enabled = true;
            //Full heart
            if (_player.Health - healthDisplayed > 1f)
            {
                heart.renderer.material = HeartFullMaterial;
            }
            //Half heart
            else if (_player.Health - healthDisplayed > 0f)
            {
                heart.renderer.material = HeartHalfMaterial;
            }
            //Empty heart
            else
            {
                heart.renderer.material = HeartEmptyMaterial;
            }
            healthDisplayed += 2f;

            if (healthDisplayed >= _player.FullHealth)
                done = true;

        }

    }
}
