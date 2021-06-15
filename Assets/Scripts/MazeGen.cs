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

	public Transform Player;
	public Transform Objective;
	public Transform ObjectParent;
	public GameObject[] PrefabsTypeA;
	public GameObject[] PrefabsTypeB;
	public GameObject[] PrefabsTypeC;
	public GameObject[] PrefabsTypeD;
	public GameObject[] PrefabsTypeE;
	public GameObject[] PrefabsTypeF;

	public bool DrawGizmos;

	private const int maxAttempts = 10000;

	// third dimension is direction: 0 = bottom, 1 = left
	private MazeConnection[,,] connectionMap;
	private Vector2Int startLoc;
	private Vector2Int endLoc;

	public void Generate()
	{
		if (GenerateMap())
		{
			GenerateObjects();
		}
	}

	private bool GenerateMap()
	{
		int attempts = maxAttempts;
		bool success = false;

		while (attempts > 0 && !success)
		{
			attempts--;

			connectionMap = new MazeConnection[GridDims.x + 1, GridDims.y + 1, 2];

			// choose start and end points
			startLoc = new Vector2Int(Random.Range(0, GridDims.x - 1), 0);
			endLoc = new Vector2Int(Random.Range(0, GridDims.x - 1), GridDims.y - 1);

			// close edges
			for (int x = 0; x <= GridDims.x; x++)
			{
				connectionMap[x, 0, 0] = MazeConnection.ForceBlocked;
				connectionMap[x, GridDims.y, 0] = MazeConnection.ForceBlocked;
				connectionMap[x, GridDims.y, 1] = MazeConnection.ForceBlocked;
			}

			for (int y = 0; y <= GridDims.y; y++)
			{
				connectionMap[0, y, 1] = MazeConnection.ForceBlocked;
				connectionMap[GridDims.x, y, 0] = MazeConnection.ForceBlocked;
				connectionMap[GridDims.x, y, 1] = MazeConnection.ForceBlocked;
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
		}

		if (success)
		{
			Debug.Log($"Generation succeeded after {maxAttempts - attempts} attempt(s)");
		}
		else
		{
			Debug.Log("Generation failed!");
		}

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

			int dirRandom = Random.Range(0, 3);
			for (int d = 0; d < 4; d++)
			{
				int dirIdx = (d + dirRandom) % 4;

				nextDirection = (MazeDirection)dirIdx;
				nextOffset = OffsetFromDirection(nextDirection);
				nextLoc = loc + nextOffset;

				if (nextLoc.x >= 0 && nextLoc.x < GridDims.x && nextLoc.y >= 0 && nextLoc.y < GridDims.y)
				{
					if (!visited.Contains(nextLoc))
					{
						foundNext = true;
						break;
					}
				}
			}

			if (foundNext)
			{
				Vector3Int connection = ConnectionCoords(loc, nextDirection);
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
				int cellMask = ConnectionMask(gridCoords);

				if (GetMazeSegment(cellMask, out GameObject segmentPrefab, out float rotationY))
				{
					Vector3 worldPos = GridToWorld(gridCoords);
					Quaternion worldRotation = Quaternion.Euler(0, rotationY, 0);

					GameObject newObject = GameObject.Instantiate(segmentPrefab, ObjectParent);
					newObject.transform.position = worldPos;
					newObject.transform.rotation = worldRotation;
				}
			}
		}

		// position player and objective
		Player.position = GridToWorld(startLoc);
		Objective.position = GridToWorld(endLoc);
	}

	private bool GetMazeSegment(int connectionMask, out GameObject prefab, out float rotationY)
	{
		GameObject[] objectPool = null;

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

		if (objectPool.Length > 0)
		{
			int randomIndex = Random.Range(0, objectPool.Length - 1);
			prefab = objectPool[randomIndex];

			return true;
		}
		else
		{
			prefab = null;
			rotationY = 0;

			return false;
		}
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

					if (connectionMap[x, y, 0] == MazeConnection.Undefined)
					{
						Gizmos.color = Color.grey;
					}
					else if (connectionMap[x, y, 0] == MazeConnection.Open)
					{
						Gizmos.color = Color.blue;
					}
					else if (connectionMap[x, y, 0] == MazeConnection.ForceOpen)
					{
						Gizmos.color = Color.green;
					}
					else if (connectionMap[x, y, 0] == MazeConnection.Blocked)
					{
						Gizmos.color = Color.red;
					}
					else if (connectionMap[x, y, 0] == MazeConnection.ForceBlocked)
					{
						Gizmos.color = Color.black;
					}

					Gizmos.DrawSphere(cellLoc + new Vector3(0, 5, GridScale.y * -0.5f), 1);

					if (connectionMap[x, y, 1] == MazeConnection.Undefined)
					{
						Gizmos.color = Color.grey;
					}
					else if (connectionMap[x, y, 1] == MazeConnection.Open)
					{
						Gizmos.color = Color.blue;
					}
					else if (connectionMap[x, y, 1] == MazeConnection.ForceOpen)
					{
						Gizmos.color = Color.green;
					}
					else if (connectionMap[x, y, 1] == MazeConnection.Blocked)
					{
						Gizmos.color = Color.red;
					}
					else if (connectionMap[x, y, 1] == MazeConnection.ForceBlocked)
					{
						Gizmos.color = Color.black;
					}

					Gizmos.DrawSphere(cellLoc + new Vector3(GridScale.x * -0.5f, 5, 0), 1);
				}
			}
		}
	}
}
