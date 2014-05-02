using UnityEngine;

public class ShrineSpiritSpawner : MonoBehaviour {
    public Drop Drop;
    public float DropInterval;
    private float _dropIntervalTimer;
    private MazeCell _startCell;
    private MiniMap _miniMap;

    private float _minimumExistTimer = 0f;

    void Start() {
        _dropIntervalTimer = 0f;
        _startCell = transform.root.gameObject.GetComponent<MazeInstance>().represents;
        _miniMap = FindObjectOfType<MiniMap>();
    }

    void FixedUpdate() {
        if (_minimumExistTimer > 5f && _miniMap.GetPlayerCellPosition() != _startCell) {
            Destroy(this);
        }

        if (_dropIntervalTimer > DropInterval) {
            var p = transform.position + Vector3.up;
            var x = Random.Range(p.x - 1, p.x + 1);
            var z = Random.Range(p.z - 1, p.z + 1);
            GameObject.Instantiate(Drop.Item, new Vector3(x, p.y, z), Quaternion.identity);
            _dropIntervalTimer = 0f;
        }
        _dropIntervalTimer += Time.deltaTime;
        _minimumExistTimer += Time.deltaTime;
    }
}
