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

	public GameObject BungieParticle;
	public GameObject ImmortalParticle;
	public GameObject LightningParticle;
	public GameObject PingPongParticle;

    public GameObject SpiritMeterDivider;

    public Material SyncOnIcon;
    public Material SyncOffIcon;

	public GameObject P1ItemsHolder;
	public GameObject P2ItemsHolder;
	private int p1ItemCount = 0;
	private int p2ItemCount = 0;

	private GameObject p1Icon;
	private System.Type p1SpiritPrevious = null;
	private GameObject p2Icon;
    private System.Type p2SpiritPrevious = null;

    private Vector3 _p1SpiritMeterDividerMin;
    private Vector3 _p1SpiritMeterDividerMax;
    private Vector3 _p2SpiritMeterDividerMin;
    private Vector3 _p2SpiritMeterDividerMax;

    private GameObject syncIcon;
    private GameObject p1Meter;
    public SyncCostLineScaler p1SyncCostLine;
    private GameObject p2Meter;
    public SyncCostLineScaler p2SyncCostLine;

    private Vector3 p1MeterZero = new Vector3(-0.39f, -0f, 0.05f);
    private Vector3 p2MeterZero = new Vector3(0.39f, -0f, 0.05f);
	private Vector3 p1MeterMid 		= new Vector3(0.03f, -0.0f, 0.05f);
    private Vector3 p2MeterMid      = new Vector3(-0.03f, -0.0f, 0.05f);

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

        p1MeterMid = p1Meter.transform.localPosition;
        p2MeterMid = p2Meter.transform.localPosition;
        p1MeterZero = p1Meter.transform.localPosition + Vector3.left * p1Meter.transform.localScale.x / 2f;
        p2MeterZero = p2Meter.transform.localPosition + Vector3.right * p2Meter.transform.localScale.x / 2f;

        _p1SpiritMeterDividerMin = p1MeterZero;
	    _p1SpiritMeterDividerMax = p1Meter.transform.localPosition + Vector3.right * p1Meter.transform.localScale.x / 2f;
        _p2SpiritMeterDividerMin = p2MeterZero;
        _p2SpiritMeterDividerMax = p2Meter.transform.localPosition + Vector3.left * p2Meter.transform.localScale.x / 2f;

		p1SyncCostLine = GameObject.Find(this.gameObject.name+"/LeftSpiritMeter/LeftSpiritSyncCostLine").GetComponent<SyncCostLineScaler>();
        p2SyncCostLine = GameObject.Find(this.gameObject.name + "/RightSpiritMeter/RightSpiritSyncCostLine").GetComponent<SyncCostLineScaler>();
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
			Vector3 distancePerDivider = (_p1SpiritMeterDividerMax - _p1SpiritMeterDividerMin)/p1divisions;
			for (int i = 1; i < p1divisions; i++) {
				_p1Dividers[i-1] = (GameObject) GameObject.Instantiate(SpiritMeterDivider, Vector3.zero, Quaternion.identity);
				_p1Dividers[i-1].transform.parent = p1Meter.transform.parent;
				_p1Dividers[i-1].transform.localPosition = _p1SpiritMeterDividerMin + distancePerDivider * i;
				_p1Dividers[i-1].transform.localPosition = _p1Dividers[i-1].transform.localPosition.SetZ(-0.01f);
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
			Vector3 distancePerDivider = (_p2SpiritMeterDividerMax - _p2SpiritMeterDividerMin)/p2divisions;
			for (int i = 1; i < p2divisions; i++) {
				_p2Dividers[i-1] = (GameObject) GameObject.Instantiate(SpiritMeterDivider, _p2SpiritMeterDividerMin + distancePerDivider * i, Quaternion.identity);
				_p2Dividers[i-1].transform.parent = p2Meter.transform.parent;
				_p2Dividers[i-1].transform.localPosition = _p2SpiritMeterDividerMin + distancePerDivider * i;
				_p2Dividers[i-1].transform.localPosition = _p2Dividers[i-1].transform.localPosition.SetZ(-0.01f);
			}
		}
	}

	void UpdateSpiritMeter(int playerNumber) {
		if (playerNumber == 1) {
			float spiritAmount = Player1.currentSpiritAmount/100f;
			p1Meter.transform.localScale 	= new Vector3(spiritAmount*0.58f, 0.1f, 1);
            p1Meter.transform.localPosition = Vector3.Lerp(p1MeterZero, p1MeterMid, spiritAmount);
			ColorizeSpiritMeter(p1Meter, Player1.currentSpiritPower, spiritAmount*100f);

			if (Player1.currentSpiritPower.GetCostActivateSync() <= Player1.currentSpiritAmount)
				p1SyncCostLine.SetVisible(true);
			else
                p1SyncCostLine.SetVisible(false);

			//Set the cost
			float cost = Player1.currentSpiritPower.GetCostActivateSync()/100f;
            p1SyncCostLine.LeftEnd = new Vector3(-cost * 0.58f/p1SyncCostLine.transform.localScale.x/2f, 0f, 0f);
            p1SyncCostLine.RightEnd = new Vector3(cost * 0.58f/p1SyncCostLine.transform.localScale.x/2f, 0f, 0f);
            Vector3 targetPosition = p1SyncCostLine.transform.localPosition;
            targetPosition.x = p1Meter.transform.localPosition.x + p1Meter.transform.localScale.x / 2f + p1SyncCostLine.LeftEnd.x * p1SyncCostLine.transform.localScale.x;
            p1SyncCostLine.transform.localPosition = targetPosition;
		}
		else if (playerNumber == 2) {
			float spiritAmount = Player2.currentSpiritAmount/100f;
            p2Meter.transform.localScale = new Vector3(spiritAmount * 0.58f, 0.1f, 1);
            p2Meter.transform.localPosition = Vector3.Lerp(p2MeterZero, p2MeterMid, spiritAmount); 
			ColorizeSpiritMeter(p2Meter, Player2.currentSpiritPower, spiritAmount*100f);

			if (Player2.currentSpiritPower.GetCostActivateSync() <= Player2.currentSpiritAmount)
                p2SyncCostLine.SetVisible(true);
			else
                p2SyncCostLine.SetVisible(false);

			//Set the cost
            float cost = Player2.currentSpiritPower.GetCostActivateSync() / 100f;
            p2SyncCostLine.LeftEnd = new Vector3(-cost * 0.58f / p2SyncCostLine.transform.localScale.x / 2f, 0f, 0f);
            p2SyncCostLine.RightEnd = new Vector3(cost * 0.58f / p2SyncCostLine.transform.localScale.x / 2f, 0f, 0f);
            Vector3 targetPosition = p2SyncCostLine.transform.localPosition;
            targetPosition.x = p2Meter.transform.localPosition.x - p2Meter.transform.localScale.x / 2f - p2SyncCostLine.LeftEnd.x * p2SyncCostLine.transform.localScale.x;
            p2SyncCostLine.transform.localPosition = targetPosition;
		}
	}
	
	void ColorizeSpiritMeter(GameObject spiritMeter, SpiritPower spiritPower, float amount) {
		if (amount < spiritPower.GetCostActivate())
			spiritMeter.renderer.material.SetColor("_Color", Color.red);
		else if (amount >= spiritPower.GetCostActivateSync())
			spiritMeter.renderer.material.SetColor("_Color",  new Color(1f, 1f, 0f));
		else
			spiritMeter.renderer.material.SetColor("_Color", new Color(1f, 0.3f, 0f));
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
		b = UpdateSpiritPowerIcon(Player1);
		b = UpdateSpiritPowerIcon(Player2) && b;
        return b;
    }

	private bool UpdateSpiritPowerIcon(Hero player)
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

		var icon = playerNumber == 1 ? p1Icon : p2Icon;

		//Clone the old icon, so we can throw it away
		var oldIcon = (GameObject) Instantiate(icon, icon.transform.position, icon.transform.rotation);
		oldIcon.transform.parent = icon.transform.parent;

		if (player.currentSpiritPower.GetType() == typeof(SpiritBungie)) {
			icon = (GameObject) GameObject.Instantiate(BungieParticle, icon.transform.position, icon.transform.rotation);
		} else if (player.currentSpiritPower.GetType() == typeof(SpiritImmortal)) {
			icon = (GameObject) GameObject.Instantiate(ImmortalParticle, icon.transform.position, icon.transform.rotation);
		} else if (player.currentSpiritPower.GetType() == typeof(SpiritLightning)) {
			icon = (GameObject) GameObject.Instantiate(LightningParticle, icon.transform.position, icon.transform.rotation);
		} else if (player.currentSpiritPower.GetType() == typeof(SpiritPingPong)) {
			icon = (GameObject) GameObject.Instantiate(PingPongParticle, icon.transform.position, icon.transform.rotation);
		}

		if (playerNumber == 1) {
			GameObject.Destroy(p1Icon);
			p1Icon = icon;
			p1Icon.transform.parent = oldIcon.transform.parent;
			StartCoroutine(AnimateIconTransition(oldIcon, p1Icon, playerNumber));
		} else if (playerNumber == 2) {
			GameObject.Destroy(p2Icon);
			p2Icon = icon;
			p2Icon.transform.parent = oldIcon.transform.parent;
			StartCoroutine(AnimateIconTransition(oldIcon, p2Icon, playerNumber));
		}
	    return true;
	}

	private IEnumerator AnimateIconTransition(GameObject oldIcon, GameObject newIcon, int playerNumber) {
		float time = 0.8f;

		if (playerNumber == 1)
			animatingSpiritPowerP1 = true;
		else if (playerNumber == 2)
			animatingSpiritPowerP2 = true;

		Vector3 tweenDirection = oldIcon.transform.localPosition + (playerNumber == 1 ? new Vector3(-2f, -2f, 0f) : new Vector3(2f, -2f, 0f));

		TweenParms oldIconTween = new TweenParms().Prop(
			"localPosition", tweenDirection).Ease(
			EaseType.EaseInOutExpo).Delay(0f);
		HOTween.To(oldIcon.transform, time, oldIconTween);

		var destination = newIcon.transform.localPosition;
		newIcon.transform.position = newIcon.transform.position + tweenDirection;

		TweenParms newIconTween = new TweenParms().Prop(
			"localPosition", destination).Ease(
			EaseType.EaseInOutExpo).Delay(0f);
		HOTween.To(newIcon.transform, time, newIconTween);

		yield return new WaitForSeconds(time);

		//Update the dividers
		SetSpiritMeterDividers(playerNumber);

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

	public void AddItemToUI(GameObject item, int playerNumber) {
		int itemsPerColumn = 5;
		if (playerNumber == 1) {
			Vector3 rowOffset = (p1ItemCount % itemsPerColumn) * Vector3.up * 1.5f;
			Vector3 columnOffset = (p1ItemCount / itemsPerColumn) * Vector3.right * 1.0f;
			item.transform.position = P1ItemsHolder.transform.position + rowOffset + columnOffset;
			item.transform.parent = P1ItemsHolder.transform;
			p1ItemCount++;
		} else if (playerNumber == 2) {
			Vector3 rowOffset = (p2ItemCount % itemsPerColumn) * Vector3.up * 1.5f;
			Vector3 columnOffset = (p2ItemCount / itemsPerColumn) * Vector3.left * 1.0f;
			item.transform.position = P2ItemsHolder.transform.position + rowOffset + columnOffset;
			item.transform.parent = P2ItemsHolder.transform;
			p2ItemCount++;
		}
	}
}
