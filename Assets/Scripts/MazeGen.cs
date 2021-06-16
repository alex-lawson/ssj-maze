using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MazeConnection
{
	Undefined,
	Open,
	ForceOpen,
	Blocked,
	ForceBlocked
}

public enum MazeDirection
{
	North, // +Y
	East, // +X
	South, // -Y
	West // -X
}

[System.Serializable]
public struct MazeSegment
{
	public GameObject Prefab;
	public float RotationY;
}

public class MazeGen : MonoBehaviour
{
	public Vector2Int GridDims;
	public Vector2 GridScale;
	public int EdgePadding;
	public int POITargetCount;
	public int POIGridSpacing;

	public Transform PlayerTransform;
	public Transform ObjectParent;
	public GameObject[] PrefabsTypeA;
	public GameObject[] PrefabsTypeB;
	public GameObject[] PrefabsTypeC;
	public GameObject[] PrefabsTypeD;
	public GameObject[] PrefabsTypeE;
	public GameObject[] PrefabsTypeF;

	public bool DrawGizmos;

	private const int maxAttempts = 1000;

	private MazeDirection[] directionList;

	// third dimension is direction: 0 = bottom, 1 = left
	private MazeConnection[,,] connectionMap;
	List<Vector2Int> poiLocations;
	private Vector2Int playerStartLocation;

	public void Generate()
	{
		bool success = false;
		int attempts = maxAttempts;

		directionList = new MazeDirection[] { MazeDirection.North, MazeDirection.East, MazeDirection.South, MazeDirection.West };

		while (attempts > 0 && !success)
		{
			attempts--;

			connectionMap = new MazeConnection[GridDims.x + 1, GridDims.y + 1, 2];
			poiLocations = new List<Vector2Int>();

			success = GeneratePOIs_RandomWithSpacing(POIGridSpacing) && GenerateMap_TreeGrowth();
		}

		if (success)
		{
			Debug.Log($"Generation succeeded after {maxAttempts - attempts} attempt(s)");

			GenerateObjects();
			PlacePlayer();
		}
		else
		{
			Debug.Log("Generation failed!");
		}
	}

	private bool GeneratePOIs_Random()
	{
		int targetCount = Mathf.Min(POITargetCount, GridDims.x * GridDims.y);
		while (poiLocations.Count < targetCount)
		{
			Vector2Int randomLocation = new Vector2Int(Random.Range(0, GridDims.x), Random.Range(0, GridDims.y));
			if (!poiLocations.Contains(randomLocation))
			{
				poiLocations.Add(randomLocation);
			}
		}

		return true;
	}

	private bool GeneratePOIs_RandomWithSpacing(int gridSpacing)
	{
		int attempts = maxAttempts;
		while (poiLocations.Count < POITargetCount && attempts > 0)
		{
			attempts--;

			Vector2Int randomLocation = new Vector2Int(Random.Range(0, GridDims.x), Random.Range(0, GridDims.y));

			bool locValid = true;
			for (int i = 0; i < poiLocations.Count; i++)
			{
				Vector2Int gridDelta = randomLocation - poiLocations[i];
				int gridDist = Mathf.Abs(gridDelta.x) + Mathf.Abs(gridDelta.y);

				if (gridDist <= gridSpacing)
				{
					locValid = false;
					break;
				}
			}

			if (locValid)
			{
				poiLocations.Add(randomLocation);
			}
		}

		bool success = poiLocations.Count == POITargetCount;

		return success;
	}

	private bool GenerateMap_TreeGrowth()
	{
		List<Vector2Int> openSet = new List<Vector2Int>();
		HashSet<Vector2Int> visitedSet = new HashSet<Vector2Int>();

		Vector2Int startPoint = new Vector2Int(Random.Range(0, GridDims.x), Random.Range(0, GridDims.y));
		openSet.Add(startPoint);
		visitedSet.Add(startPoint);

		while (openSet.Count > 0)
		{
			int expandIndex = ChooseTreeGrowthIndex(openSet.Count);

			Vector2Int expandPoint = openSet[expandIndex];

			if (!ExpandRandomFromPoint(expandPoint, openSet, visitedSet, MazeConnection.Open))
			{
				openSet.RemoveAt(expandIndex);
			}
		}

		// select player start location somewhere near the center of the map
		int xMin = Mathf.FloorToInt(GridDims.x * 0.25f);
		int xMax = Mathf.CeilToInt(GridDims.x * 0.75f);
		int yMin = Mathf.FloorToInt(GridDims.y * 0.25f);
		int yMax = Mathf.CeilToInt(GridDims.y * 0.75f);
		playerStartLocation = new Vector2Int(Random.Range(xMin, xMax), Random.Range(yMin, yMax));

		// success is guaranteed!
		return true;
	}

