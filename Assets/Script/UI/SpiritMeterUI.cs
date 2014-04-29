using UnityEngine;
using System.Collections;
using System.Reflection;
using Holoville.HOTween;

public class SpiritMeterUI : MonoBehaviour {
	private Hero Player1;
	private Hero Player2;

	public Material SpiritSpeedBoostIcon;
	public Material SpiritBungieIcon;
	public Material SpiritImmortalIcon;
	public Material SpiritHPRegenIcon;
	public Material SpiritLightningIcon;
	public Material SpiritFireIcon;
	public Material SpiritPingPongIcon;

	public GameObject SpiritMeterDivider;
	public Vector3 P1SpiritMeterDividerMin;
	public Vector3 P1SpiritMeterDividerMax;
	public Vector3 P2SpiritMeterDividerMin;
	public Vector3 P2SpiritMeterDividerMax;

    public Material SyncOnIcon;
    public Material SyncOffIcon;

	private GameObject p1Icon;
	private System.Type p1SpiritPrevious = null;
	private GameObject p2Icon;
    private System.Type p2SpiritPrevious = null;

    private GameObject syncIcon;
	private GameObject p1Meter;
	private GameObject p1SyncCostLine;
	private GameObject p2Meter;
	private GameObject p2SyncCostLine;

    private Vector3 p1MeterZero = new Vector3(-0.39f, -0f, 0.05f);
    private Vector3 p2MeterZero = new Vector3(0.39f, -0f, 0.05f);
	private Vector3 p1Zero 		= new Vector3(0.03f, -0.0f, 0.05f);
    private Vector3 p2Zero      = new Vector3(-0.03f, -0.0f, 0.05f);

	private bool playersFound = false;
	private bool animatingSpiritPowerP1 = false;
	private bool animatingSpiritPowerP2 = false;

	private GameObject[] _p1Dividers;
	private GameObject[] _p2Dividers;

