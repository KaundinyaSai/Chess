using UnityEngine;

public class Square : MonoBehaviour
{
    public bool isOccupied => occupyingPiece != null;
    [HideInInspector] public Piece occupyingPiece; // hide this from the Inspector

    public void SetOccupyingPiece(Piece piece)
    {
        occupyingPiece = piece;
    }

    public void ClearOccupyingPiece()
    {
        occupyingPiece = null;
    }
}

