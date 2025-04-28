#nullable enable
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour
{
    public GameObject LightSquarePrefab = null!;
    public GameObject DarkSquarePrefab = null!;

    public GameObject[] pieces = new GameObject[12]; // 0: wp 1: wb 2: wn 3: wr 4: wq 5: wk 6: bp 7: bb 8: bn 9: br 10: bq 11: bk

    public string startingFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    // Arrays for representing the board and pieces
    Piece?[,] board = new Piece[8, 8]; // 2D array for the board. Null for empty squares, Piece for occupied squares
    [HideInInspector] public Square[,] squares = new Square[8, 8];

    public int turn = 0; // even = white, odd = black

    public bool flipped = false; // true if the board is flipped
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InstantiateBoard();
        turn = 0; // White starts first

        transform.rotation = flipped ? Quaternion.Euler(0, 0, 180) : Quaternion.identity; // Rotate the board if flipped
    }

    // Update is called once per frame
    void Update()
    {
        
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
                board[i, j] = null; // Initialize the board with null values
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
                    GameObject? piecePrefab = GetPiecePrefab(c);
                    if (piecePrefab == null) continue;
                    
                    Quaternion rotation = flipped ? Quaternion.Euler(0, 0, 180) : Quaternion.identity; // Rotate the piece if the board is flipped

                    GameObject piece = Instantiate(piecePrefab, new Vector3(file, 7 - i, 0), rotation);
                    piece.transform.parent = transform;
                    
                    Piece pieceScript = piece.GetComponent<Piece>();
                    board[file, 7 - i] = pieceScript;

                    file++;
                }
            }
        }
    }

    GameObject? GetPiecePrefab(char pieceChar){
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
}
