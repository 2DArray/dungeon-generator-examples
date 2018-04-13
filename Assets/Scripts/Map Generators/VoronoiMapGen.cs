using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Generate rooms based on a Voronoi diagram

public class VoronoiMapGen : MapGenBase {
	public int minRoomCount = 20;
	public int maxRoomCount = 100;
	public int pushApartIterations=100;
	public int minIgnoreRoom = 3;
	public int maxIgnoreRoom = 8;
	public int connectionAttempts=1000;
	public int pushesPerFrame = 1;
	public int samplesPerFrame = 50;
	public int connectionsPerFrame = 1;

	int ignoreOneRoomPer;
	int remainingPushes;
	int remainingConnections;
	int pixelIndex;

	bool foundRooms;
	bool foundEdges;

	List<Room> rooms;
	List<Room> connectedRooms;

	Vector2Int[] dirs = new Vector2Int[] { new Vector2Int(1,0),new Vector2Int(-1,0),new Vector2Int(0,1),new Vector2Int(0,-1) };

	class Room {
		public Vector2 position;
		public float radius;
		public List<Vector2Int> tiles;
		public int index;

		public Room(Vector2 pos,float rad, int myIndex) {
			position = pos;
			radius = rad;
			index = myIndex;
			tiles = new List<Vector2Int>();
		}
	}

	void DeleteRoom(Room room) {
		for (int i=0;i<room.tiles.Count;i++) {
			map.SetTile(room.tiles[i],TileType.Wall);
		}
	}

	public override void StartGenerator() {
		if (fastForward) {
			pushesPerFrame = pushApartIterations;
			samplesPerFrame = map.width * map.height;
			connectionsPerFrame = connectionAttempts;
		}

		rooms = new List<Room>();
		connectedRooms = new List<Room>();

		int roomCount = Random.Range(minRoomCount,maxRoomCount);

		for (int i=0;i<roomCount;i++) {
			rooms.Add(new Room(new Vector2(Random.value,Random.value),Random.Range(.01f,.1f),rooms.Count));
		}

		ignoreOneRoomPer = Random.Range(minIgnoreRoom,maxIgnoreRoom);

		foundRooms = false;
		foundEdges = false;
		remainingPushes = pushApartIterations;
		remainingConnections = connectionAttempts;
		pixelIndex = 0;
	}

