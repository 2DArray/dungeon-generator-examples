using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMap : MonoBehaviour {

	public int width;
	public int height;
	[Space(10)]
	public TileType[] tileTypes;
	public Color32[] colors;

	TileType[,] map;
	int[,] roomMap;

	[Header("For display only:")]
	public float fitness;
	[System.NonSerialized]
	public Vector2Int startTile;
	[System.NonSerialized]
	public Vector2Int finishTile;
	[System.NonSerialized]
	public bool hasStartTile;
	[System.NonSerialized]
	public bool hasFinishTile;
	new Renderer renderer;
	Texture2D mapTex;
	Color32[] tileTypeToColor;

	bool initialized = false;


	public bool SetTile(int x, int y, TileType type) {
		if (initialized==false) {
			Init();
		}
		if (x<0 || x>=width || y<0 || y>=height) {
			return false;
		}
		map[x,y] = type;
		mapTex.SetPixel(x,y,tileTypeToColor[(int)type]);
		if (type==TileType.Start) {
			startTile = new Vector2Int(x,y);
			hasStartTile = true;
		}
		if (type==TileType.Finish) {
			finishTile = new Vector2Int(x,y);
			hasFinishTile = true;
		}
		return true;
	}
	public bool SetTile(Vector2Int pos,TileType type) {
		return SetTile(pos.x,pos.y,type);
	}
	public bool SetRoom(int x, int y, int roomIndex) {
		if (initialized==false) {
			Init();
		}
		if (x < 0 || x >= width || y < 0 || y >= height) {
			return false;
		}
		roomMap[x,y] = roomIndex;
		return true;
	}
	public bool SetRoom(Vector2Int pos,int roomIndex) {
		return SetRoom(pos.x,pos.y,roomIndex);
	}

	public TileType GetTile(int x, int y) {
		if (x < 0 || x >= width || y < 0 || y >= height) {
			return TileType.OutOfBounds;
		}

		return map[x,y];
	}
	public TileType GetTile(Vector2Int pos) {
		return GetTile(pos.x,pos.y);
	}

	public int GetRoom(int x,int y) {
		if (x < 0 || x >= width || y < 0 || y >= height) {
			return -2;
		}

		return roomMap[x,y];
	}
	public int GetRoom(Vector2Int pos) {
		return GetRoom(pos.x,pos.y);
	}

	public void FillMap(TileType type) {
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				SetTile(x,y,type);
			}
		}
	}

	public void ApplyTex() {
		mapTex.Apply();
	}

	public void Init() {
		initialized = true;

		startTile = Vector2Int.zero;
		finishTile = Vector2Int.zero;
		hasStartTile = false;
		hasFinishTile = false;

		renderer = GetComponent<Renderer>();
		tileTypeToColor = new Color32[System.Enum.GetValues(typeof(TileType)).Length];
		for (int i=0;i<tileTypes.Length;i++) {
			tileTypeToColor[(int)tileTypes[i]]=colors[i];
		}

		map = new TileType[width,height];
		roomMap = new int[width,height];
		for (int x=0;x<width;x++) {
			for (int y=0;y<height;y++) {
				roomMap[x,y] = -1;
			}
		}
		mapTex = new Texture2D(width,height,TextureFormat.ARGB32,false);
		FillMap(TileType.Open);
		mapTex.filterMode = FilterMode.Point;
		renderer = GetComponent<Renderer>();
		renderer.material.mainTexture = mapTex;
	}
}
