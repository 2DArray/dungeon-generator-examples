using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Spawn a set of tiny rooms and grow them until they meet.
// Then, add random connections between them

public class RectRoomMapGen : MapGenBase {
	public int minRoomCount = 20;
	public int maxRoomCount = 80;
	public int minExpansionAttempts = 500;
	public int maxExpansionAttempts = 3000;
	public int expansionsPerFrame = 50;
	public int connectionAttempts = 2000;
	public int connectionsPerFrame = 50;

	List<Room> rooms;
	List<Room> connectedRooms;
	Dictionary<int,Room> roomLookup;
	int remainingExpansions;
	int remainingConnections;

	class Room {
		// Each room is an axis-aligned bounding box,
		// so we only need to store the min/max corners of the rectangle.
		public Vector2Int minPos;
		public Vector2Int maxPos;
		public int index;

		public Room(Vector2Int min, Vector2Int max, int myIndex) {
			minPos = min;
			maxPos = max;
			index = myIndex;
		}
		public Room(Vector2Int tile, int myIndex) {
			minPos = tile;
			maxPos = tile;
			index = myIndex;
		}
	}

	Vector2Int[] moveDirs = new Vector2Int[] { new Vector2Int(1,0), new Vector2Int(-1,0), new Vector2Int(0,1), new Vector2Int(0,-1) };

	void ExpandRoom(Room room) {
		// pick a cardinal direction in which to expand
		Vector2Int dir = moveDirs[Random.Range(0,moveDirs.Length)];
		Vector2Int startTest=Vector2Int.zero;
		Vector2Int endTest=Vector2Int.zero;
		Vector2Int newMin = room.minPos;
		Vector2Int newMax = room.maxPos;
		
		// to see if we can expand, we need to check along a nearby line.
		// if there are open tiles along this line, we've been blocked!
		// we check two spaces ahead of our current wall, because
		// we want to maintain at least a single-tile barrier between all rooms
		if (dir.x==1) {
			startTest = new Vector2Int(room.maxPos.x + 2,room.minPos.y-1);
			endTest = new Vector2Int(room.maxPos.x + 2,room.maxPos.y+1);
			newMax.x++;
		} else if (dir.x==-1) {
			startTest = new Vector2Int(room.minPos.x - 2,room.minPos.y-1);
			endTest = new Vector2Int(room.minPos.x - 2,room.maxPos.y+1);
			newMin.x--;
		} else if (dir.y==1) {
			startTest = new Vector2Int(room.minPos.x - 1,room.maxPos.y + 2);
			endTest = new Vector2Int(room.maxPos.x + 1,room.maxPos.y + 2);
			newMax.y++;
		} else if (dir.y==-1) {
			startTest = new Vector2Int(room.minPos.x - 1,room.minPos.y - 2);
			endTest = new Vector2Int(room.maxPos.x + 1,room.minPos.y - 2);
			newMin.y--;
		}

		// step along our test line, check if we're blocked
		Vector2Int testDir = endTest - startTest;
		testDir.Clamp(Vector2Int.one*-1,Vector2Int.one);

		Vector2Int testPos = startTest-testDir;
		bool blocked = false;
		while (testPos != endTest) {
			testPos += testDir;
			if (map.GetTile(testPos)!=TileType.Wall) {
				blocked = true;
				break;
			}
		}

		if (blocked == false) {
			room.minPos = newMin;
			room.maxPos = newMax;

			for (int x=newMin.x; x<=newMax.x; x++) {
				for (int y=newMin.y; y<=newMax.y; y++) {
					map.SetRoom(x,y,room.index);
					map.SetTile(x,y,TileType.Open);
				}
			}
			map.ApplyTex();
		}
	}

