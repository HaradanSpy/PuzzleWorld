using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleDirector : MonoBehaviour
{
    [Header("Main")]
    public int textureId;
    public float pieceDistance;
    public List<Texture> textures = new List<Texture>();
    public GameObject puzzlesPrefab;
    public List<PuzzlePiece> puzzles = new List<PuzzlePiece>();
    public Material material;

    public int textureTableId;
    public List<Texture> texturesTable = new List<Texture>();
    public Material materialTable;

    private List<Vector3> startPoints = new List<Vector3>();

    private LayerMask layerMaskActive;
    public bool pieceOn;
    public PuzzlePiece currentPiece;

    public bool checkUp;
    public bool checkDown;
    public bool checkLeft;
    public bool checkRight;

    public GameObject UpSphere;
    public GameObject DownSphere;
    public GameObject LeftSphere;
    public GameObject RightSphere;

    [Header("Ui")]
    public Text winText;
    public GameObject winButton;
    public Image helpImage;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clip_pickup;
    public AudioClip clip_drop;
    public AudioClip clip_connect;
    public AudioClip clip_win;
    private void Start()
    {
        layerMaskActive = ~(LayerMask.GetMask("puzzlePieceActive") | LayerMask.GetMask("puzzlePieceInactive"));
        Camera.main.transform.position = new Vector3(0,190,35);

        GameObject puzz = Instantiate(puzzlesPrefab, transform);
        if (puzzles.Count > 0) 
        {
            foreach (PuzzlePiece pz in puzzles)
            {
                Destroy(pz.gameObject);
            }
        }
        puzzles.Clear();
        puzzles.AddRange(puzz.GetComponentsInChildren<PuzzlePiece>());

        for (int i = 0; i < puzzles.Count; i++) // сохранение стартовых позиций
        {
            startPoints.Add(puzzles[i].transform.position);
        }
        UpSphere.SetActive(false);
        DownSphere.SetActive(false);
        LeftSphere.SetActive(false);
        RightSphere.SetActive(false);
        SetHelpImage();

        winText.gameObject.SetActive(false);
        winButton.SetActive(false);
        DropPieces();
    }
    public void Restart() 
    {
        for (int i = 0; i < puzzles.Count; i++) // Возврат на стартовые позиции
        {
            puzzles[i].transform.position = startPoints[i];
            puzzles[i].transform.rotation = Quaternion.Euler(-90,0,0);
        }
    }
    public void RestartGame() 
    {
        winText.gameObject.SetActive(false);
        winButton.SetActive(false);
        Camera.main.transform.position = new Vector3(0, 190, 35);

        GameObject puzz = Instantiate(puzzlesPrefab, transform);
        if (puzzles.Count > 0)
        {
            foreach (PuzzlePiece pz in puzzles)
            {
                Destroy(pz.gameObject);
            }
        }
        puzzles.Clear();
        puzzles.AddRange(puzz.GetComponentsInChildren<PuzzlePiece>());
        DropPieces();
    }
    public void DropPieces() 
    {
        for (int i = 0; i < puzzles.Count; i++) // сохранение стартовых позиций
        {
            Vector3 temp = puzzles[i].transform.position;

            float X = Random.Range( - 130f, 130f);
            float Y = Random.Range(150f,250f);
            float Z = Random.Range( - 100f, 80f);

            Vector3 vector = new Vector3(X,Y,Z);

           puzzles[i].transform.position = vector;
        }
    }
    public void SetTexture() // смена текстуры
    {
        textureId ++;
        if (textureId == textures.Count) 
        {
            textureId = 0;
        }
        material.mainTexture = textures[textureId];
        SetHelpImage();
    }
    public void SetTextureTable() // смена текстуры стола
    {
        textureTableId++;
        if (textureTableId == texturesTable.Count)
        {
            textureTableId = 0;
        }
        materialTable.mainTexture = texturesTable[textureTableId];
    }
    private void SetHelpImage() 
    {
        Texture tex = textures[textureId];
        helpImage.sprite = Sprite.Create((Texture2D)tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
    }

    private void Update()
    {
        Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y - (Input.mouseScrollDelta.y * 5), Camera.main.transform.position.z);
        if (Input.GetMouseButton(2)) 
        {
            CameraMove();
        }
        if (pieceOn && currentPiece)
        {
            MovePiece();
            CheckNeighbour();
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (currentPiece != null)
            {
                if (currentPiece.puzzlePieces.Count >= 60)
                {
                    Win();
                }
                if (!checkUp && !checkDown && !checkLeft && !checkRight)
                {
                    ThrowPiece();
                    return;
                }
                else 
                {
                    if (checkUp && !currentPiece.UpCheck && currentPiece.Up > 0 && puzzles[currentPiece.UpId].Down > 0)
                    {
                        ConnectPieces(1);
                    }
                    else 
                    {
                        if (checkDown && !currentPiece.DownCheck && currentPiece.Down > 0 && puzzles[currentPiece.DownId].Up > 0) 
                        {
                            ConnectPieces(2);
                        }
                        else
                        {
                            if (checkLeft && !currentPiece.LeftCheck && currentPiece.Left > 0 && puzzles[currentPiece.LeftId].Right > 0)
                            {
                                ConnectPieces(3);
                            }
                            else
                            {
                                if (checkRight && !currentPiece.RightCheck && currentPiece.Right > 0 && puzzles[currentPiece.Right].RightId > 0)
                                {
                                    ConnectPieces(4);
                                }
                                else ThrowPiece();
                            }
                        }
                    }
                }
            }
            else
            {
                if (!winButton.activeSelf) 
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                    {
                        if (hit.collider.tag == "puzzlePiece")
                        {
                            PickPiece(hit.transform);

                        }
                    }
                }
            }
        }
    }
    private void CameraMove() // перемещение камеры
    {
        float X = Input.GetAxis("Mouse X");
        float Z = Input.GetAxis("Mouse Y");

        Camera.main.transform.position = new Vector3(Camera.main.transform.position.x + (X * 5), Camera.main.transform.position.y, Camera.main.transform.position.z + (Z * 5));
    }

    private void ConnectPieces(int id)
    {
        Debug.Log("connect " + id);

        audioSource.PlayOneShot(clip_connect);

        PuzzlePiece newPiece = puzzles[0];
        Vector3 movePoint = Vector3.zero;

        switch (id)
        {
            case 1:
                newPiece = puzzles[currentPiece.UpId];
                currentPiece.UpCheck = true;
                newPiece.DownCheck = true;
                movePoint = new Vector3(0, 0, 20);
                break;
            case 2:
                newPiece = puzzles[currentPiece.DownId];
                currentPiece.DownCheck = true;
                newPiece.UpCheck = true;
                movePoint = new Vector3(0, 0, -20);
                break;
            case 3:
                newPiece = puzzles[currentPiece.LeftId];
                currentPiece.LeftCheck = true;
                newPiece.RightCheck = true;
                movePoint = new Vector3(-20, 0, 0);
                break;
            case 4:
                newPiece = puzzles[currentPiece.RightId];
                currentPiece.RightCheck = true;
                newPiece.LeftCheck = true;
                movePoint = new Vector3(20, 0, 0);
                break;

        }

        Vector3 vector = newPiece.transform.position;

        Vector3 mainOldPos = currentPiece.transform.position; // запоминаем позиции зависимых деталей
        List<Vector3> startPositions = new List<Vector3>();
        foreach (PuzzlePiece pp in currentPiece.puzzlePieces)
        {
            startPositions.Add(pp.transform.position);
            pp.rb.isKinematic = true;
        }

        currentPiece.transform.position = new Vector3(vector.x + movePoint.x, vector.y + movePoint.y, vector.z + movePoint.z);
        vector = currentPiece.transform.position;

        for (int i = 0; i < currentPiece.puzzlePieces.Count; i++) // смещаем все зависимые на поправку
        {
            float x = startPositions[i].x - mainOldPos.x;
            float y = startPositions[i].y - mainOldPos.y;
            float z = startPositions[i].z - mainOldPos.z;

            currentPiece.puzzlePieces[i].transform.position = new Vector3(vector.x + x, vector.y + y, vector.z + z);
        }

        if (!currentPiece.puzzlePieces.Exists(piece => piece.Id == newPiece.Id))
        {
            // Debug.Log("список ++ ");
            currentPiece.puzzlePieces.AddRange(newPiece.puzzlePieces);
            foreach (PuzzlePiece pp in currentPiece.puzzlePieces)
            {
                pp.gameObject.layer = LayerMask.NameToLayer("puzzlePieceInactive");
                pp.puzzlePieces = currentPiece.puzzlePieces;
            }
        }
        else 
        {
           // Debug.Log("список -- ");
        }
        if (currentPiece.puzzlePieces.Count >= 60)
        {
            Win();
        }
        ThrowPiece();
    }
    public void Win()
    {
        audioSource.PlayOneShot(clip_win);
        winText.gameObject.SetActive(true);
        winButton.SetActive(true);
        Debug.Log("Победа!");
    }

    private void CheckNeighbour() // проверка соседей для сборки
    {
        if (currentPiece.Right > 0)
        {
            Vector3 temp;
            Vector3 targetPoint;
            Vector3 temp2;
            Vector3 piecePoint;
            if (currentPiece.Right == 1)
            {
                temp = currentPiece.transform.position;
                targetPoint = new Vector3(temp.x - 5, temp.y, temp.z);
                temp2 = puzzles[currentPiece.RightId].transform.position;
                piecePoint = new Vector3(temp2.x + 15, temp2.y, temp2.z);
            }
            else
            {
                temp = currentPiece.transform.position;
                targetPoint = new Vector3(temp.x - 15, temp.y, temp.z);
                temp2 = puzzles[currentPiece.RightId].transform.position;
                piecePoint = new Vector3(temp2.x + 5, temp2.y, temp2.z);
            }

            float d = Vector3.Distance(targetPoint, piecePoint);
            if (d < pieceDistance && !puzzles[currentPiece.RightId].LeftCheck)
            {
                RightSphere.transform.position = targetPoint;
                RightSphere.SetActive(false);
                checkRight = true;

            }
            else
            {
                RightSphere.SetActive(false);
                checkRight = false;
            }
        }
        if (currentPiece.Left > 0)
        {
            Vector3 temp;
            Vector3 targetPoint;
            Vector3 temp2;
            Vector3 piecePoint;
            if (currentPiece.Left == 1)
            {

                temp = currentPiece.transform.position;
                targetPoint = new Vector3(temp.x + 5, temp.y, temp.z);
                temp2 = puzzles[currentPiece.LeftId].transform.position;
                piecePoint = new Vector3(temp2.x - 15, temp2.y, temp2.z);
            }
            else
            {
                temp = currentPiece.transform.position;
                targetPoint = new Vector3(temp.x + 15, temp.y, temp.z);
                temp2 = puzzles[currentPiece.LeftId].transform.position;
                piecePoint = new Vector3(temp2.x - 5, temp2.y, temp2.z);
            }

            float d = Vector3.Distance(targetPoint, piecePoint);
            if (d < pieceDistance && !puzzles[currentPiece.LeftId].RightCheck)
            {

                LeftSphere.transform.position = targetPoint;
                LeftSphere.SetActive(false);
                checkLeft = true;
            }
            else
            {
                LeftSphere.SetActive(false);
                checkLeft = false;
            }
        }

        if (currentPiece.Down > 0)
        {
            Vector3 temp;
            Vector3 targetPoint;
            Vector3 temp2;
            Vector3 piecePoint;
            if (currentPiece.Down == 1)
            {
                temp = currentPiece.transform.position;
                targetPoint = new Vector3(temp.x, temp.y, temp.z + 5);
                temp2 = puzzles[currentPiece.DownId].transform.position;
                piecePoint = new Vector3(temp2.x, temp2.y, temp2.z - 15);
            }
            else
            {
                temp = currentPiece.transform.position;
                targetPoint = new Vector3(temp.x, temp.y, temp.z + 15);
                temp2 = puzzles[currentPiece.DownId].transform.position;
                piecePoint = new Vector3(temp2.x, temp2.y, temp2.z - 5);
            }

            float d = Vector3.Distance(targetPoint, piecePoint);
            if (d < pieceDistance && !puzzles[currentPiece.DownId].UpCheck)
            {

                DownSphere.transform.position = targetPoint;
                DownSphere.SetActive(false);
                checkDown = true;
            }
            else
            {
                DownSphere.SetActive(false);
                checkDown = false;
            }
        }
        if (currentPiece.Up > 0)
        {
            Vector3 temp;
            Vector3 targetPoint;
            Vector3 temp2;
            Vector3 piecePoint;
            if (currentPiece.Up == 1)
            {
                temp = currentPiece.transform.position;
                targetPoint = new Vector3(temp.x, temp.y, temp.z - 5);
                temp2 = puzzles[currentPiece.UpId].transform.position;
                piecePoint = new Vector3(temp2.x, temp2.y, temp2.z + 15);
            }
            else
            {
                temp = currentPiece.transform.position;
                targetPoint = new Vector3(temp.x, temp.y, temp.z - 15);
                temp2 = puzzles[currentPiece.UpId].transform.position;
                piecePoint = new Vector3(temp2.x, temp2.y, temp2.z + 5);
            }

            float d = Vector3.Distance(targetPoint, piecePoint);
            if (d < pieceDistance && !puzzles[currentPiece.UpId].DownCheck)
            {
                UpSphere.transform.position = targetPoint;
                UpSphere.SetActive(false);
                checkUp = true;
            }
            else
            {
                UpSphere.SetActive(false);
                checkUp = false;
            }
        }
    }
    void PickPiece(Transform tr) // подбор детальки
    {
        Debug.Log("PickPiece");
        audioSource.PlayOneShot(clip_pickup);
        pieceOn = true;
        currentPiece = tr.GetComponent<PuzzlePiece>();
        currentPiece.transform.rotation = Quaternion.Euler(-90, 0, 0);
        currentPiece.rb.isKinematic = true;
    }
    void ThrowPiece()  // выбрасывание детальки
    {
        Debug.Log("ThrowPiece");
        audioSource.PlayOneShot(clip_drop);
        currentPiece.rb.isKinematic = false;

        foreach (PuzzlePiece pp in currentPiece.puzzlePieces)
        {
            pp.rb.isKinematic = false;
        }

        pieceOn = false;
        currentPiece = null;
    }
    void MovePiece()
    {
        if (currentPiece) 
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMaskActive))
            {
                Vector3 mainOldPos = currentPiece.transform.position; // запоминаем позиции зависимых деталей
                List<Vector3> startPositions = new List<Vector3>();
                foreach (PuzzlePiece pp in currentPiece.puzzlePieces)
                {
                    startPositions.Add(pp.transform.position);
                    pp.rb.isKinematic = true;
                }

                Vector3 vector = new Vector3(hit.point.x, hit.point.y + 5, hit.point.z); // двигаем выделенную деталь
                currentPiece.transform.position = vector;

                for (int i = 0; i < currentPiece.puzzlePieces.Count; i++) // смещаем все зависимые на поправку
                {
                    float x = startPositions[i].x - mainOldPos.x;
                    float y = startPositions[i].y - mainOldPos.y;
                    float z = startPositions[i].z - mainOldPos.z;

                    currentPiece.puzzlePieces[i].transform.position = new Vector3(vector.x + x, vector.y + y, vector.z + z);
                }
            }
        }
    }
}
