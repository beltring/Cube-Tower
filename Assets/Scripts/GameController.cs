using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public float cubeChangePosSpeed = 0.5f;
    public Transform cubeToPlace;
    public GameObject cubeToCreate;
    public GameObject allCubes;
    public GameObject[] canvasStartPage;
    public Color[] bgColors;
    public GameObject vfx;
    public Text scoreTxt;

    private CubePos nowCube = new CubePos(0, 1, 0);
    private Rigidbody allCubesRB;
    private bool isLose = false;
    private Coroutine showCubePlace;
    private bool firstCube;
    private float camMoveToYPos;
    private Transform mainCam;
    private float camMoveSpeed = 2f;
    private float prevCountMaxHorizontal = 0f;
    private Color toCameraColor;
    private List<Vector3> allCubesPositions = new List<Vector3>
    {
        new Vector3(0,0,0),
        new Vector3(0,1,0),
        new Vector3(1,0,0),
        new Vector3(0,0,1),
        new Vector3(-1,0,0),
        new Vector3(0,0,-1),
        new Vector3(1,0,1),
        new Vector3(-1,0,-1),
        new Vector3(-1,0,1),
        new Vector3(1,0,-1),
    };


    void Start()
    {
        scoreTxt.text = "<size=40><color=#A90101>Best</color></size> " + PlayerPrefs.GetInt("score") + "\n<size=22>Now</size> 0";

        toCameraColor = Camera.main.backgroundColor;
        mainCam = Camera.main.transform;
        camMoveToYPos = 5.9f + nowCube.y - 1f;
        allCubesRB = allCubes.GetComponent<Rigidbody>();
        this.showCubePlace = StartCoroutine(ShowCubePlace());
    }

    void Update()
    {
        if ((Input.GetMouseButtonDown(0) || Input.touchCount > 0) && cubeToPlace != null 
            && allCubes != null && !EventSystem.current.IsPointerOverGameObject())
        {
#if !UNITY_EDITOR
            if (Input.GetTouch(0).phase != TouchPhase.Began)
            {
                return;
            }
#endif

            if (!firstCube)
            {
                firstCube = true;
                foreach (var item in canvasStartPage)
                {
                    Destroy(item);
                }
            }

            GameObject newCube = Instantiate(cubeToCreate, cubeToPlace.position, Quaternion.identity) as GameObject;

            newCube.transform.SetParent(allCubes.transform);
            nowCube.SetVector(cubeToPlace.position);
            allCubesPositions.Add(nowCube.GetVector());

            if (PlayerPrefs.GetString("music") != "No")
            {
                GetComponent<AudioSource>().Play();
            }

            GameObject newVfx = Instantiate(vfx, newCube.transform.position, Quaternion.identity) as GameObject;
            Destroy(newVfx, 1.5f);

            allCubesRB.isKinematic = true;
            allCubesRB.isKinematic = false;
            this.SpawnPositions();

            this.MoveCameraChangeBg();
        }

        if (!isLose && allCubesRB.velocity.magnitude > 0.1f)
        {
            Destroy(cubeToPlace.gameObject);
            isLose = true;
            StopCoroutine(this.showCubePlace);
        }

        mainCam.localPosition = Vector3.MoveTowards(mainCam.localPosition, mainCam.localPosition = new Vector3(mainCam.localPosition.x, camMoveToYPos, mainCam.localPosition.z), camMoveSpeed * Time.deltaTime);
    
        if (Camera.main.backgroundColor != toCameraColor)
        {
            Camera.main.backgroundColor = Color.Lerp(Camera.main.backgroundColor, toCameraColor, Time.deltaTime / 1.5f);
        }
    }

    IEnumerator ShowCubePlace()
    {
        while (true)
        {
            this.SpawnPositions();
            yield return new WaitForSeconds(this.cubeChangePosSpeed);
        }
    }

    private void SpawnPositions()
    {
        List<Vector3> positions = new List<Vector3>(5);

        if (this.IsPositionEmpty(new Vector3(this.nowCube.x + 1, this.nowCube.y, this.nowCube.z))
            && this.nowCube.x + 1 != this.cubeToPlace.position.x)
        {
            positions.Add(new Vector3(this.nowCube.x + 1, this.nowCube.y, this.nowCube.z));
        }
        if (this.IsPositionEmpty(new Vector3(this.nowCube.x - 1, this.nowCube.y, this.nowCube.z))
            && this.nowCube.x - 1 != this.cubeToPlace.position.x)
        {
            positions.Add(new Vector3(this.nowCube.x - 1, this.nowCube.y, this.nowCube.z));
        }
        if (this.IsPositionEmpty(new Vector3(this.nowCube.x, this.nowCube.y + 1, this.nowCube.z))
            && this.nowCube.y + 1 != this.cubeToPlace.position.y)
        {
            positions.Add(new Vector3(this.nowCube.x, this.nowCube.y + 1, this.nowCube.z));
        }
        if (this.IsPositionEmpty(new Vector3(this.nowCube.x, this.nowCube.y - 1, this.nowCube.z))
            && this.nowCube.y - 1 != this.cubeToPlace.position.y)
        {
            positions.Add(new Vector3(this.nowCube.x, this.nowCube.y - 1, this.nowCube.z));
        }
        if (this.IsPositionEmpty(new Vector3(this.nowCube.x, this.nowCube.y, this.nowCube.z + 1))
            && this.nowCube.z + 1 != this.cubeToPlace.position.z)
        {
            positions.Add(new Vector3(this.nowCube.x, this.nowCube.y, this.nowCube.z + 1));
        }
        if (this.IsPositionEmpty(new Vector3(this.nowCube.x, this.nowCube.y, this.nowCube.z - 1))
            && this.nowCube.z - 1 != this.cubeToPlace.position.z)
        {
            positions.Add(new Vector3(this.nowCube.x, this.nowCube.y, this.nowCube.z -1));
        }

        if (positions.Count > 1)
        {
            this.cubeToPlace.position = positions[UnityEngine.Random.Range(0, positions.Count)];
        }
        else if (positions.Count == 0)
        {
            isLose = true;
        }
        else
        {
            this.cubeToPlace.position = positions[0];
        }
    }

    private bool IsPositionEmpty(Vector3 targetPos)
    {
        if (targetPos.y <= 0)
        {
            return false;
        }

        foreach (var item in allCubesPositions)
        {
            if (item.x == targetPos.x && item.y == targetPos.y && item.z == targetPos.z)
            {
                return false;
            }
        }

        return true;
    }

    private void MoveCameraChangeBg()
    {
        int maxX = 0;
        int maxY = 0;
        int maxZ = 0;
        int maxHorizontal;

        foreach (var item in allCubesPositions)
        {
            if (Mathf.Abs(Convert.ToInt32(item.x)) > maxX)
            {
                maxX = Convert.ToInt32(item.x);
            }

            if (Convert.ToInt32(item.y) > maxY)
            {
                maxY = Convert.ToInt32(item.y);
            }

            if (Mathf.Abs(Convert.ToInt32(item.z)) > maxZ)
            {
                maxZ = Convert.ToInt32(item.z);
            }
        }

        maxY--;

        if (PlayerPrefs.GetInt("score") < maxY)
        {
            PlayerPrefs.SetInt("score", maxY);
        }

        scoreTxt.text = "<size=40><color=#A90101>Best</color></size> " + PlayerPrefs.GetInt("score") + "\n<size=22>Now</size> " + maxY;

        camMoveToYPos = 5.9f + nowCube.y - 1f;
        maxHorizontal = maxX > maxZ ? maxX : maxZ;
        
        if(maxHorizontal % 3 == 0 && prevCountMaxHorizontal != maxHorizontal)
        {
            mainCam.localPosition -= new Vector3(0, 0, 2.5f);
            prevCountMaxHorizontal = maxHorizontal;
        }

        if (maxY >= 7)
        {
            Camera.main.backgroundColor = this.bgColors[2];
        }
        else if (maxX >= 5)
        {
            Camera.main.backgroundColor = this.bgColors[1];
        }
        else if (maxX >= 2)
        {
            Camera.main.backgroundColor = this.bgColors[0];
        }
    }
}


struct CubePos
{
    public int x, y, z;
    
    public CubePos(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3 GetVector()
    {
        return new Vector3(x, y, z);
    }

    public void SetVector(Vector3 vector3)
    {
        x = Convert.ToInt32(vector3.x);
        y = Convert.ToInt32(vector3.y);
        z = Convert.ToInt32(vector3.z);
    }
}