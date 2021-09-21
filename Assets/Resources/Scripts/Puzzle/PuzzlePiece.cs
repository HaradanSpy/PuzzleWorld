using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePiece : MonoBehaviour
{
    public int Id;

    public int UpId;
    public int DownId;
    public int LeftId;
    public int RightId;

    [Tooltip ("0 - нет, 1 - впадина, 2 - выступ")]
    public int Up;
    [Tooltip("0 - нет, 1 - впадина, 2 - выступ")]
    public int Down;
    [Tooltip("0 - нет, 1 - впадина, 2 - выступ")]
    public int Left;
    [Tooltip("0 - нет, 1 - впадина, 2 - выступ")]
    public int Right;

    public bool UpCheck;
    public bool DownCheck;
    public bool LeftCheck;
    public bool RightCheck;

    public List<PuzzlePiece> puzzlePieces = new List<PuzzlePiece>();

    public Rigidbody rb;

    private void Awake()
    {
        rb = transform.GetComponent<Rigidbody>();
        puzzlePieces.Add(this);
    }
}