	private int ChooseTreeGrowthIndex(int openSetCount)
	{
		// "recursive backtrack"
		int rbtIndex = openSetCount - 1;

		return rbtIndex;

		//// Prim's-like
		//int primIndex = Random.Range(0, openSetCount);

		//// 50/50 mix
		//if (Random.value < 0.5f)
		//{
		//	return rbtIndex;
		//}
		//else
		//{
		//	return primIndex;
		//}
	}

	private bool ExpandRandomFromPoint(Vector2Int fromPoint, List<Vector2Int> openSet, HashSet<Vector2Int> visitedSet, MazeConnection connectionType)
	{
		bool success = false;

		ArrayUtil.ShuffleArray(ref directionList);

		for (int d = 0; d < 4; d++)
		{
			MazeDirection toDirection = directionList[d];
			Vector2Int offset = OffsetFromDirection(toDirection);
			Vector2Int toPoint = fromPoint + offset;

			if (IsInsideGrid(toPoint) && !visitedSet.Contains(toPoint))
			{
				Vector3Int connCoords = ConnectionCoords(fromPoint, toDirection);

				if (connectionMap[connCoords.x, connCoords.y, connCoords.z] != MazeConnection.ForceBlocked)
				{
					openSet.Add(toPoint);
					visitedSet.Add(toPoint);

					connectionMap[connCoords.x, connCoords.y, connCoords.z] = connectionType;

					success = true;
					break;
				}
			}
		}

		return success;
	}

	private bool GenerateMap_RandomWalk()
	{
		bool success = false;

		// choose start and end points
		Vector2Int startLoc = new Vector2Int(Random.Range(0, GridDims.x - 1), EdgePadding);
		Vector2Int endLoc = new Vector2Int(Random.Range(0, GridDims.x - 1), GridDims.y - 1 - EdgePadding);

		// close edges
		for (int x = 0; x <= GridDims.x; x++)
		{
			for (int i = 0; i <= EdgePadding; i++)
			{
				connectionMap[x, i, 0] = MazeConnection.ForceBlocked;
				connectionMap[x, GridDims.y - i, 0] = MazeConnection.ForceBlocked;
				connectionMap[x, GridDims.y - i, 1] = MazeConnection.ForceBlocked;
			}
		}

		for (int y = 0; y <= GridDims.y; y++)
		{
			for (int i = 0; i <= EdgePadding; i++)
			{
				connectionMap[i, y, 1] = MazeConnection.ForceBlocked;
				connectionMap[GridDims.x - i, y, 0] = MazeConnection.ForceBlocked;
				connectionMap[GridDims.x - i, y, 1] = MazeConnection.ForceBlocked;
			}
		}

		// create solution path - random walk
		HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
		success = DoRandomWalk(startLoc, endLoc, visited, MazeConnection.ForceOpen);

		// add side paths
		if (success)
		{
			Vector2Int[] visitedArray = new Vector2Int[visited.Count];
			visited.CopyTo(visitedArray);

			for (int i = 0; i < visitedArray.Length; i++)
			{
				int startIndex = Random.Range(0, visitedArray.Length);
				DoRandomWalk(visitedArray[startIndex], null, visited, MazeConnection.Open);
			}
		}

		// fill in non-solution paths
		for (int x = 0; x <= GridDims.x; x++)
		{
			for (int y = 0; y <= GridDims.y; y++)
			{
				for (int d = 0; d < 2; d++)
				{
					if (connectionMap[x, y, d] == MazeConnection.Undefined)
					{
						connectionMap[x, y, d] = MazeConnection.Blocked;
					}
				}
			}
		}

		playerStartLocation = startLoc;

		return success;
	}

