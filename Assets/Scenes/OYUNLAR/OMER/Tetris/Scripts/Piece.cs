using UnityEngine;

public class Piece : MonoBehaviour
{
#region Public State

    public Board board { get; private set; }
    public Vector3Int position { get; private set; }
    public TetrominoData data { get; private set; }
    public Vector2Int[] cells { get; private set; }

#endregion

#region Settings and Runtime State

    [Header("Düşme Ayarları")]
    [SerializeField]
    private float stepDelay = 1f;

    private float nextStepTime;

    [Header("Kilitlenme Ayarları")]
    [SerializeField]
    private float lockDelay = 0.5f;

    private float lockTimer;
    private bool isGrounded;

    [SerializeField]
    private int maximumLockResets = 15;

    private int lockResetCount;

#endregion

#region Initialization and Update

    public void Initialize(
        Board board,
        Vector3Int position,
        TetrominoData data)
    {
        this.board = board;
        this.position = position;
        this.data = data;

        // Her aktif parça için ayrı bir hücre dizisi oluştur.
        // Böylece döndürme sırasında ana parça verileri değişmez.
        cells = new Vector2Int[data.cells.Length];

        for (int i = 0; i < data.cells.Length; i++)
        {
            cells[i] = data.cells[i];
        }

        // İlk otomatik düşmenin zamanını belirle.
        nextStepTime = Time.time + stepDelay;

        isGrounded = false;
        lockTimer = 0f;
        lockResetCount = 0;
    }

    private void Update()
    {
        // Parça zemindeyse kilit süresini artır.
        if (isGrounded)
        {
            lockTimer += Time.deltaTime;

            // Bekleme süresi tamamlandıysa parçayı sabitle.
            if (lockTimer >= lockDelay)
            {
                Lock();
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) ||
            Input.GetKeyDown(KeyCode.A))
        {
            MoveLeft();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) ||
            Input.GetKeyDown(KeyCode.D))
        {
            MoveRight();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) ||
            Input.GetKeyDown(KeyCode.S))
        {
            SoftDrop();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            RotateClockwise();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            RotateCounterClockwise();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
            return;
        }

        if (Time.time >= nextStepTime)
        {
            Step();
        }
    }

#endregion

#region Player Controls

    public void MoveLeft()
    {
        board.Clear(this);

        bool moved = Move(Vector2Int.left);

        // Hareket başarılıysa zeminde bulunma durumunu yeniden kontrol et.
        if (moved)
        {
            UpdateGroundedState(true);
        }

        board.Set(this);
    }

    public void MoveRight()
    {
        board.Clear(this);

        bool moved = Move(Vector2Int.right);

        if (moved)
        {
            UpdateGroundedState(true);
        }

        board.Set(this);
    }

    public void SoftDrop()
    {
        board.Clear(this);

        if (Move(Vector2Int.down))
        {
            // Başarılı aşağı hareket için 1 puan ver.
            board.AddDropScore(1);

            // Aşağı hareket edildiği için zemin durumunu yeniden hesapla.
            UpdateGroundedState(false);
        }
        else
        {
            // Aşağı hareket edilemiyorsa kilit sayacını başlat.
            UpdateGroundedState(false);
        }

        board.Set(this);

        nextStepTime = Time.time + stepDelay;
    }


    public void HardDrop()
    {
        board.Clear(this);

        // Parçanın toplam kaç hücre indiğini tutar.
        int droppedCellCount = 0;

        // Aşağı hareket mümkün olduğu sürece devam et.
        while (Move(Vector2Int.down))
        {
            droppedCellCount++;
        }

        // Anında düşürmede geçilen her hücre için 2 puan ekle.
        board.AddDropScore(droppedCellCount * 2);

        // Parçayı ulaştığı en alt konuma sabitle.
        Lock();
    }


    public void RotateClockwise()
    {
        board.Clear(this);

        // Dönüşün başarılı olup olmadığını kaydet.
        bool rotated = Rotate(1);

        // Yalnızca başarılı dönüşte kilit süresini yenilemeyi dene.
        if (rotated)
        {
            UpdateGroundedState(true);
        }

        board.Set(this);
    }


    public void RotateCounterClockwise()
    {
        board.Clear(this);

        bool rotated = Rotate(-1);

        if (rotated)
        {
            UpdateGroundedState(true);
        }

        board.Set(this);
    }

#endregion