	void ConnectRoom() {
		Room room = connectedRooms[Random.Range(0,connectedRooms.Count)];

		Vector2Int testTile = new Vector2Int(Random.Range(room.minPos.x,room.maxPos.x + 1),Random.Range(room.minPos.y,room.maxPos.y + 1));
		Vector2Int dir = moveDirs[Random.Range(0,moveDirs.Length)];

		bool go = true;
		while (go) {
			testTile += dir;
			int testRoomIndex = map.GetRoom(testTile);
			int testRoomIndex2 = map.GetRoom(testTile.x - dir.y,testTile.y + dir.x);
			int testRoomIndex3 = map.GetRoom(testTile.x + dir.y,testTile.y - dir.x);
			if (testRoomIndex == -2) {
				// out of bounds
				return;
			} else if (testRoomIndex==-1 && map.GetTile(testTile)==TileType.Open) {
				// we hit an existing corridor
				return;
			} else if (testRoomIndex != room.index && testRoomIndex != -1) {
				Room foundRoom = roomLookup[testRoomIndex];
				if (connectedRooms.Contains(foundRoom)) {
					return;
				}
				
				connectedRooms.Add(foundRoom);
				break;
			} else {
				if (testRoomIndex2 != room.index && testRoomIndex2 != -1) {
					// too close to an existing room!
					return;
				}
				if (testRoomIndex3 != room.index && testRoomIndex3 !=-1) {
					// too close to an existing room on the other side!
					return;
				}
			}
		}

		
		// if we're here, we haven't hit a return statement...
		// so we've found a room to connect to!
		// walk backward and set the map tiles for our new corridor
		go = true;
		while (go) {
			testTile -= dir;
			int testRoom = map.GetRoom(testTile);
			if (testRoom==room.index) {
				break;
			} else {
				map.SetTile(testTile,TileType.Open);
			}
		}
		map.ApplyTex();
	}

	void DeleteRoom(Room room) {
		for (int x=room.minPos.x;x<=room.maxPos.x;x++) {
			for (int y=room.minPos.y;y<=room.maxPos.y;y++) {
				map.SetRoom(x,y,-1);
				map.SetTile(x,y,TileType.Wall);
			}
		}
	}

	public override void StartGenerator () {
		remainingExpansions = Random.Range(minExpansionAttempts,maxExpansionAttempts);
		remainingConnections = connectionAttempts;
		map = GetComponent<TileMap>();
		map.FillMap(TileType.Wall);

		if (fastForward) {
			expansionsPerFrame = remainingExpansions;
			connectionsPerFrame = 10000;
		}

		// Initially, we'll spawn a bunch of 1x1 rooms at random

		int roomCount = Random.Range(minRoomCount,maxRoomCount);

		rooms = new List<Room>();
		roomLookup = new Dictionary<int,Room>();
		while (rooms.Count<roomCount) {
			Vector2Int testPos = new Vector2Int(Random.Range(1,map.width-1),Random.Range(1,map.height-1));
			bool hasSpace = true;
			for (int j = 0; j < rooms.Count; j++) {
				int dx = Mathf.Abs(rooms[j].minPos.x - testPos.x);
				int dy = Mathf.Abs(rooms[j].minPos.y - testPos.y);
				if (dx<2 && dy<2) {
					// Too close to an existing room!
					hasSpace = false;
					break;
				}
			}
			if (hasSpace) {
				// Found a spot for a room
				Room room = new Room(testPos,rooms.Count);
				map.SetTile(testPos,TileType.Open);
				map.SetRoom(testPos,rooms.Count);
				rooms.Add(room);
				roomLookup.Add(room.index,room);
			}
		}

		connectedRooms = new List<Room>();
		connectedRooms.Add(rooms[Random.Range(0,rooms.Count)]);

		map.ApplyTex();
	}

	public override void TickGenerator() {
		// Expand rooms:
		for (int i = 0; i < expansionsPerFrame; i++) {
			if (remainingExpansions > 0) {
				remainingExpansions--;

				ExpandRoom(rooms[Random.Range(0,rooms.Count)]);
			}
		}

		// Then connect rooms:
		if (remainingExpansions == 0) {
			for (int i = 0; i < connectionsPerFrame; i++) {
				if (remainingConnections > 0) {
					remainingConnections--;
					ConnectRoom();
					if (connectedRooms.Count == rooms.Count) {
						remainingConnections = 0;
					}
				}
			}
		}

		if (remainingConnections==0) {
			Room firstRoom = connectedRooms[0];
			Room lastRoom = connectedRooms[connectedRooms.Count - 1];

			Vector2Int startTile = new Vector2Int(Random.Range(firstRoom.minPos.x,firstRoom.maxPos.x + 1),Random.Range(firstRoom.minPos.y,firstRoom.maxPos.y + 1));
			Vector2Int finishTile = new Vector2Int(Random.Range(lastRoom.minPos.x,lastRoom.maxPos.x + 1),Random.Range(lastRoom.minPos.y,lastRoom.maxPos.y + 1));
			map.SetTile(startTile,TileType.Start);
			map.SetTile(finishTile,TileType.Finish);

			for (int i=0;i<rooms.Count;i++) {
				if (connectedRooms.Contains(rooms[i])==false) {
					DeleteRoom(rooms[i]);
				}
			}
			map.ApplyTex();
			finished = true;
		}
	}
}
