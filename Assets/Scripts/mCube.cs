using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(MeshFilter))]
//[RequireComponent(typeof(MeshRenderer))]
public class mCube : MonoBehaviour
{
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> UVs = new List<Vector2>();

	bool isDestroyed = false;
    bool needToUpdate = false;
    public Vector3 LateUpdatePos;
    public string LateUpdateBlockName;
	public Vector3 direction;
    public Mesh CubeMesh;
	// Start is called before the first frame update
	void Start()
    {
        Destroy(this.gameObject, 5);
        GenerateCube();
    }
    void GenerateCube()
    {
        CubeMesh = new Mesh();

        CreateCubeSide("front");
        CreateCubeSide("back");
        CreateCubeSide("top");
        CreateCubeSide("bottom");
        CreateCubeSide("left");
        CreateCubeSide("right");

        GeneratePhysicalCube();
    }

    void GeneratePhysicalCube()
    {
		//MeshFilter mf = GetComponent<MeshFilter>();

        CubeMesh.vertices = vertices.ToArray(); 
        CubeMesh.triangles = triangles.ToArray();
        CubeMesh.uv = UVs.ToArray();

        CubeMesh.RecalculateBounds();
        CubeMesh.RecalculateNormals();

        this.GetComponent<MeshFilter>().mesh = CubeMesh;
	}

    void CreateCubeSide(string side)
    {
        triangles.Add(0 + vertices.Count);
        triangles.Add(1 + vertices.Count);
        triangles.Add(2 + vertices.Count);

        triangles.Add(0 + vertices.Count);
        triangles.Add(2 + vertices.Count);
        triangles.Add(3 + vertices.Count);

        float textureOffset = 1f / 16f;

        Vector2 texturePos = new Vector2(15,0);//count the position of the texture in atlas.
        //the order is important. x and y refers to u and v coordinate in texutreAtlas
        UVs.Add(new Vector2(textureOffset * texturePos.x + textureOffset, textureOffset * texturePos.y));
        UVs.Add(new Vector2(textureOffset * texturePos.x + textureOffset, textureOffset * texturePos.y + textureOffset));
        UVs.Add(new Vector2(textureOffset * texturePos.x, textureOffset * texturePos.y + textureOffset));
        UVs.Add(new Vector2(textureOffset * texturePos.x, textureOffset * texturePos.y));
    
        //4 points of the back side
        Vector3 V0 = new Vector3(-0.5f, -0.5f, -0.5f);
        Vector3 V1 = new Vector3(0.5f, -0.5f, -0.5f);
        Vector3 V2 = new Vector3(0.5f, 0.5f, -0.5f);
        Vector3 V3 = new Vector3(-0.5f, 0.5f, -0.5f);
        //r points of the front side
        Vector3 V4 = new Vector3(-0.5f, -0.5f, 0.5f);
        Vector3 V5 = new Vector3(0.5f, -0.5f, 0.5f);
        Vector3 V6 = new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 V7 = new Vector3(-0.5f, 0.5f, 0.5f);



        switch (side)
        {
            case "front":
                vertices.Add(V5);
                vertices.Add(V6);
                vertices.Add(V7);
                vertices.Add(V4);
                break;
            case "back":
                vertices.Add(V0);
                vertices.Add(V3);
                vertices.Add(V2);
                vertices.Add(V1);
                break;
            case "top":
                vertices.Add(V3);
                vertices.Add(V7);
                vertices.Add(V6);
                vertices.Add(V2);
                break;
            case "bottom":
                vertices.Add(V4);
                vertices.Add(V0);
                vertices.Add(V1);
                vertices.Add(V5);
                break;
            case "left":
                vertices.Add(V4);
                vertices.Add(V7);
                vertices.Add(V3);
                vertices.Add(V0);
                break;
            case "right":
                vertices.Add(V1);
                vertices.Add(V2);
                vertices.Add(V6);
                vertices.Add(V5);
                break;
        }
    }

	private void OnCollisionEnter(Collision collision)
	{
		if (isDestroyed == false)
		{
			bool effected = false;
			Vector3 collisionPos = this.gameObject.transform.position;
			collisionPos = new Vector3(Mathf.RoundToInt(collisionPos.x), Mathf.RoundToInt(collisionPos.y), Mathf.RoundToInt(collisionPos.z));
			try
			{
				Block block = World.GetWorldBlock(collisionPos);
				if (collision.gameObject.name != "magma" && collision.gameObject.name != "erruptionPoint" && block.blockType!=Block.BlockType.AIR)
				{
					Chunk foundChunk = block.owner; 
					if (foundChunk != null)
					{
						//Debug.Log("foundChunk");
						//Debug.Log("name:" + foundChunk.chunkMap[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].name);
						switch (block.blockType)
						{
							case Block.BlockType.AIR:
								Debug.Log("Air");
								collisionPos = collisionPos + new Vector3(0, -1, 0);
								block = World.GetWorldBlock(collisionPos);
								foundChunk = block.owner;
								if (block.blockType == Block.BlockType.GRASS)
								{
									block.blockType = Block.BlockType.DIAMOND;
									ChangeNeighbor(block);
									effected = true;
								}
								break;
							case Block.BlockType.STONE:
								Debug.Log("stoone");
								break;
							case Block.BlockType.GRASS:
								Debug.Log("grass");
								block.blockType = Block.BlockType.DIAMOND;
								ChangeNeighbor(block);
								effected = true;
								break;
						}
					}
				}
				if (effected)
				{
					isDestroyed = true;
					Destroy(this.gameObject);
				}
			}
			catch (System.Exception e) { }
		}
	}

	void ChangeNeighbor(Block core) 
	{
		Vector3 pos = core.position;
		Block[] neighbors = new Block[]
		{
			World.GetWorldBlock(pos + new Vector3(1, 0, 0)),
			World.GetWorldBlock(pos + new Vector3(0, 0, 1)),
			World.GetWorldBlock(pos + new Vector3(1, 0, 1)),
			World.GetWorldBlock(pos + new Vector3(-1, 0, 0)),
			World.GetWorldBlock(pos + new Vector3(0, 0, -1)),
			World.GetWorldBlock(pos + new Vector3(-1, 0, -1)),
			World.GetWorldBlock(pos + new Vector3(1, 0, -1)),
			World.GetWorldBlock(pos + new Vector3(-1, 0, 1)),
		};
		foreach (Block b in neighbors) 
		{
			b.blockType = Block.BlockType.STONE;
		}
	}
}
