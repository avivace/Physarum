using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private Camera m_Camera;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void MoveCamera(int x, int y){
    	Debug.Log("Moved camera");
        transform.position = new Vector3(x, y, -10);
    }
}
