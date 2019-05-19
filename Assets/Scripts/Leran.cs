using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leran : MonoBehaviour
{
	public GameObject TextMesh;
    // Start is called before the first frame update
    void Start()
    {
		Vector3[] list = {
		new Vector3( -0.5f, -0.5f, 0.5f ),
		 new Vector3( 0.5f, -0.5f, 0.5f ),
		 new Vector3( 0.5f, -0.5f, -0.5f ),
		new Vector3( -0.5f, -0.5f, -0.5f ),
		// Bottom vertices
		new Vector3( -0.5f, 0.5f, 0.5f ),
		 new Vector3( 0.5f, 0.5f, 0.5f ),
		 new Vector3(  0.5f,   0.5f, -0.5f ),
		new Vector3( -0.5f, 0.5f, -0.5f )
		};

		Quaternion zero = Quaternion.Euler( new Vector3( 0, 0, 0 ));
		var i = 0;
		foreach( Vector3 x in list){

			GameObject.Instantiate( TextMesh, x, zero ).GetComponent<TextMesh>().text = i.ToString();
			i++;
		}

	}

	// Update is called once per frame
	void Update()
    {
        
    }
}
