using UnityEngine;
using System.Collections.Generic;

public static class LegalMoves
{
    public static List<Vector2> GetLegalMovesAt(Piece piece, int file, int rank)
    {
        switch (piece.pieceType)
        {
            case PieceType.Pawn:
                return GetPawnMoves(piece, file, rank);
            case PieceType.Rook:
                return GetRookMoves(piece, file, rank);
            case PieceType.Knight:
                return GetKnightMoves(piece, file, rank);
            case PieceType.Bishop:
                return GetBishopMoves(piece, file, rank);
            case PieceType.Queen:
                return GetQueenMoves(piece, file, rank);
            case PieceType.King:
                return GetKingMoves(piece, file, rank);
            default:
                return new List<Vector2>();
        }
            
    }
    
    static List<Vector2> GetPawnMoves(Piece piece, int startFile, int startRank)
    {
        List<Vector2> moves = new List<Vector2>();

        int forwardDir = 1;

        if ((!piece.board.flipped && piece.pieceColor == PieceColor.Black) ||
            (piece.board.flipped && piece.pieceColor == PieceColor.White))
        {
            forwardDir = -1;
        }

        Board board = piece.board;

        // Forward one square
        int oneStepRank = startRank + forwardDir;
        if (oneStepRank >= 0 && oneStepRank <= 7 && !board.squares[startFile, oneStepRank].isOccupied)
        {
            moves.Add(new Vector2(startFile, oneStepRank));

            // Forward two squares (only if first move)
            int twoStepRank = startRank + 2 * forwardDir;
            if (piece.moved == false && 
                twoStepRank >= 0 && twoStepRank <= 7 && 
                !board.squares[startFile, twoStepRank].isOccupied)
            {
                moves.Add(new Vector2(startFile, twoStepRank));
            }
        }

        // Diagonal captures
        int[] fileOffsets = { -1, 1 };
        foreach (int offset in fileOffsets)
        {
            int targetFile = startFile + offset;
            int targetRank = startRank + forwardDir;

            if (targetFile >= 0 && targetFile <= 7 &&
                targetRank >= 0 && targetRank <= 7)
            {
                Square targetSquare = board.squares[targetFile, targetRank];
                if (targetSquare.isOccupied && targetSquare.occupyingPiece.pieceColor != piece.pieceColor)
                {
                    moves.Add(new Vector2(targetFile, targetRank));
                }

                Square enPassantSquare = board.squares[targetFile, startRank];
                if (!targetSquare.isOccupied && enPassantSquare.isOccupied &&
                    enPassantSquare.occupyingPiece.pieceType == PieceType.Pawn &&
                    enPassantSquare.occupyingPiece.pieceColor != piece.pieceColor &&
                    board.lastMovedPiece == enPassantSquare.occupyingPiece)
                {
                    // Validate that the last move was a two-square pawn advance
                    if (Mathf.Abs(board.lastMovedPieceStartRank - board.lastMovedPieceEndRank) == 2 &&
                        board.lastMovedPieceStartFile == targetFile)
                    {
                        moves.Add(new Vector2(targetFile, targetRank));
                    }
                }
            }
        }

        return moves;
    }


    static List<Vector2> GetBishopMoves(Piece piece, int startFile, int startRank){
        
        Vector2[] directions = new[] {
            new Vector2(1, 1), new Vector2(-1, 1),
            new Vector2(1, -1), new Vector2(-1, -1)
        };

        List<Vector2> moves = GetMovesFromDirections(directions, startFile, startRank, piece, 8);
        
        return moves;
    }

    static List<Vector2> GetKnightMoves(Piece piece, int startFile, int startRank){
        Vector2[] directions = new[] {
            new Vector2(2, 1), new Vector2(2, -1),
            new Vector2(-2, 1), new Vector2(-2, -1),
            new Vector2(1, 2), new Vector2(1, -2),
            new Vector2(-1, 2), new Vector2(-1, -2)
        };

        List<Vector2> moves = GetMovesFromDirections(directions, startFile, startRank, piece, 2);        
        return moves;
    }

