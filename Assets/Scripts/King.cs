using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

public class King : Piece{
    
    public bool isInCheck = false; 
    public bool isInCheckMate = false;
    public bool isInStaleMate = false; 
    public List<Piece> checkingPieces = new List<Piece>(); // List to store pieces checking the king
    public override void Start()
    {
        base.Start();
    }
    
    public void Update(){
        if(isInCheckMate){
            // Handle checkmate logic here
            Debug.Log("Checkmate! " + pieceColor + " loses.");
        }else if(isInStaleMate){
            // Handle stalemate logic here
            Debug.Log("Stalemate!") ;
        }
    }

    public void CheckForChecks()
    {
        isInCheck = false;
        checkingPieces.Clear();

        int kingFile = occupyingSquare.file;
        int kingRank = occupyingSquare.rank;

        foreach (Piece piece in board.piecesOnBoard)
        {
            if (piece.pieceColor != pieceColor)
            {
                List<Vector2> attackedSquares = LegalMoves.GetLegalMovesAt(piece, piece.occupyingSquare.file, piece.occupyingSquare.rank);

                foreach (Vector2 square in attackedSquares)
                {
                    if ((int)square.x == kingFile && (int)square.y == kingRank)
                    {
                        isInCheck = true;
                        if (!checkingPieces.Contains(piece))
                        {
                            checkingPieces.Add(piece);
                            if (checkingPieces.Count >= 2)
                                break; // Max is double check
                        }
                    }
                }
            }
        }

        if (!isInCheck)
        {
            checkingPieces.Clear();
        }
    }

    public void CheckForCheckMate()
    {
        // Iterate through all pieces of the same color
        foreach (Piece piece in board.piecesOnBoard)
        {
            if (piece.pieceColor == pieceColor) // Same color as the king
            {
                List<Vector2> legalMoves = LegalMoves.GetLegalMovesAt(piece, piece.occupyingSquare.file, piece.occupyingSquare.rank);

                // If any piece has a legal move, it's not checkmate
                if (legalMoves.Count > 0)
                {
                    isInCheckMate = false;
                    return;
                }
            }
        }

        if(isInCheck)
        {
            isInCheckMate = true; // If no pieces have legal moves and the king is in check, it's checkmate
        }else{
            int turnRemainder = pieceColor == PieceColor.White ? 0 : 1; 
            if (board.turn % 2 == turnRemainder) // Check if it's the correct turn
            {
                isInStaleMate = true;
            }
        }
    }

    
    public override void FinalizeMove(Vector3 targetPosition){
        base.FinalizeMove(targetPosition);
        
        // Castling logic
        Castling(targetPosition);
    }

    void Castling(Vector2 targetPosition){
        int rank = Mathf.RoundToInt(targetPosition.y);
        int file = Mathf.RoundToInt(targetPosition.x);
        
        int rookFile = (file > originalSquarePosition.x) ? 7 : 0; // Rook's file
        Square rookSquare = board.squares[rookFile, rank];
        if (rookSquare.isOccupied && rookSquare.occupyingPiece.pieceType == PieceType.Rook &&
            rookSquare.occupyingPiece.pieceColor == pieceColor)
        {
            if(transform.position.x != originalSquarePosition.x 
                && transform.position.x - originalSquarePosition.x != 1 
                && transform.position.x - originalSquarePosition.x != -1){ 
                Piece rook = rookSquare.occupyingPiece;
                rook.transform.position = new Vector3(file - (rookFile > file ? 1 : -1), rank, 0); // Move the rook
                board.squares[file - (rookFile > file ? 1 : -1), rank].SetOccupyingPiece(rook); // Set the new square
                rookSquare.ClearOccupyingPiece();
            }
        }
    }
}