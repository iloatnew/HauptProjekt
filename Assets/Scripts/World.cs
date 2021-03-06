﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Realtime.Messaging.Internal;

/// <summary>
/// The world MonoBehavior is in charge of creating, updating and destroying chunks based on the player's location.
/// These mechanisms are completed with the help of Coroutines (IEnumerator methods). https://docs.unity3d.com/Manual/Coroutines.html
/// </summary>
public class World : MonoBehaviour
{
	public GameObject player;
	public Material textureAtlas;
	public Material fluidTexture;
	public Material grassAtlas;
	public static int WaterHeight = 55;
	public static int SandHeight = 57;
	public static int StoneHeight = 90;
	public static int columnHeight = 16;
	public static int chunkSize = 8;
	public static int radius = 3;
	public static uint maxCoroutines = 1000;
	public static ConcurrentDictionary<string, Chunk> chunks;
	public static List<string> toRemove = new List<string>();

	public static bool firstbuild = true;

	public static CoroutineQueue queue;

	public Vector3 lastbuildPos;
    public static List<Vector2> riverPoints;
	public static List<Vector2> volcanoPoints = new List<Vector2>();
	
	public static int worldTime;
	static float worldTimeFloat;
	static float oldTimeFloat;
	List<Vector3> directionList = new List<Vector3>();
	List<GameObject> eruptionPointList = new List<GameObject>();
	public static List<Vector3>volcanoBottomWorldPos = new List<Vector3>();
	/// <summary>
	/// Creates a name for the chunk based on its position
	/// </summary>
	/// <param name="v">Position of tje chunk</param>
	/// <returns>Returns a string witht he chunk's name</returns>
	public static string BuildChunkName(Vector3 v)
	{
		return (int)v.x + "_" + 
			         (int)v.y + "_" + 
			         (int)v.z;
	}

    /// <summary>
    /// Creates a name for the column based on its position
    /// </summary>
    /// <param name="v">Position of the column</param>
    /// <returns>Returns a string witht he column's name</returns>
	public static string BuildColumnName(Vector3 v)
	{
		return (int)v.x + "_" + (int)v.z;
	}

    /// <summary>
    /// Get block based on world coordinates
    /// </summary>
    /// <param name="pos">Rough position of the block to be returned</param>
    /// <returns>Returns the block related to the input position</returns>
	public static Block GetWorldBlock(Vector3 pos)
	{
        // Cast float to int to specify the actual chunk and block, which might got hit a by a raycast
        // Chunk
		int cx = (int) (Mathf.Round(pos.x)/(float)chunkSize) * chunkSize;
		int cy = (int) (Mathf.Round(pos.y)/(float)chunkSize) * chunkSize;
		int cz = (int) (Mathf.Round(pos.z)/(float)chunkSize) * chunkSize;

        // Block
		int blx = (int) (Mathf.Round(pos.x) - cx);
		int bly = (int) (Mathf.Round(pos.y) - cy);
		int blz = (int) (Mathf.Round(pos.z) - cz);
		// Create chunk name 
		/*
		 * Bug: sometime blz will be negativ;
		 * Fix: redirect blz to nearby chunk
		 * */
		string cn;
		if (blz < 0) 
		{
			blz = 8 + blz;
			cn = BuildChunkName(new Vector3(cx, cy, (cz-8) ));
		}
		else
			cn = BuildChunkName(new Vector3(cx,cy,cz));
		Chunk c;
        // Find block in chunk
		if(chunks.TryGetValue(cn, out c))
		{
			return c.chunkData[blx,bly,blz];
		}
		else
			return null;
	}

