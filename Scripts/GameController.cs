using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using NUnit.Framework.Internal;
using UnityEditor.Build;

public class GameController : MonoBehaviour
{
    private CubePos nowCube = new CubePos(0, 1, 0);
    public float cubeChangePlaceSpeed = 0.5f;
    public Transform cubeToPlace;
    private float camMoveToYPosition;

    public Text scoreTxt;

    public GameObject[] cubesToCreate;
        

    public GameObject allCubes, vfx;

    // Массив для UI элементов, которые нужно скрыть при старте
    public GameObject[] canvasStartPage;

    private Rigidbody allCubesRb;
    public Color[] bgColors;
    private Color toCameraColor;
    private bool IsLoos, firstCube;

    private List<Vector3> allCubesPositions = new List<Vector3> {
        new Vector3(0, 0, 0),
        new Vector3(1, 0, 0),
        new Vector3(-1, 0, 0),
        new Vector3(0, 1, 0),
        new Vector3(0, 0, 1),
        new Vector3(0, 0, -1),
        new Vector3(1, 0, 1),
        new Vector3(-1, 0, -1),
        new Vector3(-1, 0, 1),
        new Vector3(1, 0, -1),
    };

    private int prevCountMaxHorizontal;
    private Coroutine showCubePlace;

    private List<GameObject> posibleCubesToCreate = new List<GameObject>();
    private void Start()
    {
        if (PlayerPrefs.GetInt("score") < 5)
            posibleCubesToCreate.Add(cubesToCreate[0]);
        else if (PlayerPrefs.GetInt("score") < 10) 
            AddPossibleCubes(2);
        else if (PlayerPrefs.GetInt("score") < 15)
                AddPossibleCubes(3);
        else if (PlayerPrefs.GetInt("score") < 25)
            AddPossibleCubes(4);
        else if (PlayerPrefs.GetInt("score") < 35)
            AddPossibleCubes(5);
        else if (PlayerPrefs.GetInt("score") < 55)
            AddPossibleCubes(6);
        else if (PlayerPrefs.GetInt("score") < 75)
            AddPossibleCubes(7);
        else if (PlayerPrefs.GetInt("score") < 85)
            AddPossibleCubes(8);
        else if (PlayerPrefs.GetInt("score") < 100)
            AddPossibleCubes(9);
        else
            AddPossibleCubes(10);

        scoreTxt.text = "<size=70><color=#FFED00>Best:</color></size>" + PlayerPrefs.GetInt("score") + "\r\n<size=55><color=#000000>Now:</color></size> 0";
        toCameraColor = Camera.main.backgroundColor;
        allCubesRb = allCubes.GetComponent<Rigidbody>();
        showCubePlace = StartCoroutine(ShowCubePlace());

        // Проверяем, что массив заполнен
        if (canvasStartPage == null || canvasStartPage.Length == 0)
        {
            Debug.LogError("Массив canvasStartPage не заполнен в инспекторе!");
        }
    }

    private void Update()
    {
        // Проверка на ввод с помощью нового Input System
        bool inputDetected = false;

        // Проверка мыши
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            inputDetected = true;
        }

