using UnityEngine;
using UnityEngine.EventSystems;

public class King : Piece{
    
    public bool isInCheck = false; // Flag to indicate if the king is in check
    public bool isInCheckMate = false; // Flag to indicate if the king is in checkmate
    public override void FinalizeMove(Vector3 targetPosition){
        base.FinalizeMove(targetPosition); // Call the base class method to finalize the move
        
        int rank = Mathf.RoundToInt(targetPosition.y);
        int file = Mathf.RoundToInt(targetPosition.x);
        
        int rookFile = (file > originalSquarePosition.x) ? 7 : 0; // Rook's file
        Square rookSquare = board.squares[rookFile, rank];
        if (rookSquare.isOccupied && rookSquare.occupyingPiece.pieceType == PieceType.Rook &&
            rookSquare.occupyingPiece.pieceColor == pieceColor)
        {
            if(transform.position.x != originalSquarePosition.x 
                && transform.position.x - originalSquarePosition.x != 1 && transform.position.x - originalSquarePosition.x != -1){ // Check if the king has moved{
                Piece rook = rookSquare.occupyingPiece;
                rook.transform.position = new Vector3(file - (rookFile > file ? 1 : -1), rank, 0); // Move the rook
                board.squares[file - (rookFile > file ? 1 : -1), rank].SetOccupyingPiece(rook); // Set the new square
                rookSquare.ClearOccupyingPiece();
            }
            
        }
    }
}