    /// <summary>
    /// Create a set of positions, where shall be a river
    /// </summary>
    /// <param name="worldX">start x</param>
    /// <returns>the list of points</returns>
    public List<Vector2> BlockOnRiver(int worldX, int worldZ)
    {
        float lowestHeight;
        Vector2 lowestNeighbour = new Vector2((int)worldX, (int)worldZ);
        List<Vector2> list = new List<Vector2>() { };
		List<Vector2> RiverSideList = new List<Vector2>() { };
		int i = 0;
        do
        {
			bool change = false;
			i++;
            lowestHeight = Utils.GenerateHeightFloat(worldX, worldZ, 0, 150);
            list.Add(lowestNeighbour);
            worldX = (int)lowestNeighbour.x;
            worldZ = (int)lowestNeighbour.y;
            Vector2[] neighbours = new Vector2[] {
                new Vector2(worldX-1, worldZ),
                new Vector2(worldX, worldZ-1),
                new Vector2(worldX, worldZ+1),
                new Vector2(worldX+1, worldZ)};
            
			// lowestNeighbor: current river position, neighbour: a block nearby
            foreach (Vector2 neighbour in neighbours)
            {
				if(list.Count>2)
					if (neighbour != list[list.Count - 2])
						RiverSideList.Add(neighbour);
				if (Utils.GenerateHeightFloat((int)neighbour.x, (int)neighbour.y, 0f, 150) < lowestHeight && neighbour!=lowestNeighbour)
                {
                    lowestHeight = Utils.GenerateHeightFloat((int)neighbour.x, (int)neighbour.y, 0f, 150);
                    lowestNeighbour = neighbour;
                    change = true;
                }
            }
			if (!change) 
			{
				lowestNeighbour = list[list.Count - 1] + (list[list.Count - 1] - list[list.Count - 2]);
				//Debug.Log("last: " + list[list.Count - 1] + " cur: " + lowestNeighbour);
			}
        } while (lowestHeight>=WaterHeight && i < 400);
		// reach a relativ lowest point, no lower neighbour
		list.AddRange(RiverSideList);
        return list;
    }

	public static bool IsVolcanoChunk(Vector3 pos)
	{
		if (Utils.GenerateHeight(pos.x, pos.z) > World.StoneHeight + 20)
		{
			Vector2 potentialPos = new Vector2(pos.x, pos.z);
			foreach (Vector2 point in volcanoPoints)
			{
				if ((potentialPos - point).SqrMagnitude() < 1000)
					return false;
			}
			volcanoPoints.Add(potentialPos);
			volcanoBottomWorldPos.Add(new Vector3(potentialPos.x, Utils.GenerateHeight(pos.x, pos.z) - 9, potentialPos.y));
			return true;
		}
		return false;
		
	}

	/// <summary>
	/// Instantiates a new chunk at a specified location.
	/// </summary>
	/// <param name="x">y position of the chunk</param>
	/// <param name="y">y position of the chunk</param>
	/// <param name="z">z position of the chunk</param>
	private void BuildChunkAt(int x, int y, int z)
	{
		Vector3 chunkPosition = new Vector3(x*chunkSize, 
											y*chunkSize, 
											z*chunkSize);
					
		string n = BuildChunkName(chunkPosition);
		Chunk c;

		if(!chunks.TryGetValue(n, out c))
		{
			c = new Chunk(chunkPosition, textureAtlas, fluidTexture, grassAtlas);
			c.chunk.transform.parent = this.transform;
			c.fluid.transform.parent = this.transform;
			c.flower.transform.parent = this.transform;
			chunks.TryAdd(c.chunk.name, c);
		}
	}