	private bool DoRandomWalk(Vector2Int startLoc, Vector2Int? endLoc, HashSet<Vector2Int> visited, MazeConnection markConnectionType)
	{
		bool reachedEnd = false;
		bool stuck = false;

		Vector2Int loc = startLoc;

		while (!stuck && !reachedEnd)
		{
			visited.Add(loc);

			// try each cardinal direction (starting with a random direction)
			bool foundNext = false;

			MazeDirection nextDirection = MazeDirection.North;
			Vector2Int nextOffset = Vector2Int.zero;
			Vector2Int nextLoc = loc;
			Vector3Int connection = Vector3Int.zero;

			int dirRandom = Random.Range(0, 3);
			for (int d = 0; d < 4; d++)
			{
				int dirIdx = (d + dirRandom) % 4;

				nextDirection = (MazeDirection)dirIdx;
				nextOffset = OffsetFromDirection(nextDirection);
				nextLoc = loc + nextOffset;

				if (nextLoc.x >= 0 && nextLoc.x < GridDims.x && nextLoc.y >= 0 && nextLoc.y < GridDims.y)
				{
					connection = ConnectionCoords(loc, nextDirection);

					if (connectionMap[connection.x, connection.y, connection.z] != MazeConnection.ForceBlocked && !visited.Contains(nextLoc))
					{
						foundNext = true;
						break;
					}
				}
			}

			if (foundNext)
			{
				connectionMap[connection.x, connection.y, connection.z] = markConnectionType;
				loc = nextLoc;

				visited.Add(loc);

				if (endLoc.HasValue && loc == endLoc.Value)
				{
					reachedEnd = true;
				}
			}
			else
			{
				stuck = true;
			}
		}

		return reachedEnd;
	}

	private void GenerateObjects()
	{
		// clear objects
		for (int i = ObjectParent.childCount - 1; i >= 0; i--)
		{
			DestroyImmediate(ObjectParent.GetChild(i).gameObject);
		}

		// place new objects
		for (int x = 0; x < GridDims.x; x++)
		{
			for (int y = 0; y < GridDims.y; y++)
			{
				Vector2Int gridCoords = new Vector2Int(x, y);

				if (GetMazeSegmentPrefab(gridCoords, out GameObject segmentPrefab, out float rotationY))
				{
					Vector3 worldPos = GridToWorld(gridCoords);
					Quaternion worldRotation = Quaternion.Euler(0, rotationY, 0);

					GameObject newObject = GameObject.Instantiate(segmentPrefab, ObjectParent);
					newObject.transform.position = worldPos;
					newObject.transform.rotation = worldRotation;
				}
			}
		}

		// place edge padding objects
		if (EdgePadding > 0 && PrefabsTypeA.Length > 0)
		{
			for (int x = -EdgePadding; x < GridDims.x + EdgePadding; x++)
			{
				for (int i = 0; i < EdgePadding; i++)
				{
					PlaceEdgePaddingObject(new Vector2Int(x, -EdgePadding));
					PlaceEdgePaddingObject(new Vector2Int(x, GridDims.y - 1 + EdgePadding));
				}
			}

			for (int y = 0; y < GridDims.y; y++)
			{
				for (int i = 0; i < EdgePadding; i++)
				{
					PlaceEdgePaddingObject(new Vector2Int(-EdgePadding, y));
					PlaceEdgePaddingObject(new Vector2Int(GridDims.x - 1 + EdgePadding, y));
				}
			}
		}
	}

	private void PlaceEdgePaddingObject(Vector2Int gridCoords)
	{
		Vector3 worldPos = GridToWorld(gridCoords);
		GameObject newObject = GameObject.Instantiate(PrefabsTypeA[Random.Range(0, PrefabsTypeA.Length)], ObjectParent);
		newObject.transform.position = worldPos;
	}

