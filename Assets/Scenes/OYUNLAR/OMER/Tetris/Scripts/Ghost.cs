using UnityEngine;
using UnityEngine.Tilemaps;

public class Ghost : MonoBehaviour
{
    [Header("Bağlantılar")]
    [SerializeField]
    private Board board;

    [SerializeField]
    private Piece activePiece;

    [SerializeField]
    private Tilemap ghostTilemap;

    private void LateUpdate()
    {
        // Oyun başlamadan Piece hücreleri henüz oluşturulmamış olabilir.
        if (board == null ||
            activePiece == null ||
            ghostTilemap == null ||
            activePiece.cells == null)
        {
            return;
        }

        // Ana menüde hayalet parça gösterme.
        if (board.isInMainMenu)
        {
            ghostTilemap.ClearAllTiles();
            return;
        }

        ghostTilemap.ClearAllTiles();

        // Oyun bittiyse yeni hayalet parça gösterme.
        if (board.isGameOver)
        {
            return;
        }

        UpdateGhost();
    }

    private void UpdateGhost()
    {
        // Aktif parça BoardTilemap üzerinde bulunduğu için
        // konum kontrolünden önce geçici olarak kaldır.
        board.Clear(activePiece);

        // Hesaplamaya aktif parçanın mevcut konumundan başla.
        Vector3Int ghostPosition = activePiece.position;

        // Bir alt konumu kontrol etmek için kullanılacak değer.
        Vector3Int nextPosition =
            ghostPosition + Vector3Int.down;

        // Parça aşağı hareket edebildiği sürece
        // hayalet konumunu bir hücre aşağı indir.
        while (board.IsValidPosition(
            activePiece,
            nextPosition))
        {
            ghostPosition = nextPosition;
            nextPosition += Vector3Int.down;
        }

        // Geçici olarak kaldırdığımız aktif parçayı geri çiz.
        board.Set(activePiece);

        // Hesaplanan en alt konuma hayalet parçayı çiz.
        DrawGhost(ghostPosition);
    }

    private void DrawGhost(Vector3Int ghostPosition)
    {
        for (int i = 0; i < activePiece.cells.Length; i++)
        {
            Vector2Int cell = activePiece.cells[i];

            Vector3Int tilePosition = ghostPosition;
            tilePosition.x += cell.x;
            tilePosition.y += cell.y;

            ghostTilemap.SetTile(tilePosition, activePiece.data.tile);
        }
    }
}