    /// <summary>
    /// Coroutine to to recursively build chunks of the world depending on some location and a radius.
    /// </summary>
    /// <param name="x">x position</param>
    /// <param name="y">y position</param>
    /// <param name="z">z position</param>
    /// <param name="startrad">Starting radius (is necessary for recursive calls of this function)</param>
    /// <param name="rad">Desired radius</param>
    /// <returns></returns>
	IEnumerator BuildRecursiveWorld(int x, int y, int z, int startrad, int rad)
	{
		int nextrad = rad-1;
		if(rad <= 0 || y < 0 || y > columnHeight) yield break;
		// Build chunk front
		BuildChunkAt(x,y,z+1);
		queue.Run(BuildRecursiveWorld(x,y,z+1,rad,nextrad));
		yield return null;

		// Build chunk back
		BuildChunkAt(x,y,z-1);
		queue.Run(BuildRecursiveWorld(x,y,z-1,rad,nextrad));
		yield return null;
		
		// Build chunk left
		BuildChunkAt(x-1,y,z);
		queue.Run(BuildRecursiveWorld(x-1,y,z,rad,nextrad));
		yield return null;

		// Build chunk right
		BuildChunkAt(x+1,y,z);
		queue.Run(BuildRecursiveWorld(x+1,y,z,rad,nextrad));
		yield return null;
		
		// Build chunk up
		BuildChunkAt(x,y+1,z);
		queue.Run(BuildRecursiveWorld(x,y+1,z,rad,nextrad));
		yield return null;
		
		// Build chunk down
		BuildChunkAt(x,y-1,z);
		queue.Run(BuildRecursiveWorld(x,y-1,z,rad,nextrad));
		yield return null;
	}

    /// <summary>
    /// Coroutine to render chunks that are in the DRAW state. Adds chunks to the toRemove list, which are outside the player's radius.
    /// </summary>
    /// <returns></returns>
	IEnumerator DrawChunks()
	{
		toRemove.Clear();
		foreach(KeyValuePair<string, Chunk> c in chunks)
		{
			if(c.Value.status == Chunk.ChunkStatus.DRAW) 
			{
				c.Value.DrawChunk();
			}
			if(c.Value.chunk && Vector3.Distance(player.transform.position,
								c.Value.chunk.transform.position) > radius*chunkSize)
				toRemove.Add(c.Key);

			yield return null;
		}
	}

    /// <summary>
    /// Coroutine to save and then to unload unused chunks.
    /// </summary>
    /// <returns></returns>
	IEnumerator RemoveOldChunks()
	{
		for(int i = 0; i < toRemove.Count; i++)
		{
			string n = toRemove[i];
			Chunk c;
			if(chunks.TryGetValue(n, out c))
			{
				Destroy(c.chunk);
				c.Save();
				chunks.TryRemove(n, out c);
				yield return null;
			}
		}
	}

    /// <summary>
    /// Builds chunks that are inside the player's radius.
    /// </summary>
	public void BuildNearPlayer()
	{
        // Stop the coroutine of building the world, because it is getting replaced
		StopCoroutine("BuildRecursiveWorld");
		queue.Run(BuildRecursiveWorld((int)(player.transform.position.x/chunkSize),
											(int)(player.transform.position.y/chunkSize),
											(int)(player.transform.position.z/chunkSize), radius, radius));
	}

	/// <summary>
    /// Unity lifecycle start method. Initializes the world and its first chunk and triggers the building of further chunks.
    /// Player is disabled during Start() to avoid him falling through the floor. Chunks are built using coroutines.
    /// </summary>
	void Start ()
    {
        riverPoints = BlockOnRiver(90, -30);

        Vector3 ppos = player.transform.position;
		player.transform.position = new Vector3(ppos.x,
											Utils.GenerateHeight(ppos.x,ppos.z) + 1,
											ppos.z);
		lastbuildPos = player.transform.position;
		player.SetActive(false);

		firstbuild = true;
		chunks = new ConcurrentDictionary<string, Chunk>();
		this.transform.position = Vector3.zero;
		this.transform.rotation = Quaternion.identity;
		

		queue = new CoroutineQueue(maxCoroutines, StartCoroutine);

		// Build starting chunk
		BuildChunkAt((int)(player.transform.position.x/chunkSize),
											(int)(player.transform.position.y/chunkSize),
											(int)(player.transform.position.z/chunkSize));
		// Draw starting chunk
		queue.Run( DrawChunks() );

		// Create further chunks
		queue.Run( BuildRecursiveWorld( (int)(player.transform.position.x / chunkSize),
											(int)(player.transform.position.y / chunkSize),
											(int)(player.transform.position.z / chunkSize), radius, radius ) );
	}



