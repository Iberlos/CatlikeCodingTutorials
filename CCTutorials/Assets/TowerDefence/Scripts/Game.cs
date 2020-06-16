using UnityEngine;
using UnityEngine.EventSystems;

public class Game : MonoBehaviour
{
    [SerializeField]
    private CameraBehavior cameraControl;
    [SerializeField]
    private Vector2Int boardSize = new Vector2Int(11, 11);
    [SerializeField]
    private GameBoard board = default;
    [SerializeField]
    private GameTileContentFactory tileContentFactory = default;

    private Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);
    private Vector3 lastMousePos;
    private ConstructionManager constructionManager;

    private void OnValidate()
    {
        if (boardSize.x < 2)
        {
            boardSize.x = 2;
        }
        if (boardSize.y < 2)
        {
            boardSize.y = 2;
        }
    }

    private void Awake()
    {
        board.Initialize(boardSize, tileContentFactory);
        board.ShowGrid = true;
    }

    private void Update()
    {
        //Input
        //Mouse
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButton(0))
                HandleHold();
        }
        if (Input.GetMouseButtonUp(0))
            HandleRelease();

        if (Input.GetMouseButtonDown(1))
            HandleAlternativeTouch();
        else if (Input.GetMouseButton(1))
            HandleAlternativeHold();

        float scrollDelta = Input.mouseScrollDelta.y;
        if (scrollDelta != 0)
            HandleScroll(scrollDelta);

        //Keyboard
        if(Input.GetKeyDown(KeyCode.V))
        {
            board.ShowPaths = !board.ShowPaths;
        }

        if(Input.GetKeyDown(KeyCode.G))
        {
            board.ShowGrid = !board.ShowGrid;
        }
    }

    void HandleAlternativeTouch()
    {
        lastMousePos = Input.mousePosition;
        cameraControl.InitiatePan();
    }

    void HandleAlternativeHold()
    {
        Vector3 mouseoffset = Input.mousePosition - lastMousePos;
        cameraControl.ApplyPan(mouseoffset);
    }

    void HandleHold()
    {
        GameTile tile = board.GetTile(TouchRay);
        if(tile != null)
        {
            if(constructionManager == null)
            {
                constructionManager = GetComponent<ConstructionManager>();
            }
            switch (constructionManager.ConstructionType)
            {
                case GameTileContentType.Invalid: case GameTileContentType.Last:
                    {
                        break;
                    }
                case GameTileContentType.Empty:
                    {
                        board.Deconstruct(tile);
                        break;
                    }
                default:
                    {
                        if (constructionManager.LastConstructedTile != tile)
                        {
                            constructionManager.LastConstructedTile = tile;
                            board.Construct(tile, constructionManager.ConstructionType);
                        }
                        break;
                    }
            }


        }
    }

    void HandleRelease()
    {
        if(constructionManager != null)
            constructionManager.ClearConsnstruction();
    }

    void HandleScroll(float scrollDelta)
    {
        cameraControl.ApplyZoom(scrollDelta);
    }
}