	public override void TickGenerator() {
		if (remainingPushes > 0) {
			// Push our rooms apart, like a fluid particle system,
			// or like the ball-pit at the play pen

			for (int k = 0; k < pushesPerFrame && remainingPushes > 0; k++) {
				remainingPushes--;
				for (int i = 0; i < rooms.Count - 1; i++) {
					for (int j = i + 1; j < rooms.Count; j++) {
						Room room1 = rooms[i];
						Room room2 = rooms[j];
						Vector2 delta = room1.position - room2.position;
						float sqrDist = delta.x * delta.x + delta.y * delta.y;
						float combinedRad = room1.radius + room2.radius;
						if (sqrDist < combinedRad * combinedRad) {
							// Rooms are too close together - push them apart
							float dist = Mathf.Sqrt(sqrDist);
							float extra = (combinedRad - dist) * .5f;

							room1.position += delta.normalized * extra;
							room2.position -= delta.normalized * extra;
						}
					}
				}
			}

			// Keep our room centers inside the map
			for (int i = 0; i < rooms.Count; i++) {
				rooms[i].position.x = Mathf.Clamp01(rooms[i].position.x);
				rooms[i].position.y = Mathf.Clamp01(rooms[i].position.y);
			}
		}

		if (remainingPushes==0 && foundRooms == false) {
			for (int j = 0; j < samplesPerFrame; j++) {
				if (pixelIndex < map.width * map.height) {
					// Rasterize the voronoi diagram
					// (Find the closest room to each pixel)

					// map cell coordinates
					int x = pixelIndex % map.width;
					int y = pixelIndex / map.width;

					// 0-1 UV-space position of our pixel's center point
					float u = (float)x / map.width + .5f / map.width;
					float v = (float)y / map.height + .5f / map.height;

					// find closest room to this pixel
					float minDist = 100f;
					int closestRoom = -1;
					for (int i = 0; i < rooms.Count; i++) {
						Vector2 delta = new Vector2(rooms[i].position.x - u,rooms[i].position.y - v);
						float sqrDist = delta.x * delta.x + delta.y * delta.y;
						if (sqrDist < minDist) {
							minDist = sqrDist;
							closestRoom = i;
						}
					}

					map.SetRoom(x,y,closestRoom);

					if (samplesPerFrame < map.width * map.height) {
						float t = closestRoom / (float)rooms.Count;
						Debug.DrawRay(transform.TransformPoint(new Vector3(u - .5f,v - .5f,0f)),Vector3.up * .02f*transform.lossyScale.x,Color.Lerp(Color.black,Color.white,t),2f);
					}
					pixelIndex++;

					if (pixelIndex == map.width * map.height) {
						foundRooms = true;
						pixelIndex = 0;
					}
				}
			}
		}
		if (foundRooms && foundEdges==false) {
			// The Voronoi diagram has finished!
			// Let's put in walls along the cell borders
			// and erase certain cells entirely
			for (int k = 0; k < samplesPerFrame; k++) {
				if (pixelIndex < map.width * map.height) {
					// map cell coordinates
					int x = pixelIndex % map.width;
					int y = pixelIndex / map.width;

					// We'll check nearby tiles to see if any have mismatched room IDs

					if (map.GetRoom(x,y) % ignoreOneRoomPer == 0) {
						map.SetTile(x,y,TileType.Wall);
					} else {

						bool edgeTile = false;
						int roomID = -1;
						for (int i = x-1; i <= x + 1; i++) {
							for (int j = y-1; j <= y + 1; j++) {
								int testRoom = map.GetRoom(i,j);
								if (testRoom != roomID) {
									if (roomID == -1) {
										roomID = testRoom;
									} else {
										edgeTile = true;
										break;
									}
								}
							}
						}

						if (edgeTile) {
							map.SetTile(x,y,TileType.Wall);
						} else {
							List<Vector2Int> roomTiles = rooms[map.GetRoom(x,y)].tiles;
							Vector2Int newTile = new Vector2Int(x,y);
							roomTiles.Add(newTile);
						}
					}

					pixelIndex++;
					if (pixelIndex == map.width * map.height) {
						int firstRoomIndex = 0;
						do {
							firstRoomIndex = Random.Range(0,rooms.Count);
						} while (firstRoomIndex % ignoreOneRoomPer == 0 && rooms[firstRoomIndex].tiles.Count==0);
						connectedRooms.Add(rooms[firstRoomIndex]);
						foundEdges = true;
					}
				}
			}
			map.ApplyTex();
		}

		if (foundEdges && finished==false) {
			for (int j = 0; j < connectionsPerFrame; j++) {
				if (remainingConnections > 0) {
					remainingConnections--;

					Room room = connectedRooms[Random.Range(0,connectedRooms.Count)];
					if (room.tiles.Count > 0) {
						Vector2Int tile = room.tiles[Random.Range(0,room.tiles.Count)];
						Vector2Int dir = dirs[Random.Range(0,dirs.Length)];
						bool go = true;
						bool success = false;
						int reachDist = 0;
						while (go) {
							tile += dir;
							if (map.GetRoom(tile) != room.index) {
								reachDist++;
								if (reachDist > 3) {
									break;
								}
								if (map.GetTile(tile) == TileType.Open) {
									Room newRoom = rooms[map.GetRoom(tile)];
									if (connectedRooms.Contains(newRoom) == false) {
										connectedRooms.Add(newRoom);
										success = true;
										break;
									}
								}
							}
						}

						if (success) {
							// we found a new neighbor!
							// step backwards and add our connecting hallway

							go = true;
							while (go) {
								tile -= dir;
								if (map.GetRoom(tile) == room.index && map.GetTile(tile) == TileType.Open) {
									break;
								} else {
									map.SetTile(tile,TileType.Open);
								}
							}

							if (connectedRooms.Count == rooms.Count) {
								connectionAttempts = 0;
							}
						}
					}
				} else {
					for (int i = 0; i < rooms.Count; i++) {
						if (connectedRooms.Contains(rooms[i]) == false) {
							DeleteRoom(rooms[i]);
						}
					}
					finished = true;
					break;
				}
			}

			if (finished) {
				Room startRoom = connectedRooms[0];
				int lastRoomID = connectedRooms.Count - 1;
				while (connectedRooms[lastRoomID].tiles.Count==0) {
					lastRoomID--;
				}
				Room finishRoom = connectedRooms[lastRoomID];

				Vector2Int startTile = startRoom.tiles[Random.Range(0,startRoom.tiles.Count)];
				Vector2Int finishTile = finishRoom.tiles[Random.Range(0,finishRoom.tiles.Count)];

				map.SetTile(startTile,TileType.Start);
				map.SetTile(finishTile,TileType.Finish);
			}

			map.ApplyTex();
		}
	}

	new private void OnDrawGizmos() {
		base.OnDrawGizmos();
		if (rooms!=null && foundRooms==false) {
			Gizmos.color = Color.blue;
			for (int i=0;i<rooms.Count;i++) {
				Gizmos.DrawWireSphere(transform.TransformPoint(rooms[i].position - Vector2.one * .5f),rooms[i].radius*transform.lossyScale.x);
			}
		}
	}
}
