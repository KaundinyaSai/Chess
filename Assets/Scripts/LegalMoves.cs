using UnityEngine;
using System.Collections.Generic;

public static class LegalMoves
{
    public static List<Vector2> GetLegalMovesAt(Piece piece, int file, int rank)
    {
        List<Vector2> pseudoLegalMoves;
        List<Vector2> legalMoves;
        
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
        int startingRank = (piece.pieceColor == PieceColor.White) ? 1 : 6; // Starting rank for pawns
        if (oneStepRank >= 0 && oneStepRank <= 7 && !board.squares[startFile, oneStepRank].isOccupied)
        {
            moves.Add(new Vector2(startFile, oneStepRank));

            // Forward two squares (only if first move)
            int twoStepRank = startRank + 2 * forwardDir;
            if (piece.moved == false && 
                twoStepRank >= 0 && twoStepRank <= 7 && 
                !board.squares[startFile, twoStepRank].isOccupied && piece.occupyingSquare.rank == startingRank)
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
                        Pawn pawn = piece.GetComponent<Pawn>();
                        if(pawn != null)
                        {
                            pawn.enPassantSquare = new Vector2(targetFile, targetRank);
                        }
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
            new Vector2(1, 0), new Vector2(-1, 0),
            new Vector2(0, 1), new Vector2(0, -1),
            new Vector2(1, 1), new Vector2(-1, 1),
            new Vector2(1, -1), new Vector2(-1, -1)
        };

        List<Vector2> moves = GetMovesFromDirections(direction, startFile, startRank, piece, 2);

        King king = piece.ownKing;
        if (king.isInCheck)
        {
            foreach (Piece checkPiece in king.checkingPieces)
            {
                if (checkPiece.IsSlidingPiece())
                {
                    Vector2 dir = new Vector2(
                        checkPiece.occupyingSquare.file == king.occupyingSquare.file ? 0 : Mathf.Sign(checkPiece.occupyingSquare.file - king.occupyingSquare.file),
                        checkPiece.occupyingSquare.rank == king.occupyingSquare.rank ? 0 : Mathf.Sign(checkPiece.occupyingSquare.rank - king.occupyingSquare.rank)
                    );
                    
                    Vector2 targetMove = king.transform.position - (Vector3)dir;
                    
                    for (int i = moves.Count - 1; i >= 0; i--)
                    {
                        if (Mathf.Approximately(moves[i].x, targetMove.x) && Mathf.Approximately(moves[i].y, targetMove.y))
                        {
                            moves.RemoveAt(i);
                        }
                    }
                }
            }
        }

        // Castling logic
        Board board = piece.board;
        if(piece.ownKing.isInCheck) return moves; // No castling if king is in check
        if (!piece.moved)
        {
            int rank = (piece.pieceColor == PieceColor.White) ? 0 : 7;
            
            // White and Black castling
            Piece kingSideRook = board.squares[7, rank].occupyingPiece;
            Piece queenSideRook = board.squares[0, rank].occupyingPiece;

            // King-side castling
            if (kingSideRook != null && kingSideRook.pieceType == PieceType.Rook && kingSideRook.pieceColor == piece.pieceColor && !kingSideRook.moved)
            {
                if (!board.squares[5, rank].isOccupied && !board.squares[6, rank].isOccupied)
                {
                    // Check if the squares the king moves through are attacked
                    bool isKingSideSafe = piece.pieceColor == PieceColor.White ? 
                    !board.squares[5, rank].isAttackedByBlack && !board.squares[6, rank].isAttackedByBlack : 
                    !board.squares[5, rank].isAttackedByWhite && !board.squares[6, rank].isAttackedByWhite;
                    if (isKingSideSafe)
                    {
                        moves.Add(new Vector2(6, rank));
                    }
                }
            }

            // Queen-side castling
            if (queenSideRook != null && queenSideRook.pieceType == PieceType.Rook && queenSideRook.pieceColor == piece.pieceColor && !queenSideRook.moved)
            {
                if (!board.squares[1, rank].isOccupied && !board.squares[2, rank].isOccupied && !board.squares[3, rank].isOccupied)
                {
                    // Check if the squares the king moves through are attacked
                    bool isQueenSideSafe = piece.pieceColor == PieceColor.White ? 
                    !board.squares[1, rank].isAttackedByBlack && !board.squares[2, rank].isAttackedByBlack && !board.squares[3, rank].isAttackedByBlack :
                    !board.squares[1, rank].isAttackedByWhite && !board.squares[2, rank].isAttackedByWhite && !board.squares[3, rank].isAttackedByWhite;
                    if (isQueenSideSafe)
                    {
                        moves.Add(new Vector2(2, rank));
                    }
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
                    else if(board.squares[f, r].occupyingPiece.pieceColor == piece.pieceColor){
                        // For protecting pieces
                        if(piece.pieceColor == PieceColor.White){
                            board.squares[f, r].isAttackedByWhite = true;
                        }else{
                            board.squares[f, r].isAttackedByBlack = true;
                        }
                        break;
                    }
                    break;
                }
                moves.Add(new Vector2(f, r));
            }
        }
        return moves;
    }

