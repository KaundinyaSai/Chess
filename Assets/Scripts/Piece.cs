using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public PieceType pieceType;
    public PieceColor pieceColor;

    Vector3 offset;
    [HideInInspector] public Board board;

    bool isDraggingAllowed = false; // Flag to control dragging

    [HideInInspector] public Square occupyingSquare; // The square this piece occupies

    private Vector2 originalSquarePosition; // NEW: to store starting square (rounded to grid)
    private List<Vector2> legalMoves;

    [HideInInspector] public Vector3 startPos;

    void Start()
    {
        board = GameObject.Find("Manager").GetComponent<Board>();
        SetOccupyingSquare(); // Set the square this piece occupies
        startPos = transform.position;
    }

    void OnMouseDown()
    {
        if (!CanMove())
        {
            isDraggingAllowed = false;
            return;
        }

        isDraggingAllowed = true;

        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        offset = transform.position - mouseWorldPos;
        
        // NEW: Store the *grid snapped* starting position
        originalSquarePosition = new Vector2(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y)
        );

        // Then pass the *original square position* into LegalMoves
        legalMoves = LegalMoves.GetLegalMovesAt(this, (int)originalSquarePosition.x, (int)originalSquarePosition.y);
    }


    void OnMouseDrag()
    {
        if(!isDraggingAllowed) return; // Prevent dragging if not allowed
        
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        transform.position = mouseWorldPos + offset; // Drag the piece around
    }

    void OnMouseUp()
    {
        if (!isDraggingAllowed) return;

        
        Vector3 worldPos = transform.position;
        
        int file = Mathf.RoundToInt(worldPos.x);
        int rank = Mathf.RoundToInt(worldPos.y);

        //Bounds check
        if (file < 0 || file > 7 || rank < 0 || rank > 7)
        {
            transform.position = originalSquarePosition;
            return;
        }

        // 4) See if the new square is in the legal moves
        Vector2 targetSquare = new Vector2(file, rank);
        if (!legalMoves.Contains(targetSquare))
        {
            transform.position = originalSquarePosition;
            return;
        }

        // 5) Check if another piece is on the destination square
        if (board.squares[file, rank].isOccupied)
        {
            Piece otherPiece = board.squares[file, rank].occupyingPiece;
            if (otherPiece.pieceColor != pieceColor)
            {
                Destroy(otherPiece.gameObject); // Capture
            }
            else
            {
                // Can't capture your own piece
                transform.position = originalSquarePosition;
                return;
            }
        }

        occupyingSquare.SetOccupyingPiece(null); // Clear old square
        occupyingSquare = board.squares[file, rank];
        occupyingSquare.SetOccupyingPiece(this);

        transform.position = new Vector3(file, rank, 0); // Snap to grid

        board.turn++;
    }

    
    bool CanMove(){
        if(pieceColor == PieceColor.White && board.turn % 2 == 0 || pieceColor == PieceColor.Black && board.turn % 2 == 1){
            return true;
        }else{
            return false;
        }
    }

    void SetOccupyingSquare()
    {
        foreach (Square square in board.squares)
        {
            // Compare positions with a small tolerance
            if (square.transform.position == transform.position)
            {
                square.SetOccupyingPiece(this);
                occupyingSquare = square; // Set the square this piece occupies
                return;
            }
        }
    }
}
