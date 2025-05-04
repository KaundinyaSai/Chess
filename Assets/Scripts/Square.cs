using UnityEngine;

public class Square : MonoBehaviour
{
    public bool isOccupied => occupyingPiece != null;
    [HideInInspector] public Piece occupyingPiece; 

    [HideInInspector] public bool isHighlightedNoramal = false; // Flag to indicate if the square is highlighted
    [HideInInspector] public bool isHighlightedOccupied = false; // Flag to indicate if the square is highlighted and occupied

    GameObject noramlHighlightObject; 
    GameObject occupiedHighlightObject;

    Board board;

    public bool isAttackedByWhite = false;
    public bool isAttackedByBlack = false;

    public int file, rank; 
    public void SetOccupyingPiece(Piece piece)
    {
        occupyingPiece = piece;
    }

    public void ClearOccupyingPiece()
    {
        occupyingPiece = null;
    }

    void Start(){
        noramlHighlightObject = transform.GetChild(0).gameObject;
        occupiedHighlightObject = transform.GetChild(1).gameObject;
        noramlHighlightObject.SetActive(false); // Hide the highlight object at the start
        board = GameObject.Find("Manager").GetComponent<Board>();
    }

    void Update(){
        if(isHighlightedNoramal){
            noramlHighlightObject.SetActive(true); // Show the highlight object
        }else{
            noramlHighlightObject.SetActive(false); // Hide the highlight object
        }

        if(isHighlightedOccupied){
            occupiedHighlightObject.SetActive(true); // Show the highlight object
        }else{
            occupiedHighlightObject.SetActive(false); // Hide the highlight object
        }

    }


    public void OnClick()
    {
        Piece lastPiece = board.lastClickedPiece;

        if (lastPiece == null)
            return;

        // Attempt move to this square
        lastPiece.FinalizeMove(transform.position); // Pass the square to the piece for finalizing the move
    }

}

