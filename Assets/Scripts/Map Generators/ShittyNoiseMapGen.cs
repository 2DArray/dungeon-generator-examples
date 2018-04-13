using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Generate wall placements with random noise.
// (just a simple example)

public class ShittyNoiseMapGen : MapGenBase {
	[Range(0f,1f)]
	public float wallChance;

	public override void StartGenerator() {
		// random wall placement
		for (int x=0; x<map.width; x++) {
			for (int y=0; y<map.height; y++) {
				if (Random.value<wallChance) {
					map.SetTile(x,y,TileType.Wall);
				}
			}
		}

		// random start/end placement
		Vector2Int startPos = new Vector2Int(Random.Range(0,map.width),Random.Range(0,map.height));
		Vector2Int finishPos;
		do {
			finishPos = new Vector2Int(Random.Range(0,map.width),Random.Range(0,map.height));
		} while (startPos == finishPos);

		map.SetTile(startPos,TileType.Start);
		map.SetTile(finishPos,TileType.Finish);

		map.ApplyTex();
		finished = true;
	}
}