	private bool GetMazeSegmentPrefab(Vector2Int gridLocation, out GameObject prefab, out float rotationY)
	{
		GameObject[] objectPool = null;

		int connectionMask = ConnectionMask(gridLocation);

		if (connectionMask == 0)
		{
			objectPool = PrefabsTypeA;
			rotationY = 0;
		}
		else if (connectionMask == 1)
		{
			objectPool = PrefabsTypeB;
			rotationY = 0;
		}
		else if (connectionMask == 2)
		{
			objectPool = PrefabsTypeB;
			rotationY = 90;
		}
		else if (connectionMask == 3)
		{
			objectPool = PrefabsTypeC;
			rotationY = 0;
		}
		else if (connectionMask == 4)
		{
			objectPool = PrefabsTypeB;
			rotationY = 180;
		}
		else if (connectionMask == 5)
		{
			objectPool = PrefabsTypeD;
			rotationY = 0;
		}
		else if (connectionMask == 6)
		{
			objectPool = PrefabsTypeC;
			rotationY = 90;
		}
		else if (connectionMask == 7)
		{
			objectPool = PrefabsTypeE;
			rotationY = 0;
		}
		else if (connectionMask == 8)
		{
			objectPool = PrefabsTypeB;
			rotationY = 270;
		}
		else if (connectionMask == 9)
		{
			objectPool = PrefabsTypeC;
			rotationY = 270;
		}
		else if (connectionMask == 10)
		{
			objectPool = PrefabsTypeD;
			rotationY = 270;
		}
		else if (connectionMask == 11)
		{
			objectPool = PrefabsTypeE;
			rotationY = 270;
		}
		else if (connectionMask == 12)
		{
			objectPool = PrefabsTypeC;
			rotationY = 180;
		}
		else if (connectionMask == 13)
		{
			objectPool = PrefabsTypeE;
			rotationY = 180;
		}
		else if (connectionMask == 14)
		{
			objectPool = PrefabsTypeE;
			rotationY = 90;
		}
		else // if (connectionMask == 15)
		{
			objectPool = PrefabsTypeF;
			rotationY = 0;
		}

		if (objectPool.Length > 1)
		{
			if (poiLocations.Contains(gridLocation))
			{
				int randomIndex = Random.Range(1, objectPool.Length);
				prefab = objectPool[randomIndex];
			}
			else
			{
				prefab = objectPool[0];
			}

			return true;
		}
		else if (objectPool.Length == 1)
		{
			prefab = objectPool[0];

			return true;
		}
		else
		{
			prefab = null;
			rotationY = 0;

			return false;
		}
	}

	private void PlacePlayer()
	{
		ArrayUtil.ShuffleArray(ref directionList);

		for (int d = 0; d < 4; d++)
		{
			MazeDirection faceDirection = directionList[d];

			Vector3Int conn = ConnectionCoords(playerStartLocation, faceDirection);
			if (connectionMap[conn.x, conn.y, conn.z] == MazeConnection.Open || connectionMap[conn.x, conn.y, conn.z] == MazeConnection.ForceOpen)
			{
				PlayerTransform.position = GridToWorld(playerStartLocation);
				PlayerTransform.rotation = OrientationFromDirection(faceDirection);

				break;
			}
		}
	}

	private bool IsInsideGrid(Vector2Int gridCoords)
	{
		return IsInsideGrid(gridCoords.x, gridCoords.y);
	}

	private bool IsInsideGrid(int x, int y)
	{
		return x >= 0 && x < GridDims.x && y >= 0 && y < GridDims.y;
	}

	private bool IsInsideConnectionGrid(Vector3Int connectionCoords)
	{
		return IsInsideConnectionGrid(connectionCoords.x, connectionCoords.y, connectionCoords.z);
	}

	private bool IsInsideConnectionGrid(int x, int y, int z)
	{
		return x >= 0 && x <= GridDims.x && y >= 0 && y <= GridDims.y && z >= 0 && z <= 1;
	}

	private Vector3 GridToWorld(Vector2Int gridLocation)
	{
		Vector2 worldPos2D = gridLocation * GridScale;

		return new Vector3(worldPos2D.x, 0, worldPos2D.y);
	}

	private Vector2Int OffsetFromDirection(MazeDirection direction)
	{
		if (direction == MazeDirection.North)
		{
			return new Vector2Int(0, 1);
		}
		else if (direction == MazeDirection.East)
		{
			return new Vector2Int(1, 0);
		}
		else if (direction == MazeDirection.South)
		{
			return new Vector2Int(0, -1);
		}
		else // West
		{
			return new Vector2Int(-1, 0);
		}
	}

	private Quaternion OrientationFromDirection(MazeDirection direction)
	{
		return Quaternion.Euler(0, (float)direction * 90, 0);
	}

