using UnityEngine;

public class Pawn : Piece{
    [HideInInspector] public Vector2 enPassantSquare; // The square where the pawn can be captured en passant
    public override void FinalizeMove(Vector3 targetPosition)
    {
        // Call the base class method to finalize the move
        base.FinalizeMove(targetPosition);
        
        int file = Mathf.RoundToInt(targetPosition.x);
        int rank = Mathf.RoundToInt(targetPosition.y);

        int forwardDirection = (pieceColor == PieceColor.White) ? 1 : -1; // Determine the forward direction based on piece color

        if((Vector2)transform.position == enPassantSquare){
            Square targetSquare = board.squares[(int)transform.position.x, (int)transform.position.y - forwardDirection];

            if(targetSquare.isOccupied){
                Piece targetPiece = targetSquare.occupyingPiece;
                if(targetPiece != null && targetPiece.pieceColor != pieceColor){
                    // Capture the target piece and remove it from the board
                    board.piecesOnBoard.Remove(targetPiece);
                    Destroy(targetPiece.gameObject); // Destroy the captured piece
                    targetSquare.ClearOccupyingPiece();
                }
            }
        }
       
        int promotionRank = (pieceColor == PieceColor.White) ? 7 : 0; // The rank where promotion is possible
        if (occupyingSquare.rank == promotionRank)
        {
            // Handle promotion logic here (e.g., change to a queen, rook, bishop, or knight)
            // For simplicity, we'll just promote to a queen
            int pieceIndex = (pieceColor == PieceColor.White) ? 4 : 10; // Set the piece index to the queen
            Piece newPiece = Instantiate(board.pieces[pieceIndex], new Vector3(file, rank, 0), Quaternion.identity).GetComponent<Piece>();
            newPiece.transform.parent = board.transform;
            newPiece.pieceColor = pieceColor; // Set the color of the new piece
            newPiece.ownKing = ownKing; // Set the reference to the king
            board.piecesOnBoard.Add(newPiece); // Add the new piece to the board
            board.piecesOnBoard.Remove(this); // Remove the old pawn from the board
            Destroy(gameObject); // Destroy the pawn
        }
    }
}