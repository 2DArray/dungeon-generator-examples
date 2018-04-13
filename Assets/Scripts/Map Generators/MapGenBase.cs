using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// All map generator examples inherit this class

public abstract class MapGenBase : MonoBehaviour {
	public abstract void StartGenerator();
	public virtual void TickGenerator() { }

	public bool finished;
	public TileMap map;
	public bool fastForward;

	public void Reset() {
		finished = false;
		map.Init();
		StartGenerator();
		TickGenerator();
	}

	void Start() {
		map = GetComponent<TileMap>();
		StartGenerator();
	}

	void Update() {
		if (finished == false) {
			TickGenerator();
		}
	}

	protected void OnDrawGizmos() {
		if (map != null) {
			if (map.hasStartTile) {
				Gizmos.color = Color.blue;
				Gizmos.DrawWireSphere(transform.TransformPoint(new Vector2((map.startTile.x+.5f)/map.width,(map.startTile.y+.5f)/map.height) - Vector2.one * .5f),.02f*transform.lossyScale.x);
			}
			if (map.hasFinishTile) {
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(transform.TransformPoint(new Vector2((map.finishTile.x+.5f) / map.width,(map.finishTile.y+.5f) / map.height) - Vector2.one * .5f),.02f*transform.lossyScale.x);
			}
		}
	}
}
