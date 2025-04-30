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

    [HideInInspector] public bool moved; // Flag to indicate if the piece has moved

    void Start()
    {
        board = GameObject.Find("Manager").GetComponent<Board>();
        SetOccupyingSquare(); // Set the square this piece occupies
        
        moved = false; // Initialize moved to false
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

        // Clear highlights from all squares
        foreach (Square square in board.squares)
        {
            square.isHighlightedNoramal = false; // Unhighlight the square
            square.isHighlightedOccupied = false; // Unhighlight the square
        }

        // Then pass the *original square position* into LegalMoves
        legalMoves = LegalMoves.GetLegalMovesAt(this, (int)originalSquarePosition.x, (int)originalSquarePosition.y);

        foreach(Vector2 move in legalMoves){
            Square moveSquare = board.squares[(int)move.x, (int)move.y];
            if(moveSquare.isOccupied && moveSquare.occupyingPiece.pieceColor != pieceColor){
                moveSquare.isHighlightedOccupied = true; // Highlight the square
            }else{
                moveSquare.isHighlightedNoramal = true; // Highlight the square
            }
        }

        board.lastClickedPiece = this; // Set the last clicked piece
    }


    void OnMouseDrag()
    {
        if(!isDraggingAllowed) return; // Prevent dragging if not allowed
        
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        transform.position = mouseWorldPos + offset; // Drag the piece around
    }

    public void FinalizeMove(Vector3 targetPosition)
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
            return;
        }

        // Handle capturing logic
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
        else if (pieceType == PieceType.Pawn) // En Passant capture
        {
            int forwardDir = (pieceColor == PieceColor.White) ? 1 : -1;
            Square enPassantSquare = board.squares[file, rank - forwardDir];
            if (enPassantSquare.isOccupied &&
                enPassantSquare.occupyingPiece == board.lastMovedPiece &&
                enPassantSquare.occupyingPiece.pieceType == PieceType.Pawn)
            {
                Destroy(enPassantSquare.occupyingPiece.gameObject); // Capture the pawn
                enPassantSquare.SetOccupyingPiece(null); // Clear the square
            }
        }

        // Handle castling logic
        if (pieceType == PieceType.King && Mathf.Abs(file - originalSquarePosition.x) == 2)
        {
            int rookFile = (file > originalSquarePosition.x) ? 7 : 0; // Rook's file
            Square rookSquare = board.squares[rookFile, rank];
            if (rookSquare.isOccupied && rookSquare.occupyingPiece.pieceType == PieceType.Rook &&
                rookSquare.occupyingPiece.pieceColor == pieceColor)
            {
                Piece rook = rookSquare.occupyingPiece;
                rook.transform.position = new Vector3(file - (rookFile > file ? 1 : -1), rank, 0); // Move the rook
                board.squares[file - (rookFile > file ? 1 : -1), rank].SetOccupyingPiece(rook); // Set the new square
                rookSquare.ClearOccupyingPiece();
            }
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
        board.lastMovedPiece = this; // Set the last moved piece
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