	// Use this for initialization
	void Start () 
	{
		p1Icon = GameObject.Find(this.gameObject.name+"/LeftSpiritIcon");
		p2Icon = GameObject.Find(this.gameObject.name+"/RightSpiritIcon");
        syncIcon = GameObject.Find(this.gameObject.name + "/SyncIcon");

		p1Meter = GameObject.Find(this.gameObject.name+"/LeftSpiritMeter/LeftSpiritMeterAmount");
		p2Meter = GameObject.Find(this.gameObject.name+"/RightSpiritMeter/RightSpiritMeterAmount");

		p1SyncCostLine = GameObject.Find(this.gameObject.name+"/LeftSpiritMeter/LeftSpiritSyncCostLine");
		p2SyncCostLine = GameObject.Find(this.gameObject.name+"/RightSpiritMeter/RightSpiritSyncCostLine");
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
		}
	}

	bool FindPlayers()
	{
		var ps = GameObject.FindObjectsOfType<Hero>();
		if(ps.Length == 2) {
		    foreach (var player in ps) {
		        if (player.PlayerSlot == Hero.Player.One)
					Player1 = player;
                if (player.PlayerSlot == Hero.Player.Two)
					Player2 = player;
		    }
			if (UpdateSpiritPowerIcons())
			    return true;
            else 
                print("dafuq");
		}
		return false;
	}

	void SetSpiritMeterDividers(int playerNumber) {
		if (playerNumber == 1) {
			if (_p1Dividers != null) {
				foreach(GameObject go in _p1Dividers) {
					Destroy(go);
				}
			}
			int p1divisions = (int) (100/Player1.currentSpiritPower.GetCostActivate());
			_p1Dividers = new GameObject[p1divisions-1];
			Vector3 distancePerDivider = (P1SpiritMeterDividerMax - P1SpiritMeterDividerMin)/p1divisions;
			for (int i = 1; i < p1divisions; i++) {
				_p1Dividers[i-1] = (GameObject) GameObject.Instantiate(SpiritMeterDivider, Vector3.zero, Quaternion.identity);
				_p1Dividers[i-1].transform.parent = p1Meter.transform.parent;
				_p1Dividers[i-1].transform.localPosition = P1SpiritMeterDividerMin + distancePerDivider * i;
			}
		}
		else if (playerNumber == 2) {
			if (_p2Dividers != null) {
				foreach(GameObject go in _p2Dividers) {
					Destroy(go);
				}
			}
			int p2divisions = (int) (100/Player2.currentSpiritPower.GetCostActivate());
			_p2Dividers = new GameObject[p2divisions-1];
			Vector3 distancePerDivider = (P2SpiritMeterDividerMax - P2SpiritMeterDividerMin)/p2divisions;
			for (int i = 1; i < p2divisions; i++) {
				_p2Dividers[i-1] = (GameObject) GameObject.Instantiate(SpiritMeterDivider, P2SpiritMeterDividerMin + distancePerDivider * i, Quaternion.identity);
				_p2Dividers[i-1].transform.parent = p2Meter.transform.parent;
				_p2Dividers[i-1].transform.localPosition = P2SpiritMeterDividerMin + distancePerDivider * i;
			}
		}
	}

	void UpdateSpiritMeter(int playerNumber) {
		if (playerNumber == 1) {
			float spiritAmount = Player1.currentSpiritAmount/100f;
			p1Meter.transform.localScale 	= new Vector3(spiritAmount*0.85f, 0.5f, 1);
            p1Meter.transform.localPosition = Vector3.Lerp(p1MeterZero, p1Zero, spiritAmount);
			ColorizeSpiritMeter(p1Meter, Player1.currentSpiritPower, spiritAmount*100f);

			if (Player1.currentSpiritPower.GetCostActivateSync() <= Player1.currentSpiritAmount)
				p1SyncCostLine.renderer.enabled = true;
			else
				p1SyncCostLine.renderer.enabled = false;

			//Set the cost
			float cost = Player1.currentSpiritPower.GetCostActivateSync()/100f;
			p1SyncCostLine.transform.localScale	= new Vector3(cost*0.85f, 0.15f, 1);
			Vector3 targetPosition = p1SyncCostLine.transform.position;
			targetPosition.x = p1Meter.transform.position.x + p1Meter.transform.localScale.x/2f - p1SyncCostLine.transform.localScale.x/2f;
			p1SyncCostLine.transform.position = targetPosition;
		}
		else if (playerNumber == 2) {
			float spiritAmount = Player2.currentSpiritAmount/100f;
			p2Meter.transform.localScale = new Vector3(spiritAmount*0.85f, 0.5f, 1);
            p2Meter.transform.localPosition = Vector3.Lerp(p2MeterZero, p2Zero, spiritAmount); 
			ColorizeSpiritMeter(p2Meter, Player2.currentSpiritPower, spiritAmount*100f);

			if (Player2.currentSpiritPower.GetCostActivateSync() <= Player2.currentSpiritAmount)
				p2SyncCostLine.renderer.enabled = true;
			else
				p2SyncCostLine.renderer.enabled = false;

			//Set the cost
			float cost = Player2.currentSpiritPower.GetCostActivateSync()/100f;
			p2SyncCostLine.transform.localScale	= new Vector3(cost*0.85f, 0.15f, 1);
			Vector3 targetPosition = p2SyncCostLine.transform.position;
			targetPosition.x = p2Meter.transform.position.x - p2Meter.transform.localScale.x/2f + p2SyncCostLine.transform.localScale.x/2f;
			p2SyncCostLine.transform.position = targetPosition;
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
		if (Player1.currentSpiritAmount >= Player1.currentSpiritPower.GetCostActivateSync() && Player2.currentSpiritAmount >= Player2.currentSpiritPower.GetCostActivateSync())
        {
            syncIcon.renderer.material = SyncOnIcon;
        }
        else
        {
            syncIcon.renderer.material = SyncOffIcon;
        }
    }

    public bool UpdateSpiritPowerIcons() {
        bool b;
		b = UpdateSpiritPowerIcon(Player1, p1Icon);
		b = UpdateSpiritPowerIcon(Player2, p2Icon) && b;
        return b;
    }

	private bool UpdateSpiritPowerIcon(Hero player, GameObject icon)
	{
		int playerNumber = player.PlayerSlot == Hero.Player.One ? 1 : 2;

		//Stop if player has no power
	    if (player.currentSpiritPower == null) {
            return false;
	    }
		//Stop if already animating this player
		if (playerNumber == 1 ? animatingSpiritPowerP1 : animatingSpiritPowerP2) {
            return false;
		}

		//Which player should we check against
		var previous = playerNumber == 1 ? p1SpiritPrevious : p2SpiritPrevious;

		//Stop if no change to spirit power since last time (and not first time)
        if (previous != null && player.currentSpiritPower.GetType() == previous) {
            return false;
		}

		//Update previous power to current one
		if (playerNumber == 1) {
			p1SpiritPrevious = player.currentSpiritPower.GetType();
		} else if (playerNumber == 2) {
			p2SpiritPrevious = player.currentSpiritPower.GetType();
		}

		//Clone the old icon, so we can throw it away
		var oldIcon = (GameObject) Instantiate(icon, icon.transform.position, icon.transform.rotation);
		oldIcon.transform.parent = icon.transform.parent;

		//Update with the new icon
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
		} else if (player.currentSpiritPower.GetType() == typeof(SpiritFire)) {
			icon.renderer.material = SpiritFireIcon;
		} else if (player.currentSpiritPower.GetType() == typeof(SpiritPingPong)) {
			icon.renderer.material = SpiritPingPongIcon;
		}

		//Animate it
		StartCoroutine(AnimateIconTransition(oldIcon, icon, playerNumber));
	    return true;
	}

	private IEnumerator AnimateIconTransition(GameObject oldIcon, GameObject newIcon, int playerNumber) {
		float time = 0.8f;

		//Update the dividers
		SetSpiritMeterDividers(playerNumber);

		if (playerNumber == 1)
			animatingSpiritPowerP1 = true;
		else if (playerNumber == 2)
			animatingSpiritPowerP2 = true;

		TweenParms oldIconTween = new TweenParms().Prop(
			"localPosition", oldIcon.transform.localPosition + Vector3.back * 2f).Ease(
			EaseType.EaseInOutExpo).Delay(0f);
		HOTween.To(oldIcon.transform, time, oldIconTween);

		var destination = newIcon.transform.localPosition;
		newIcon.transform.localPosition += Vector3.back * 2f;

		TweenParms newIconTween = new TweenParms().Prop(
			"localPosition", destination).Ease(
			EaseType.EaseInOutExpo).Delay(0f);
		HOTween.To(newIcon.transform, time, newIconTween);

		yield return new WaitForSeconds(time);
		Destroy(oldIcon);

		if (playerNumber == 1)
			animatingSpiritPowerP1 = false;
		else if (playerNumber == 2)
			animatingSpiritPowerP2 = false;
	}

    public Hero GetPlayer(int i)
    {
        if (i == 1)
            return Player1;
        else
            return Player2;
    }
}
