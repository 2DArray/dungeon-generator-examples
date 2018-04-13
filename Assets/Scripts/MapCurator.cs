using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCurator : MonoBehaviour {
	public GameObject generatorTemplate;
	public int generatorCount;
	public bool fastForward;
	[Space(10)]
	public bool fitnessCheck;
	[Range(0f,1f)]
	public float fitnessPercentile;
	[Space(10)]
	public bool fillInactiveTiles;

	List<MapGenBase> generators;
	List<float> fitnessValues;
	bool measuredFitnesses = false;
	MapGenBase bestMap;

	int SortByFitness(MapGenBase a, MapGenBase b) {
		if (a.map.fitness>b.map.fitness) {
			return -1;
		} else if (a.map.fitness<b.map.fitness) {
			return 1;
		} else {
			return 0;
		}
	}

	void Start () {
		int widthAndHeight = Mathf.CeilToInt(Mathf.Sqrt(generatorCount));
		generators = new List<MapGenBase>();
		for (int i=0;i<generatorCount;i++) {
			GameObject mapObject = Instantiate(generatorTemplate);
			mapObject.transform.position = Vector3.right * (i%widthAndHeight)+Vector3.up*(i/widthAndHeight);
			mapObject.transform.localScale = .98f*Vector3.one / widthAndHeight;
			mapObject.transform.position *= 1f / widthAndHeight;
			mapObject.transform.position -= (Vector3)Vector2.one*(.5f-.5f/widthAndHeight);
			mapObject.SetActive(true);
			MapGenBase mapGen = mapObject.GetComponent<MapGenBase>();
			mapGen.fastForward = fastForward;
			generators.Add(mapGen);
		}
	}
	
	void Update () {
		if (Input.GetKeyDown(KeyCode.Space)) {
			for (int i=0;i<generators.Count;i++) {
				generators[i].Reset();
			}
			measuredFitnesses = false;
		}

		bool allFinished = true;
		for (int i=0;i<generators.Count;i++) {
			generators[i].fastForward = fastForward;
			if (generators[i].finished==false) {
				allFinished = false;
			}
		}
		if (allFinished && measuredFitnesses==false) {
			measuredFitnesses = true;

			if (fitnessCheck) {
				bestMap = null;
				for (int i = 0; i < generators.Count; i++) {
					TileMap map = generators[i].map;
					
					float fitness = 0f;
					if (map.hasStartTile && map.hasFinishTile) {
						fitness = Pathing.GetShortestDistance(map,map.startTile,map.finishTile);

						if (fillInactiveTiles) {
							for (int x = 0; x < map.width; x++) {
								for (int y = 0; y < map.height; y++) {
									if (Pathing.deadTiles[x,y] == 0) {
										map.SetTile(x,y,TileType.Wall);
									}
								}
							}
							map.ApplyTex();
						}
					}
					map.fitness = fitness;
				}

				generators.Sort(SortByFitness);
			}
		}
		if (measuredFitnesses && fitnessCheck) {
			bestMap = generators[(int)((generators.Count - 1) * (1f - fitnessPercentile))];
		}
	}

	const int circleSegmentCount=40;
	private void OnDrawGizmos() {
		if (measuredFitnesses && bestMap!=null && fitnessCheck) {
			Gizmos.color = Color.green;
			Transform mapTrans = bestMap.transform;
			for (int i=0; i<circleSegmentCount;i++) {
				float angle1 = 2f*Mathf.PI * i / circleSegmentCount;
				float angle2 = 2f*Mathf.PI * (i + 1f) / circleSegmentCount;

				Vector3 point1 = mapTrans.TransformPoint(new Vector2(Mathf.Cos(angle1)*.6f,Mathf.Sin(angle1)*.6f));
				Vector3 point2 = mapTrans.TransformPoint(new Vector2(Mathf.Cos(angle2)*.6f,Mathf.Sin(angle2)*.6f));

				Gizmos.DrawLine(point1,point2);
			}
		}
	}
}
