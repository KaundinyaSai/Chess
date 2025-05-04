using UnityEngine;

public class Pawn : Piece{
   public override void FinalizeMove(Vector3 targetPosition)
    {
        // Call the base class method to finalize the move
        base.FinalizeMove(targetPosition);
        
        int file = Mathf.RoundToInt(targetPosition.x);
        int rank = Mathf.RoundToInt(targetPosition.y);

        int EnPassantRank = (pieceColor == PieceColor.White) ? 4 : 3; // The rank where en passant is possible

        int forwardDir = (pieceColor == PieceColor.White) ? 1 : -1;
        Square enPassantSquare = board.squares[file, rank - forwardDir];
        if (enPassantSquare.isOccupied &&
            enPassantSquare.occupyingPiece == board.lastMovedPiece &&
            enPassantSquare.occupyingPiece.pieceType == PieceType.Pawn &&
            transform.position.y == EnPassantRank)
        {
            Destroy(enPassantSquare.occupyingPiece.gameObject); // Capture the pawn
            enPassantSquare.ClearOccupyingPiece();
        }

        int promotionRank = (pieceColor == PieceColor.White) ? 7 : 0; // The rank where promotion is possible
        if (rank == promotionRank)
        {
            // Handle promotion logic here (e.g., change to a queen, rook, bishop, or knight)
            // For simplicity, we'll just promote to a queen
            int pieceIndex = (pieceColor == PieceColor.White) ? 4 : 10; // Set the piece index to the queen
            Piece newPiece = Instantiate(board.pieces[pieceIndex], new Vector3(file, rank, 0), Quaternion.identity).GetComponent<Piece>();
            newPiece.pieceColor = pieceColor;
            newPiece.board = board;
            newPiece.transform.parent = board.transform;
            board.piecesOnBoard.Add(newPiece); // Add the new piece to the board
            board.piecesOnBoard.Remove(this); // Remove the old pawn from the board
            Destroy(gameObject); // Destroy the pawn
        }
    }
}