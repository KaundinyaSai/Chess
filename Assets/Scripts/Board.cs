using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
public class Board : MonoBehaviour
{
    public GameObject LightSquarePrefab;
    public GameObject DarkSquarePrefab;

    public GameObject[] pieces = new GameObject[12]; // 0: wp 1: wb 2: wn 3: wr 4: wq 5: wk 6: bp 7: bb 8: bn 9: br 10: bq 11: bk

    public string startingFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    // Arrays for representing the board and pieces
    [HideInInspector] public Square[,] squares = new Square[8, 8];

    [HideInInspector] public Piece lastMovedPiece; 
    
    [HideInInspector] public int lastMovedPieceStartFile = -1; 
    [HideInInspector] public int lastMovedPieceStartRank = -1;
    [HideInInspector] public int lastMovedPieceEndFile = -1;   
    [HideInInspector] public int lastMovedPieceEndRank = -1;   

    [HideInInspector] public Piece lastClickedPiece; 


    public int turn = 0; // even = white, odd = black

    public bool flipped = false; 

    public AudioSource moveSound;
    public AudioSource captureSound; 
    
    public List<Piece> piecesOnBoard = new List<Piece>(); // List of all pieces on the board

    Camera mainCamera;

    public King whiteKing, blackKing;
    void Start()
    {
        InstantiateBoard();
        turn = 0; // White starts first

        transform.rotation = flipped ? Quaternion.Euler(0, 0, 180) : Quaternion.identity; // Rotate the board if flipped
        mainCamera = Camera.main;
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            int file = Mathf.RoundToInt(worldPos.x);
            int rank = Mathf.RoundToInt(worldPos.y);

            if (file < 0 || file > 7 || rank < 0 || rank > 7) return;

            Square clickedSquare = squares[file, rank];
            if (clickedSquare != null &&
                (clickedSquare.isHighlightedNoramal || clickedSquare.isHighlightedOccupied))
            {
                clickedSquare.OnClick();
            }
        }
    }
    void InstantiateBoard(){
        for (int j = 0; j < 8; j++) // ranks (rows)
        {
            for (int i = 0; i < 8; i++) // files (columns)
            {
                GameObject square = (i + j) % 2 != 0 
                    ? Instantiate(LightSquarePrefab, new Vector3(i, j, 0), Quaternion.identity) 
                    : Instantiate(DarkSquarePrefab, new Vector3(i, j, 0), Quaternion.identity);

                square.transform.parent = transform;

                Square squareScript = square.GetComponent<Square>();
                
                squares[i, j] = squareScript; // Store the square in the array
                squareScript.file = i; // Set the file (x-coordinate) of the square
                squareScript.rank = j; // Set the rank (y-coordinate) of the square
            }
        }

        LoadPositionFromFen(startingFen);
    }


    void LoadPositionFromFen(string fen){
        //Example FEN: rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1
        
        string piecesPlacement = fen.Split(' ')[0]; // Get the piece placement part of the FEN string
        string[] ranks = piecesPlacement.Split("/"); // Split the FEN string into ranks (rows)
        for (int i = 0; i < 8; i++)
        {
            string rank = ranks[i];
            int file = 0;
            foreach (char c in rank)
            {
                if (char.IsDigit(c))
                {
                    file += int.Parse(c.ToString());
                }
                else
                {
                    GameObject piecePrefab = GetPiecePrefab(c);
                    if (piecePrefab == null) continue;
                    
                    Quaternion rotation = flipped ? Quaternion.Euler(0, 0, 180) : Quaternion.identity; // Rotate the piece if the board is flipped

                    GameObject piece = Instantiate(piecePrefab, new Vector3(file, 7 - i, 0), rotation);
                    piece.transform.parent = transform;
                    
                    Piece pieceScript = piece.GetComponent<Piece>();
                    piecesOnBoard.Add(pieceScript);
                    file++;

                    if(c == 'K'){
                        whiteKing = pieceScript as King; // Cast to King type
                    }else if(c == 'k'){
                        blackKing = pieceScript as King; // Cast to King type
                    }
                }
            }
        }

        foreach(Piece piece in piecesOnBoard){
            King ownKing = piece.pieceColor == PieceColor.White ? whiteKing : blackKing;
            piece.ownKing = ownKing; // Set the king reference for each piece
        }
    }

    GameObject GetPiecePrefab(char pieceChar){
        switch (pieceChar)
        {
            case 'P': return pieces[0]; // White Pawn
            case 'p': return pieces[6]; // Black Pawn
            case 'B': return pieces[1]; // White Bishop
            case 'b': return pieces[7]; // Black Bishop
            case 'N': return pieces[2]; // White Knight
            case 'n': return pieces[8]; // Black Knight
            case 'R': return pieces[3]; // White Rook
            case 'r': return pieces[9]; // Black Rook
            case 'Q': return pieces[4]; // White Queen
            case 'q': return pieces[10]; // Black Queen
            case 'K': return pieces[5]; // White King
            case 'k': return pieces[11]; // Black King
            default: return null!;
        }
    }

    public void MarkAttacks(Piece piece){
        List<Vector2> attackedSquares = LegalMoves.GetLegalMovesAt(piece, 
        (int)piece.transform.position.x, (int)piece.transform.position.y); 

        foreach (Vector2 square in attackedSquares)
        {
            Square attackedSquare = squares[(int)square.x, (int)square.y];
            if (piece.pieceColor == PieceColor.White)
            {
                attackedSquare.isAttackedByWhite = true; 
            }
            else
            {
                attackedSquare.isAttackedByBlack = true;
            }
        }
    }

    public void UnmarkAttacks(Piece piece)
    {
        List<Vector2> previouslyAttackedSquares = LegalMoves.GetLegalMovesAt(piece, 
            (int)piece.originalSquarePosition.x, 
            (int)piece.originalSquarePosition.y);

        foreach (Vector2 square in previouslyAttackedSquares)
        {
            Square attackedSquare = squares[(int)square.x, (int)square.y];
            if (piece.pieceColor == PieceColor.White)
            {
                attackedSquare.isAttackedByWhite = false;
            }
            else
            {
                attackedSquare.isAttackedByBlack = false;
            }
        }
    }

    public void RemarkAttacks()
    {
        // Clear all attack markers
        foreach (Square square in squares)
        {
            square.isAttackedByWhite = false;
            square.isAttackedByBlack = false;
        }

        // Recalculate attack markers for all pieces
        foreach (Piece piece in piecesOnBoard)
        {
            MarkAttacks(piece);
        }
    }

    public void AfterTurn(Piece Piece){
        foreach(Piece piece in piecesOnBoard){
            piece.GetLegalMoves(new Vector2(piece.transform.position.x, piece.transform.position.y)); // Update legal moves for all pieces
        }

        lastMovedPiece = Piece; // Set the last moved piece
        UnmarkAttacks(Piece); // Unmark attacks for the piece that just moved
        MarkAttacks(Piece);
        RemarkAttacks();
        UpdatePinnedPieces();

        blackKing.CheckForChecks(); // Check for checks after the turn
        whiteKing.CheckForChecks(); // Check for checks after the turn

        blackKing.CheckForCheckMate(); // Check for checkmate after the turn
        whiteKing.CheckForCheckMate(); // Check for checkmate after the turn
    }

    public King GetKing(PieceColor pieceColor){
        if(pieceColor == PieceColor.White){
            return whiteKing; // Return the white king
        }else{
            return blackKing; // Return the black king
        }
    }

    public void UpdatePinnedPieces()
    {
        // First, clear all current pin info
        foreach (Piece piece in piecesOnBoard)
        {
            piece.isPinned = false;
            piece.pinnedDirection = Vector2Int.zero;
        }
        // Check pins for both sides
        CheckPins(whiteKing);
        CheckPins(blackKing);
    }
    public bool IsInBounds(Vector2 pos){
        return pos.x >= 0 && pos.x < 8 && pos.y >= 0 && pos.y < 8; // Check if the position is within the board bounds
    }

    private void CheckPins(Piece king)
    {
        Vector2[] directions = {
            new Vector2(1,0), new Vector2(-1,0), // horizontal
            new Vector2(0,1), new Vector2(0,-1), // vertical
            new Vector2(1,1), new Vector2(1,-1), new Vector2(-1,1), new Vector2(-1,-1) // diagonals
        };

        foreach (Vector2 dir in directions)
        {
            bool blockerFound = false;
            Piece blocker = null;

            Vector2 pos = new Vector2(king.occupyingSquare.file, king.occupyingSquare.rank) + dir;
            

            while (IsInBounds(pos))
            {
                Square square = squares[(int)pos.x, (int)pos.y];
                Piece p = square.occupyingPiece; // Get the piece at the current position

                if (p != null && p.pieceType != PieceType.King)
                {
                    if (p.pieceColor == king.pieceColor)
                    {
                        if (!blockerFound)
                        {
                            blockerFound = true;
                            blocker = p;
                        }
                        else
                        {
                            break; // second friendly → no pin
                        }
                    }
                    else
                    {
                        // enemy piece
                        if (p.IsSlidingPiece())
                        {
                            if (blockerFound)
                            {
                                blocker.isPinned = true;
                                blocker.pinnedDirection = dir;
                            }
                        }
                        break;
                    }
                }

                pos += dir;
            }
        }
    }
}