    static List<Vector2> FilterPseudoLegalMoves(List<Vector2> moves, Piece piece)
    {
        if (piece.pieceType == PieceType.King)
        {
            Board board = piece.board;
            for (int i = moves.Count - 1; i >= 0; i--)
            {
                Vector2 move = moves[i];
                Square targetSquare = board.squares[(int)move.x, (int)move.y];

                // Check if the square is attacked by the opponent
                bool isAttacked = piece.pieceColor == PieceColor.White ? targetSquare.isAttackedByBlack : targetSquare.isAttackedByWhite;
                if (isAttacked)
                {
                    moves.RemoveAt(i);
                    continue;
                }

                King opponentKing = piece.pieceColor == PieceColor.White ? board.blackKing : board.whiteKing;
                if (opponentKing != null && opponentKing.occupyingSquare != null)
                {
                    int dx = Mathf.Abs((int)move.x - opponentKing.occupyingSquare.file);
                    int dy = Mathf.Abs((int)move.y - opponentKing.occupyingSquare.rank);
                    if (dx <= 1 && dy <= 1)
                    {
                        moves.RemoveAt(i);
                    }
                }
            }

            return moves;
        }
        
        King king = piece.ownKing;

        if (!king.isInCheck){
            if(piece.isPinned){
                // If the piece is pinned, it can only move along the line of attack
                List<Vector2> movesToKeep = new List<Vector2>();
                foreach (Vector2 move in moves)
                {
                    if (move == new Vector2(piece.occupyingSquare.file, piece.occupyingSquare.rank) + piece.pinnedDirection)
                    {
                        movesToKeep.Add(move);
                    }
                }

            return movesToKeep;
            }else{
                return moves; // No filtering needed if not pinned
            }
        }

        Piece checkingPiece = king.checkingPieces[0];
        Vector2 kingPos = new Vector2(king.occupyingSquare.file, king.occupyingSquare.rank);
        Vector2 checkerPos = new Vector2(checkingPiece.occupyingSquare.file, checkingPiece.occupyingSquare.rank);

        if (king.checkingPieces.Count == 1)
        {
            List<Vector2> movesToKeep = new List<Vector2>();

            if (checkingPiece.IsSlidingPiece())
            {   // Sliding piece: can block or capture
                
                // Calculate the step direction from king to checking piece
                Vector2 dir = new Vector2(
                checkerPos.x == kingPos.x ? 0 : Mathf.Sign(checkerPos.x - kingPos.x),
                checkerPos.y == kingPos.y ? 0 : Mathf.Sign(checkerPos.y - kingPos.y)
                );

                // Collect blocking squares between king and checker
                List<Vector2> blockingSquares = new List<Vector2>();
                Vector2 current = kingPos + dir;
                
                if(dir != Vector2.zero) 
                {
                    while (current != checkerPos)
                    {
                        blockingSquares.Add(current);
                        current += dir;
                    }
                }
                
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
