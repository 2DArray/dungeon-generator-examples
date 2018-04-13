using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathing : MonoBehaviour {

	static List<Vector2Int> activeTiles;
	static List<Vector2Int> nextActiveTiles;
	public static int[,] deadTiles;

	static Vector2Int[] dirs = new Vector2Int[]{ new Vector2Int(1,0),new Vector2Int(-1,0), new Vector2Int(0,1), new Vector2Int(0,-1) };

	public static int GetShortestDistance(TileMap map,Vector2Int start,Vector2Int end) {
		if (deadTiles == null) {
			deadTiles = new int[map.width,map.height];
		} else {
			if (deadTiles.GetLength(0)!=map.width || deadTiles.GetLength(1)!=map.height) {
				deadTiles = new int[map.width,map.height];
			}
		}
		if (activeTiles==null) {
			activeTiles = new List<Vector2Int>();
			nextActiveTiles = new List<Vector2Int>();
		} else {
			activeTiles.Clear();
			nextActiveTiles.Clear();
		}
		for (int x=0;x<map.width;x++) {
			for (int y=0;y<map.height;y++) {
				deadTiles[x,y] = 0;
			}
		}

		activeTiles.Add(start);

		int stepCount = 0;
		int output = 0;

		while (activeTiles.Count>0) {
			stepCount++;
			for (int i=0;i<activeTiles.Count;i++) {
				for (int j=0;j<dirs.Length;j++) {
					Vector2Int testTile = activeTiles[i] + dirs[j];
					TileType type = map.GetTile(testTile);
					if (type != TileType.Wall && type != TileType.OutOfBounds) {
						if (deadTiles[testTile.x,testTile.y] == 0) {
							deadTiles[testTile.x,testTile.y] = 1;
							nextActiveTiles.Add(testTile);

							if (testTile == end) {
								output = stepCount;
							}
						}
					}
				}
			}
			activeTiles.Clear();
			List<Vector2Int> temp = activeTiles;
			activeTiles = nextActiveTiles;
			nextActiveTiles = temp;
		}

		return output;
	}
}
