using UnityEngine;
using System.Collections;

public class SpiritMeterUI : MonoBehaviour {
	private Hero Player1;
	private Hero Player2;

	public Material SpiritSpeedBoostIcon;
	public Material SpiritBungieIcon;
	public Material SpiritImmortalIcon;
	public Material SpiritHPRegenIcon;
	public Material SpiritLightningIcon;

    public Material SyncOnIcon;
    public Material SyncOffIcon;

	private GameObject p1Icon;
	private GameObject p2Icon;

    private GameObject syncIcon;
	private GameObject p1Meter;
	private GameObject p2Meter;

	private Vector3 p1MeterZero = new Vector3(-0.37f, 0f, 0.001f);
	private Vector3 p2MeterZero = new Vector3(0.37f, 0f, 0.001f);
	private Vector3 p1Zero 		= new Vector3(0.05f, 0f, 0.001f);
    private Vector3 p2Zero      = new Vector3(-0.05f, 0f, 0.001f);

	private bool playersFound = false;

	// Use this for initialization
	void Start () 
	{
		p1Icon = GameObject.Find(this.gameObject.name+"/LeftSpiritIcon");
		p2Icon = GameObject.Find(this.gameObject.name+"/RightSpiritIcon");
        syncIcon = GameObject.Find(this.gameObject.name + "/SyncIcon");

		p1Meter = GameObject.Find(this.gameObject.name+"/LeftSpiritMeter/LeftSpiritMeterAmount");
		p2Meter = GameObject.Find(this.gameObject.name+"/RightSpiritMeter/RightSpiritMeterAmount");
	}

	// Update is called once per frame
	void Update () 
	{
		if (!playersFound) {
			playersFound = FindPlayers();
		}
		else {
			UpdateSpiritMeter(1);
			UpdateSpiritMeter(2);
		    UpdateSyncIcon();
            UpdateSpiritPowerIcons();
		}
	}

	bool FindPlayers()
	{
		var ps = GameObject.FindObjectsOfType<Hero>();
		if(ps.Length > 0) {
		    foreach (var player in ps)
		    {
		       if (player.PlayerSlot == Hero.Player.One)
					Player1 = player;
               if (player.PlayerSlot == Hero.Player.Two)
					Player2 = player;
		    }
			return true;
		}
		else {
			return false;
		}

	}
	void UpdateSpiritMeter(int playerNumber) {
		if (playerNumber == 1) {
			float spiritAmount = Player1.currentSpiritAmount/100f;
			p1Meter.transform.localScale 	= new Vector3(spiritAmount*0.82f, 0.6f, 1);
            p1Meter.transform.localPosition = Vector3.Lerp(p1MeterZero, p1Zero, spiritAmount);
			ColorizeSpiritMeter(p1Meter, Player1.currentSpiritPower, spiritAmount*100f);
		}
		else if (playerNumber == 2) {
			float spiritAmount = Player2.currentSpiritAmount/100f;
			p2Meter.transform.localScale = new Vector3(spiritAmount*0.82f, 0.6f, 1);
            p2Meter.transform.localPosition = Vector3.Lerp(p2MeterZero, p2Zero, spiritAmount); 
			ColorizeSpiritMeter(p2Meter, Player2.currentSpiritPower, spiritAmount*100f);
		}
	}

	void ColorizeSpiritMeter(GameObject spiritMeter, SpiritPower spiritPower, float amount) {
		if (amount < spiritPower.GetCostActivate())
			spiritMeter.renderer.material.SetColor("_Color", Color.red);
		else if (amount >= spiritPower.GetCostActivateSync())
			spiritMeter.renderer.material.SetColor("_Color", Color.green);
		else
			spiritMeter.renderer.material.SetColor("_Color", Color.yellow);
	}

    public void UpdateSyncIcon()
    {
        if (Player1.currentSpiritAmount >= 100f && Player2.currentSpiritAmount >= 100f)
        {
            syncIcon.renderer.material = SyncOnIcon;
        }
        else
        {
            syncIcon.renderer.material = SyncOffIcon;
        }
    }

	public void UpdateSpiritPowerIcons() {
		UpdateSpiritPowerIcon(Player1, p1Icon);
		UpdateSpiritPowerIcon(Player2, p2Icon);
	}

	private void UpdateSpiritPowerIcon(Hero player, GameObject icon)
	{
	    if (player.currentSpiritPower == null) {
            return;
	    }

	    if (player.currentSpiritPower.GetType() == typeof(SpiritSpeedBoost)) {
			icon.renderer.material = SpiritSpeedBoostIcon;
		} else if (player.currentSpiritPower.GetType() == typeof(SpiritBungie)) {
			icon.renderer.material = SpiritBungieIcon;
		} else if (player.currentSpiritPower.GetType() == typeof(SpiritImmortal)) {
			icon.renderer.material = SpiritImmortalIcon;
		} else if (player.currentSpiritPower.GetType() == typeof(SpiritRegenHP)) {
			icon.renderer.material = SpiritHPRegenIcon;
		} else if (player.currentSpiritPower.GetType() == typeof(SpiritLightning)) {
			icon.renderer.material = SpiritLightningIcon;
		}
	}

    public Hero GetPlayer(int i)
    {
        if (i == 1)
            return Player1;
        else
            return Player2;
    }
}
