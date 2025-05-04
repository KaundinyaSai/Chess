using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public PieceType pieceType;
    public PieceColor pieceColor;

    Vector3 offset;
    [HideInInspector] public Board board;

    bool isDraggingAllowed = false; 

    [HideInInspector] public Square occupyingSquare;

    [HideInInspector] public Vector2 originalSquarePosition; 
    [HideInInspector] public List<Vector2> legalMoves;

    [HideInInspector] public bool moved; 

    Camera mainCamera;

    public King ownKing;

    public virtual void Start()
    {
        board = GameObject.Find("Manager").GetComponent<Board>();
        SetOccupyingSquare();
        GetLegalMoves(transform.position);
        
        moved = false;
        board.MarkAttacks(this);

        mainCamera = Camera.main;
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
        
        originalSquarePosition = new Vector2(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y)
        );

        // Clear highlights from all squares
        foreach (Square square in board.squares)
        {
            square.isHighlightedNoramal = false;
            square.isHighlightedOccupied = false;
        }

        GetLegalMoves(originalSquarePosition);

        foreach(Vector2 move in legalMoves){
            Square moveSquare = board.squares[(int)move.x, (int)move.y];
            if(moveSquare.isOccupied && moveSquare.occupyingPiece.pieceColor != pieceColor){
                moveSquare.isHighlightedOccupied = true;
            }else{
                moveSquare.isHighlightedNoramal = true;
            }
        }

        board.lastClickedPiece = this; 
    }


    void OnMouseDrag()
    {
        if(!isDraggingAllowed) return; 
        
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = mainCamera.WorldToScreenPoint(transform.position).z;
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);

        transform.position = mouseWorldPos + offset;
    }

    public virtual void FinalizeMove(Vector3 targetPosition)
    {
        int file = Mathf.RoundToInt(targetPosition.x);
        int rank = Mathf.RoundToInt(targetPosition.y);

        // Bounds check
        if (file < 0 || file > 7 || rank < 0 || rank > 7)
        {
            transform.position = originalSquarePosition;
            return;
        }

        // Check if the new square is in the legal moves
        Vector2 targetSquare = new Vector2(file, rank);
        if (!legalMoves.Contains(targetSquare))
        {
            transform.position = originalSquarePosition;
            occupyingSquare.SetOccupyingPiece(this); 
            return;
        }

        // Handle capturing logic
        if (board.squares[file, rank].isOccupied)
        {
            Piece otherPiece = board.squares[file, rank].occupyingPiece;
            if (otherPiece.pieceColor != pieceColor)
            {
                board.piecesOnBoard.Remove(otherPiece); // Remove the captured piece from the board
                Destroy(otherPiece.gameObject); // Capture
                board.captureSound.Play(); // Play capture sound
            }
            else
            {
                // Can't capture your own piece
                transform.position = originalSquarePosition;
                return;
            }
        }else{
            board.moveSound.Play(); // Play move sound
        }
        
        // Update board state
        board.lastMovedPieceStartFile = (int)originalSquarePosition.x;
        board.lastMovedPieceStartRank = (int)originalSquarePosition.y;
        board.lastMovedPieceEndFile = file;
        board.lastMovedPieceEndRank = rank;

        occupyingSquare.SetOccupyingPiece(null); // Clear old square
        occupyingSquare = board.squares[file, rank];
        occupyingSquare.SetOccupyingPiece(this);

        transform.position = new Vector3(file, rank, 0); // Snap to grid

        // Clear highlights
        foreach (Vector2 legalMove in legalMoves)
        {
            Square moveSquare = board.squares[(int)legalMove.x, (int)legalMove.y];
            moveSquare.isHighlightedNoramal = false; // Unhighlight the square
            moveSquare.isHighlightedOccupied = false; // Unhighlight the square
        }
        
        board.turn++;
        moved = true; // Set moved to true after a successful move
        board.AfterTurn(this); // Update the board state after the turn
        //ownKing.CheckForChecks();
    }

    void OnMouseUp()
    {
        if (!isDraggingAllowed) return;

        FinalizeMove(transform.position);
    }

    bool CanMove(){
        if(pieceColor == PieceColor.White && board.turn % 2 == 0 || pieceColor == PieceColor.Black && board.turn % 2 == 1){
            return true;
        }else{
            return false;
        }
    }

    public void SetOccupyingSquare()
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

    public void GetLegalMoves(Vector2 originalSquarePosition){
        // Get the legal moves for this piece
        legalMoves = LegalMoves.GetLegalMovesAt(this, (int)originalSquarePosition.x, (int)originalSquarePosition.y);
    }

    public bool IsSlidingPiece(){
        return pieceType == PieceType.Rook || pieceType == PieceType.Bishop || pieceType == PieceType.Queen;
    }
}