	private Vector3Int ConnectionCoords(Vector2Int cellCoords, MazeDirection direction)
	{
		int axis = (int)direction % 2;
		int shift = 1 - (int)direction / 2;

		return new Vector3Int(
			cellCoords.x + (axis == 1 ? shift : 0),
			cellCoords.y + (axis == 0 ? shift : 0),
			axis
		);
	}

	private int ConnectionMask(Vector2Int cellCoords)
	{
		int res = 0;

		if (connectionMap[cellCoords.x, cellCoords.y + 1, 0] == MazeConnection.Open || connectionMap[cellCoords.x, cellCoords.y + 1, 0] == MazeConnection.ForceOpen)
		{
			res += (1 << 0);
		}
		if (connectionMap[cellCoords.x + 1, cellCoords.y, 1] == MazeConnection.Open || connectionMap[cellCoords.x + 1, cellCoords.y, 1] == MazeConnection.ForceOpen)
		{
			res += (1 << 1);
		}
		if (connectionMap[cellCoords.x, cellCoords.y, 0] == MazeConnection.Open || connectionMap[cellCoords.x, cellCoords.y, 0] == MazeConnection.ForceOpen)
		{
			res += (1 << 2);
		}
		if (connectionMap[cellCoords.x, cellCoords.y, 1] == MazeConnection.Open || connectionMap[cellCoords.x, cellCoords.y, 1] == MazeConnection.ForceOpen)
		{
			res += (1 << 3);
		}

		return res;
	}

    private void Start()
    {
		Generate();
    }

    private void Update()
    {

    }

	private void OnDrawGizmos()
	{
		if (DrawGizmos && connectionMap != null)
		{
			for (int x = 0; x < connectionMap.GetLength(0); x++)
			{
				for (int y = 0; y < connectionMap.GetLength(1); y++)
				{
					Vector3 cellLoc = GridToWorld(new Vector2Int(x, y));
					bool drawLine = false;

					if (connectionMap[x, y, 0] == MazeConnection.Undefined)
					{
						Gizmos.color = Color.grey;
						drawLine = false;
					}
					else if (connectionMap[x, y, 0] == MazeConnection.Open)
					{
						Gizmos.color = Color.blue;
						drawLine = true;
					}
					else if (connectionMap[x, y, 0] == MazeConnection.ForceOpen)
					{
						Gizmos.color = Color.green;
						drawLine = true;
					}
					else if (connectionMap[x, y, 0] == MazeConnection.Blocked)
					{
						Gizmos.color = Color.red;
						drawLine = false;
					}
					else if (connectionMap[x, y, 0] == MazeConnection.ForceBlocked)
					{
						Gizmos.color = Color.black;
						drawLine = false;
					}

					if (drawLine)
					{
						Gizmos.DrawCube(cellLoc + new Vector3(0, 5, GridScale.y * -0.5f), new Vector3(1, 1, GridScale.y));
					}

					if (connectionMap[x, y, 1] == MazeConnection.Undefined)
					{
						Gizmos.color = Color.grey;
						drawLine = false;
					}
					else if (connectionMap[x, y, 1] == MazeConnection.Open)
					{
						Gizmos.color = Color.blue;
						drawLine = true;
					}
					else if (connectionMap[x, y, 1] == MazeConnection.ForceOpen)
					{
						Gizmos.color = Color.green;
						drawLine = true;
					}
					else if (connectionMap[x, y, 1] == MazeConnection.Blocked)
					{
						Gizmos.color = Color.red;
						drawLine = false;
					}
					else if (connectionMap[x, y, 1] == MazeConnection.ForceBlocked)
					{
						Gizmos.color = Color.black;
						drawLine = false;
					}

					if (drawLine)
					{
						Gizmos.DrawCube(cellLoc + new Vector3(GridScale.x * -0.5f, 5, 0), new Vector3(GridScale.x, 1, 1));
					}
				}
			}

			Gizmos.color = Color.yellow;
			for (int i = 0; i < poiLocations.Count; i++)
			{
				Vector3 worldLoc = GridToWorld(poiLocations[i]) + Vector3.up * 6;
				Gizmos.DrawSphere(worldLoc, 2);
			}
		}
	}
}