    public static List<Vector2> GetRookMoves(Piece piece, int startFile, int startRank)
    {
        Vector2[] directions = new[] {
            new Vector2(1, 0), new Vector2(-1,0),
            new Vector2(0, 1), new Vector2(0, -1)
        };

        List<Vector2> moves = GetMovesFromDirections(directions, startFile, startRank, piece, 8);
        return moves;
    }

    static List<Vector2> GetQueenMoves(Piece piece, int startFile, int startRank){
        Vector2[] directions = new[] {
            new Vector2(1, 0), new Vector2(-1,0),
            new Vector2(0, 1), new Vector2(0, -1),
            new Vector2(1, 1), new Vector2(-1, 1),
            new Vector2(1, -1), new Vector2(-1, -1)
        };

        List<Vector2> moves = GetMovesFromDirections(directions, startFile, startRank, piece, 8);

        return moves;
    }

    static List<Vector2> GetKingMoves(Piece piece, int startFile, int startRank){
        Vector2[] direction = new Vector2[]{
            new Vector2(1, 0), new Vector2(-1,0),
            new Vector2(0, 1), new Vector2(0, -1),
            new Vector2(1, 1), new Vector2(-1, 1),
            new Vector2(1, -1), new Vector2(-1, -1)
        };  

        List<Vector2> moves = GetMovesFromDirections(direction, startFile, startRank, piece, 2);

        // Castling
        Board board = piece.board;
        if(!piece.moved && piece.pieceColor == PieceColor.White){ // White king
            Piece kingSideRook = board.squares[7, 0].occupyingPiece;
            Piece queenSideRook = board.squares[0, 0].occupyingPiece;

            //KingSide
            if(kingSideRook != null && kingSideRook.pieceType == PieceType.Rook && kingSideRook.pieceColor == PieceColor.White 
            && !kingSideRook.moved){
                if(!board.squares[5, 0].isOccupied && !board.squares[6, 0].isOccupied){
                    moves.Add(new Vector2(6, 0));
                }
            }
            //QueenSide
            if(queenSideRook != null && queenSideRook.pieceType == PieceType.Rook && queenSideRook.pieceColor == PieceColor.White 
            && !queenSideRook.moved){
                if(!board.squares[1, 0].isOccupied && !board.squares[2, 0].isOccupied && !board.squares[3, 0].isOccupied){
                    moves.Add(new Vector2(2, 0));
                }
            }
        }else if(!piece.moved && piece.pieceColor == PieceColor.Black){ // Black king
            Piece kingSideRook = board.squares[7, 7].occupyingPiece;
            Piece queenSideRook = board.squares[0, 7].occupyingPiece;
            //KingSide
            if(kingSideRook != null && kingSideRook.pieceType == PieceType.Rook && kingSideRook.pieceColor == PieceColor.Black 
            && !kingSideRook.moved){
                if(!board.squares[5, 7].isOccupied && !board.squares[6, 7].isOccupied){
                    moves.Add(new Vector2(6, 7));
                }
            }
            //QueenSide
            if(queenSideRook != null && queenSideRook.pieceType == PieceType.Rook && queenSideRook.pieceColor == PieceColor.Black 
            && !queenSideRook.moved){
                if(!board.squares[1, 7].isOccupied && !board.squares[2, 7].isOccupied && !board.squares[3, 7].isOccupied){
                    moves.Add(new Vector2(2, 7));
                }
            }
        }
        
        
        return moves;
    }

    static List<Vector2> GetMovesFromDirections(Vector2[] directions, int startFile, int startRank, Piece piece, int maxDistance){
        List<Vector2> moves = new List<Vector2>();
        
        Board board = piece.board;
        foreach (Vector2 dir in directions)
        {
            for (int i = 1; i < maxDistance; i++)
            {
                int f = startFile + ((int)dir.x * i);
                int r = startRank + ((int)dir.y * i);
                if (f < 0 || f > 7 || r < 0 || r > 7) break;

                if (board.squares[f, r].isOccupied)
                {
                    if (board.squares[f, r].occupyingPiece.pieceColor != piece.pieceColor)
                        moves.Add(new Vector2(f, r));
                    break;
                }
                moves.Add(new Vector2(f, r));
            }
        }
        return moves;
    }
}