#region Movement and Rotation

    private void Step()
    {
        nextStepTime = Time.time + stepDelay;

        board.Clear(this);

        if (Move(Vector2Int.down))
        {
            // Parça aşağı indiği için zemin durumunu kontrol et.
            UpdateGroundedState(false);
        }
        else
        {
            // Aşağı inemiyorsa zemine ulaşmıştır.
            UpdateGroundedState(false);
        }

        board.Set(this);
    }

    public void SetStepDelay(float newStepDelay)
    {
        // Değerin sıfır veya aşırı küçük olmasını engelle.
        stepDelay = Mathf.Max(0.1f, newStepDelay);
    }


    public bool Move(Vector2Int translation)
    {
        // Hareket başarısız olursa geri dönebilmek için
        // mevcut konumu sakla.
        Vector3Int originalPosition = position;

        // İstenen yönü parçanın konumuna ekle.
        position += new Vector3Int(
            translation.x,
            translation.y,
            0
        );

        // Yeni konum geçerli değilse eski konuma dön.
        if (!board.IsValidPosition(this, position))
        {
            position = originalPosition;
            return false;
        }

        return true;
    }


    private bool Rotate(int direction)
    {
        // O parçası döndüğünde görüntüsü değişmediği için işlem yapma.
        if (data.tetromino == Tetromino.O)
        {
            return false;
        }

        // Dönüş başarısız olursa geri yüklemek için hücreleri sakla.
        Vector2Int[] originalCells =
            new Vector2Int[cells.Length];

        for (int i = 0; i < cells.Length; i++)
        {
            originalCells[i] = cells[i];
        }

        // Bütün hücreleri parça merkezi etrafında döndür.
        for (int i = 0; i < cells.Length; i++)
        {
            int originalX = cells[i].x;
            int originalY = cells[i].y;

            if (direction == 1)
            {
                cells[i] = new Vector2Int(
                    originalY,
                    -originalX
                );
            }
            else
            {
                cells[i] = new Vector2Int(
                    -originalY,
                    originalX
                );
            }
        }

        // Parça bulunduğu konumda dönebiliyorsa dönüş başarılıdır.
        if (board.IsValidPosition(this, position))
        {
            return true;
        }

        // Wall-kick başarılıysa dönüş tamamlanmıştır.
        if (TryWallKick())
        {
            return true;
        }

        // Dönüş başarısızsa eski hücreleri geri yükle.
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = originalCells[i];
        }

        return false;
    }


    private bool TryWallKick()
    {
        // Önce bir hücre sola, sonra sağa;
        // ardından iki hücre sola ve sağa bakılır.
        Vector2Int[] testPositions =
        {
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.left * 2,
            Vector2Int.right * 2,
            Vector2Int.up
        };

        for (int i = 0; i < testPositions.Length; i++)
        {
            Vector3Int testPosition = position;

            testPosition.x += testPositions[i].x;
            testPosition.y += testPositions[i].y;

            // Denenen konum geçerliyse parçayı oraya taşı.
            if (board.IsValidPosition(this, testPosition))
            {
                position = testPosition;
                return true;
            }
        }

        // Denenen hiçbir konum geçerli değil.
        return false;
    }


    private void Lock()
    {
        // Parçayı bulunduğu konumda kalıcı hâle getir.
        board.Set(this);

        // Parçanın yerleşmesiyle tamamlanan satırları kontrol et.
        board.CheckAndClearLines();

        // Yeni aktif parçayı oluştur.
        board.SpawnPiece();
    }


    private void UpdateGroundedState(bool requestLockReset)
    {
        Vector3Int positionBelow =
            position + Vector3Int.down;

        bool canMoveDown =
            board.IsValidPosition(this, positionBelow);

        // Hareket veya dönüş sonucunda parça zeminden ayrıldıysa
        // bunu da bir kilit yenilemesi olarak say.
        if (canMoveDown)
        {
            if (isGrounded && requestLockReset)
            {
                // Yalnızca yenileme hakkı kaldıysa süreyi sıfırla.
                if (lockResetCount < maximumLockResets)
                {
                    lockTimer = 0f;
                    lockResetCount++;
                }
            }

            // Parça artık zeminde değil.
            // Yenileme hakkı bitmişse lockTimer değerini koruyoruz.
            isGrounded = false;
            return;
        }

        // Parça zemine ilk kez ulaştı.
        if (!isGrounded)
        {
            isGrounded = true;

            // Yenileme hakkı hâlâ varsa normal şekilde
            // yeni kilit süresi başlat.
            if (lockResetCount < maximumLockResets)
            {
                lockTimer = 0f;
            }

            return;
        }

        // Parça zemindeyken başarılı bir hareket veya dönüş yapıldı.
        if (requestLockReset &&
            lockResetCount < maximumLockResets)
        {
            lockTimer = 0f;
            lockResetCount++;
        }
    }

#endregion
}
