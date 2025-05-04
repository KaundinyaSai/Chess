using UnityEngine;
using System.Collections.Generic;

public static class LegalMoves
{
    public static List<Vector2> GetLegalMovesAt(Piece piece, int file, int rank)
    {
        List<Vector2> pseudoLegalMoves;
        List<Vector2> legalMoves = new List<Vector2>();

        Board board = piece.board;

        switch (piece.pieceType)
        {
            case PieceType.Pawn:
                pseudoLegalMoves = GetPawnMoves(piece, file, rank);
                break;
            case PieceType.Rook:
                pseudoLegalMoves = GetRookMoves(piece, file, rank);
                break;
            case PieceType.Knight:
                pseudoLegalMoves = GetKnightMoves(piece, file, rank);
                break;
            case PieceType.Bishop:
                pseudoLegalMoves = GetBishopMoves(piece, file, rank);
                break;
            case PieceType.Queen:
                pseudoLegalMoves = GetQueenMoves(piece, file, rank);
                break;
            case PieceType.King:
                pseudoLegalMoves = GetKingMoves(piece, file, rank);
                break;
            default:
                return new List<Vector2>();
        }

        legalMoves = FilterPseudoLegalMoves(pseudoLegalMoves, piece);
        return legalMoves;
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

    static List<Vector2> GetKingMoves(Piece piece, int startFile, int startRank)
    {
        Vector2[] direction = new Vector2[]{
            new Vector2(1, 0), new Vector2(-1,0),
            new Vector2(0, 1), new Vector2(0, -1),
            new Vector2(1, 1), new Vector2(-1, 1),
            new Vector2(1, -1), new Vector2(-1, -1)
        };  

        List<Vector2> moves = GetMovesFromDirections(direction, startFile, startRank, piece, 2);

        // Castling logic (unchanged)
        Board board = piece.board;
        if (!piece.moved && piece.pieceColor == PieceColor.White)
        {
            Piece kingSideRook = board.squares[7, 0].occupyingPiece;
            Piece queenSideRook = board.squares[0, 0].occupyingPiece;

            if (kingSideRook != null && kingSideRook.pieceType == PieceType.Rook && kingSideRook.pieceColor == PieceColor.White && !kingSideRook.moved)
            {
                if (!board.squares[5, 0].isOccupied && !board.squares[6, 0].isOccupied)
                {
                    moves.Add(new Vector2(6, 0));
                }
            }
            if (queenSideRook != null && queenSideRook.pieceType == PieceType.Rook && queenSideRook.pieceColor == PieceColor.White && !queenSideRook.moved)
            {
                if (!board.squares[1, 0].isOccupied && !board.squares[2, 0].isOccupied && !board.squares[3, 0].isOccupied)
                {
                    moves.Add(new Vector2(2, 0));
                }
            }
        }
        else if (!piece.moved && piece.pieceColor == PieceColor.Black)
        {
            Piece kingSideRook = board.squares[7, 7].occupyingPiece;
            Piece queenSideRook = board.squares[0, 7].occupyingPiece;

            if (kingSideRook != null && kingSideRook.pieceType == PieceType.Rook && kingSideRook.pieceColor == PieceColor.Black && !kingSideRook.moved)
            {
                if (!board.squares[5, 7].isOccupied && !board.squares[6, 7].isOccupied)
                {
                    moves.Add(new Vector2(6, 7));
                }
            }
            if (queenSideRook != null && queenSideRook.pieceType == PieceType.Rook && queenSideRook.pieceColor == PieceColor.Black && !queenSideRook.moved)
            {
                if (!board.squares[1, 7].isOccupied && !board.squares[2, 7].isOccupied && !board.squares[3, 7].isOccupied)
                {
                    moves.Add(new Vector2(2, 7));
                }
            }
        }

        // Avoid modifying the list while iterating
        List<Vector2> movesToRemove = new List<Vector2>();
        foreach (Vector2 move in moves)
        {
            Square targetSquare = board.squares[(int)move.x, (int)move.y];
            bool isAttacked = piece.pieceColor == PieceColor.White ? targetSquare.isAttackedByBlack : targetSquare.isAttackedByWhite;
            if (isAttacked)
            {
                movesToRemove.Add(move);
            }
        }

        // Remove invalid moves
        foreach (Vector2 move in movesToRemove)
        {
            moves.Remove(move);
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

    static List<Vector2> FilterPseudoLegalMoves(List<Vector2> moves, Piece piece)
    {
        if (piece.pieceType == PieceType.King) return moves;

        King king = piece.ownKing;

        if (!king.isInCheck) return moves;

        Piece checkingPiece = king.checkingPieces[0];
        Vector2 kingPos = new Vector2(king.occupyingSquare.file, king.occupyingSquare.rank);
        Vector2 checkerPos = new Vector2(checkingPiece.occupyingSquare.file, checkingPiece.occupyingSquare.rank);

        if (king.checkingPieces.Count == 1)
        {
            List<Vector2> movesToKeep = new List<Vector2>();

            if (checkingPiece.IsSlidingPiece())
            {
                // Calculate the step direction from king to checking piece
                Vector2 dir = new Vector2(
                    Mathf.Sign(checkerPos.x - kingPos.x),
                    Mathf.Sign(checkerPos.y - kingPos.y)
                );

                // Collect blocking squares between king and checker
                List<Vector2> blockingSquares = new List<Vector2>();
                Vector2 current = kingPos + dir;
                while (current != checkerPos)
                {
                    blockingSquares.Add(current);
                    current += dir;
                }

                // Add capture square (the checker's position)
                blockingSquares.Add(checkerPos);

                // Keep only moves that block the check or capture the checker
                foreach (Vector2 move in moves)
                {
                    if (blockingSquares.Contains(move))
                    {
                        movesToKeep.Add(move);
                    }
                }
            }
            else
            {
                // Non-sliding piece: only valid move is to capture it
                foreach (Vector2 move in moves)
                {
                    if (move == checkerPos)
                    {
                        movesToKeep.Add(move);
                    }
                }
            }

            moves = movesToKeep;
        }
        else
        {
            // Double check: only legal move is for the king to move
            moves.Clear();
        }

        return moves;
    }

}