	/// <summary>
	/// Unity lifecycle update method. Actviates the player's GameObject. Updates chunks based on the player's position.
	/// </summary>
	void Update ()
    {
		worldTimeFloat = worldTimeFloat + Time.deltaTime;
		if (worldTimeFloat - oldTimeFloat <= 10)
		{
			volcanoEruptting();//the eruption lasts 10 seconds, this is the method that squirrs magma.
		}
		if (worldTimeFloat - oldTimeFloat >= 15)
		{
			//clearOldMagma(); //not using at first

			oldTimeFloat = worldTimeFloat;
			volcanoEruptionEnd();//only clears lists now
			volcanoErruptionInit();
			//testMagmaInteraction();
		}
		worldTime = (int)worldTimeFloat;

		// Determine whether to build/load more chunks around the player's location
		Vector3 movement = lastbuildPos - player.transform.position;
		
		if(movement.magnitude > chunkSize )
		{
			lastbuildPos = player.transform.position;
			BuildNearPlayer();
		}

        // Activate the player's GameObject
		if(!player.activeSelf)
		{
			player.SetActive(true);	
			firstbuild = false;
		}

        // Draw new chunks and removed deprecated chunks
		queue.Run(DrawChunks());
		queue.Run(RemoveOldChunks());
	}

	//bind the direction to the erruption point
	void volcanoErruptionInit()
	{

		while (directionList.Count < 5)
		{
			var seedx = UnityEngine.Random.Range(-0.1f, 0.1f);
			var seedz = UnityEngine.Random.Range(-0.1f, 0.1f);
			directionList.Add(new Vector3(seedx, 25, seedz));
		}

		while (eruptionPointList.Count < 5)
		{
			eruptionPointList.Add(new GameObject("erruptionPoint"));
		}
		foreach (Vector3 pos in volcanoBottomWorldPos)
			foreach (GameObject erruptionPoint in eruptionPointList)
			{
				erruptionPoint.transform.position = pos + new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f));
				erruptionPoint.AddComponent<EruptionPoint>();
				erruptionPoint.GetComponent<EruptionPoint>().direction = directionList[0];
				directionList.RemoveAt(0);
				//Debug.Log(erruptionPoint.GetComponent<EruptionPoint>().direction);
			}
	}
	private void volcanoEruptting()
	{
		//MeshCollider mc = magma.AddComponent<MeshCollider>();
		//mc.sharedMesh = magma.GetComponent<MeshFilter>().mesh;
		//GameObject eruptionPoint = GameObject.Find("EruptionPoint");
		//Vector3 chunkPos = WorldToLocalChunk(volcanoBottomWorldPos);
		//Vector3 blockPos = WorldToLocalBlock(volcanoBottomWorldPos);
		foreach (GameObject erruptionPoint in eruptionPointList)
		{
			GameObject magma = new GameObject("magma");
			//Debug.Log("magma created");
			magma.transform.position = erruptionPoint.transform.position;
			magma.AddComponent<MeshFilter>();
			magma.AddComponent<MeshRenderer>();
			magma.AddComponent<mCube>();

			MeshRenderer mr = magma.GetComponent<MeshRenderer>();
			mr.material = Resources.Load("minecraft") as Material;
			magma.AddComponent<BoxCollider>();
			magma.AddComponent<Rigidbody>();
			magma.GetComponent<Rigidbody>().isKinematic = false;
			magma.GetComponent<Rigidbody>().AddForce(erruptionPoint.GetComponent<EruptionPoint>().direction, ForceMode.Impulse);
		}

	}

	private void volcanoEruptionEnd()
	{
		foreach (GameObject eruptionPoint in eruptionPointList)
		{
			Destroy(eruptionPoint);
		}
		eruptionPointList.Clear();
		directionList.Clear();
	}
}