        // Проверка тача
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            inputDetected = true;
        }

        // Проверяем, не нажали ли на UI элемент
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // Если это первый клик и мы нажали на UI, не начинаем игру
            if (!firstCube)
                return;
        }

        if (inputDetected && !firstCube)
        {
            firstCube = true;
            HideStartUI();
        }

        if (inputDetected && firstCube)
        {
            if (cubeToPlace != null && cubeToPlace.gameObject != null)
            {
                GameObject createCube = null;
                if (posibleCubesToCreate.Count == 1)
                    createCube = posibleCubesToCreate[0];
                else
                    createCube = posibleCubesToCreate[UnityEngine.Random.Range(0, posibleCubesToCreate.Count)];
                // Создаем новый куб
                GameObject newCube = Instantiate(
                     createCube,cubeToPlace.position, Quaternion.identity) as GameObject;

                newCube.transform.SetParent(allCubes.transform);
                nowCube.setVector(cubeToPlace.position);
                allCubesPositions.Add(nowCube.getVector());

                if (PlayerPrefs.GetString("music") != "No")
                    GetComponent<AudioSource>().Play();

                GameObject newVfx = Instantiate(vfx, newCube.transform.position, Quaternion.identity) as GameObject;
                Destroy(newVfx, 1.5f);

                allCubesRb.isKinematic = true;
                allCubesRb.isKinematic = false;
            }

            SpawnPosition();
            MoveCameraChangeBg();
        }

        if (!IsLoos && allCubesRb != null && allCubesRb.linearVelocity.magnitude > 0.1f)
        {
            if (cubeToPlace != null && cubeToPlace.gameObject != null)
            {
                Destroy(cubeToPlace.gameObject);
                cubeToPlace = null;
            }
            IsLoos = true;
            StopCoroutine(showCubePlace);
        }
        if (Camera.main.backgroundColor != toCameraColor)
            Camera.main.backgroundColor = Color.Lerp(Camera.main.backgroundColor, toCameraColor, Time.deltaTime / 1.5f);

        ;

    }

    // Метод для скрытия стартового UI
    private void HideStartUI()
    {
        foreach (GameObject obj in canvasStartPage)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                // Или если хотите полностью уничтожить:
                // Destroy(obj);
            }
        }
        Debug.Log("Стартовый UI скрыт");
    }

    IEnumerator ShowCubePlace()
    {
        while (true)
        {
            SpawnPosition();
            yield return new WaitForSeconds(cubeChangePlaceSpeed);
        }
    }

    private void SpawnPosition()
    {
        if (cubeToPlace == null) return;
        List<Vector3> positions = new List<Vector3>();

        if (IsPositionEmpty(new Vector3(nowCube.x + 1, nowCube.y, nowCube.z)) && (cubeToPlace.position.x != nowCube.x + 1))
            positions.Add(new Vector3(nowCube.x + 1, nowCube.y, nowCube.z));
        if (IsPositionEmpty(new Vector3(nowCube.x - 1, nowCube.y, nowCube.z)) && (cubeToPlace.position.x != nowCube.x - 1))
            positions.Add(new Vector3(nowCube.x - 1, nowCube.y, nowCube.z));
        if (IsPositionEmpty(new Vector3(nowCube.x, nowCube.y + 1, nowCube.z)) && (cubeToPlace.position.y != nowCube.y + 1))
            positions.Add(new Vector3(nowCube.x, nowCube.y + 1, nowCube.z));
        if (IsPositionEmpty(new Vector3(nowCube.x, nowCube.y - 1, nowCube.z)) && (cubeToPlace.position.y != nowCube.y - 1))
            positions.Add(new Vector3(nowCube.x, nowCube.y - 1, nowCube.z));
        if (IsPositionEmpty(new Vector3(nowCube.x, nowCube.y, nowCube.z + 1)) && (cubeToPlace.position.z != nowCube.z + 1))
            positions.Add(new Vector3(nowCube.x, nowCube.y, nowCube.z + 1));
        if (IsPositionEmpty(new Vector3(nowCube.x, nowCube.y, nowCube.z - 1)) && (cubeToPlace.position.z != nowCube.z - 1))
            positions.Add(new Vector3(nowCube.x, nowCube.y, nowCube.z - 1));

        if (positions.Count > 1)

            cubeToPlace.position = positions[UnityEngine.Random.Range(0, positions.Count)];
        else if (positions.Count == 0)
            IsLoos = true;
        else
            cubeToPlace.position = positions[0];
    }

    private bool IsPositionEmpty(Vector3 targetPos)
    {
        if (targetPos.y == 0)
            return false;
        foreach (Vector3 pos in allCubesPositions)
            if (pos.x == targetPos.x && pos.y == targetPos.y && pos.z == targetPos.z)
                return false;
        return true;
    }

    private void MoveCameraChangeBg()
    {
        int maxX = 0, maxY = 0, maxZ = 0, maxHor;

        foreach (Vector3 pos in allCubesPositions)
        {
            if (Mathf.Abs(Convert.ToInt32(pos.x)) > maxX)
                maxX = Convert.ToInt32(pos.x);

            if (Mathf.Abs(Convert.ToInt32(pos.y)) > maxY)
                maxY = Convert.ToInt32(pos.y);

            if (Mathf.Abs(Convert.ToInt32(pos.z)) > maxZ)
                maxZ = Convert.ToInt32(pos.z);
        }

        Transform mainCam = Camera.main.transform;
        maxY--;

        if (PlayerPrefs.GetInt("score") < maxY)
            PlayerPrefs.SetInt("score", maxY);
        scoreTxt.text = "<size=70><color=#FFED00>Best:</color></size>" + PlayerPrefs.GetInt("score") + "\r\n<size=55><color=#000000>Now:</color></size>" + maxY;


        camMoveToYPosition = 5.9f + nowCube.y - 1f;
        mainCam.position = new Vector3(mainCam.position.x, camMoveToYPosition, mainCam.position.z);

        maxHor = maxX > maxZ ? maxX : maxZ;
        if (maxHor % 3 == 0 && prevCountMaxHorizontal != maxHor)
        {
            mainCam.localPosition -= new Vector3(0, 0, 2f);
            prevCountMaxHorizontal = maxHor;
        }
        if (maxY >= 7)
            toCameraColor = bgColors[2];
        else if (maxY >= 5)
            toCameraColor = bgColors[1];
        else if (maxY >= 2)
            toCameraColor = bgColors[0];

    }
    private void AddPossibleCubes(int till)
    {

        for(int i = 0; i < till; i++) 
            posibleCubesToCreate.Add(cubesToCreate[i]);
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

    public Vector3 getVector()
    {
        return new Vector3(x, y, z);
    }

    public void setVector(Vector3 pos)
    {
        x = Convert.ToInt32(pos.x);
        y = Convert.ToInt32(pos.y);
        z = Convert.ToInt32(pos.z);
    }
}