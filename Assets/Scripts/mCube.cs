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
        Destroy(this.gameObject, 5f);
        GenerateCube();
    }
    //private void LateUpdate()
    //{
    //    if (true||needToUpdate)
    //    {
    //        Debug.Log("lateupdate");
    //        World_Interaction.updateAfterChangeTerrain(LateUpdatePos, LateUpdateBlockName);
    //    }
    //}
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
        //UVs.Add(new Vector2(0.8125f, 0.0625f));
        //UVs.Add(new Vector2(0.875f, 0.0625f));
        //UVs.Add(new Vector2(0.875f, 0.125f));
        //UVs.Add(new Vector2(0.8125f, 0.125f));
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
			////Debug.Log("oncollisionEnter" + this.gameObject.transform.position);
			//if (collision.gameObject.name != "magma") 
			//{
			//	//Debug.Log("Destroy");
			//	isDestroyed = true;
			//	Destroy(this.gameObject);
			//	Vector3 collisionPos = this.gameObject.transform.position;
			//	collisionPos = new Vector3(Mathf.RoundToInt(collisionPos.x), Mathf.RoundToInt(collisionPos.y), Mathf.RoundToInt(collisionPos.z));

			//	Vector3 chunkPos = World.WorldToLocalChunk(collisionPos);
			//	Vector3 blockPos = World.WorldToLocalBlock(collisionPos);

			//	Chunk foundChunk;
			//	World.chunkList.TryGetValue(chunkPos, out foundChunk);
			//	Debug.Log("collisionPos: " + collisionPos);
			//	Debug.Log("chunkPos: " + chunkPos);
			//	Debug.Log("blockPos: " + blockPos);
			//	if (foundChunk != null)
			//	{
			//		//Debug.Log("foundChunk");
			//		Debug.Log("name:" + foundChunk.chunkMap[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].name);
			//		switch (foundChunk.chunkMap[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].name)
			//		{
			//			case "Air":
			//				//Debug.Log("Air");
			//				collisionPos = collisionPos + new Vector3(0, -1, 0);
			//				chunkPos = World.WorldToLocalChunk(collisionPos);
			//				blockPos = World.WorldToLocalBlock(collisionPos);
			//				foundChunk = World.GetChunkAtPos(collisionPos);
			//				if (foundChunk.chunkMap[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].name == "Grass")
			//				{
			//					//Debug.Log("in case air and grass");
			//                             //needToUpdate = true;
			//                             //LateUpdateBlockName = "Stone";
			//                             //LateUpdatePos = collisionPos;
			//					World_Interaction.changeTerrain(collisionPos, "Stone");

			//                             World_Interaction.changeTerrain(collisionPos + new Vector3(1, 0, 0), "Stone");
			//                             World_Interaction.changeTerrain(collisionPos + new Vector3(0, 0, 1), "Stone");
			//                             World_Interaction.changeTerrain(collisionPos + new Vector3(1, 0, 1), "Stone");
			//                             World_Interaction.changeTerrain(collisionPos + new Vector3(-1, 0, 0), "Stone");
			//                             World_Interaction.changeTerrain(collisionPos + new Vector3(0, 0, -1), "Stone");
			//                             World_Interaction.changeTerrain(collisionPos + new Vector3(-1, 0, -1), "Stone");
			//                             World_Interaction.changeTerrain(collisionPos + new Vector3(1, 0, -1), "Stone");
			//                             World_Interaction.changeTerrain(collisionPos + new Vector3(-1, 0, 1), "Stone");
			//                         }
			//                         else if (foundChunk.chunkMap[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z].name == "Stone")
			//                         {
			//                             World_Interaction.changeTerrain(collisionPos, "Stone");
			//                             World_Interaction.changeTerrain(collisionPos + new Vector3(1, 1, 0), "Stone");
			//                             World_Interaction.changeTerrain(collisionPos + new Vector3(0, 1, 1), "Stone");
			//                             World_Interaction.changeTerrain(collisionPos + new Vector3(1, 1, 1), "Stone");
			//                             World_Interaction.changeTerrain(collisionPos + new Vector3(-1, 1, 0), "Stone");
			//                             World_Interaction.changeTerrain(collisionPos + new Vector3(0, 1, -1), "Stone");
			//                             World_Interaction.changeTerrain(collisionPos + new Vector3(-1, 1, -1), "Stone");
			//                             World_Interaction.changeTerrain(collisionPos + new Vector3(1, 1, -1), "Stone");
			//                             World_Interaction.changeTerrain(collisionPos + new Vector3(-1, 1, 1), "Stone");
			//                         }
			//                         break;
			//			case "Stone":
			//                         //Debug.Log("Stone");
			//                         World_Interaction.changeTerrain(collisionPos + new Vector3(0, 1, 0), "Diamond");
			//                         World_Interaction.changeTerrain(collisionPos + new Vector3(1, 1, 0), "Stone");
			//                         World_Interaction.changeTerrain(collisionPos + new Vector3(0, 1, 1), "Stone");
			//                         World_Interaction.changeTerrain(collisionPos + new Vector3(1, 1, 1), "Stone");
			//                         World_Interaction.changeTerrain(collisionPos + new Vector3(-1, 1, 0), "Stone");
			//                         World_Interaction.changeTerrain(collisionPos + new Vector3(0, 1, -1), "Stone");
			//                         World_Interaction.changeTerrain(collisionPos + new Vector3(-1, 1, -1), "Stone");
			//                         World_Interaction.changeTerrain(collisionPos + new Vector3(1, 1, -1), "Stone");
			//                         World_Interaction.changeTerrain(collisionPos + new Vector3(-1, 1, 1), "Stone");
			//                         break;
			//			case "Grass":
			//                         //Debug.Log("Grass");
			//                         World_Interaction.changeTerrain(collisionPos, "Diamond");
			//                         World_Interaction.changeTerrain(collisionPos + new Vector3(1, 0, 0), "Stone");
			//                         World_Interaction.changeTerrain(collisionPos + new Vector3(0, 0, 1), "Stone");
			//                         World_Interaction.changeTerrain(collisionPos + new Vector3(1, 0, 1), "Stone");
			//                         World_Interaction.changeTerrain(collisionPos + new Vector3(-1, 0, 0), "Stone");
			//                         World_Interaction.changeTerrain(collisionPos + new Vector3(0, 0, -1), "Stone");
			//                         World_Interaction.changeTerrain(collisionPos + new Vector3(-1, 0, -1), "Stone");
			//                         World_Interaction.changeTerrain(collisionPos + new Vector3(1, 0, -1), "Stone");
			//                         World_Interaction.changeTerrain(collisionPos + new Vector3(-1, 0, 1), "Stone");
			//                         break;


			//		}
			//	}
			//}

		}
		else 
		{
		
		}
	}

}
