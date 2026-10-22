using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
#region Inspector Fields

    [Header("Sahne Bağlantıları")]
    [SerializeField]
    private Tilemap tilemap;

    [SerializeField]
    private Piece activePiece;

    [SerializeField]
    private GameObject gameOverPanel;

    [SerializeField]
    private GameObject controls;

    [Header("Parça Ayarları")]
    [SerializeField]
    private TetrominoData[] tetrominoes;

    [Header("Oyun Alanı Ayarları")]
    [SerializeField]
    private int boardWidth = 10;

    [SerializeField]
    private int boardHeight = 20;


    [Header("Puan Arayüzü")]
    [SerializeField]
    private TMP_Text scoreText;

    [SerializeField]
    private TMP_Text linesText;

    [SerializeField]
    private TMP_Text levelText;

    [Header("Hız Ayarları")]
    [SerializeField]
    private float startingStepDelay = 1f;

    [SerializeField]
    private float speedMultiplier = 0.85f;

    [SerializeField]
    private float minimumStepDelay = 0.1f;

    [Header("Sıradaki Parça Arayüzü")]
    [SerializeField]
    private Image nextPieceImage;

    [Header("Duraklatma Sistemi")]
    [SerializeField]
    private GameObject pausePanel;

    [Header("Game Over Puan Arayüzü")]
    [SerializeField]
    private TMP_Text finalScoreText;

    [SerializeField]
    private TMP_Text highScoreText;

    [Header("Oyun Bildirimleri")]
    [SerializeField]
    private TMP_Text notificationText;

    [Header("Ana Menü Sistemi")]
    [SerializeField]
    private GameObject mainMenuPanel;

    [SerializeField]
    private GameObject statsGroup;

    [SerializeField]
    private GameObject nextPanel;

    [SerializeField]
    private GameObject pauseButton;

#endregion

#region Public State

    public bool isInMainMenu { get; private set; }
    public bool isPaused { get; private set; }
    public bool isGameOver { get; private set; }

#endregion

#region Runtime State

    private const string HighScoreKey = "TetrisHighScore";

    private Coroutine notificationCoroutine;
    private int highScore;
    private TetrominoData nextTetrominoData;
    private bool hasNextTetromino;
    private int score;
    private int totalLines;
    private int level;
    private readonly List<TetrominoData> tetrominoBag = new List<TetrominoData>();
    private readonly Vector3Int spawnPosition = new Vector3Int(4, 18, 0);

#endregion

#region Board Properties

    public RectInt Bounds
    {
        get
        {
            return new RectInt(
                0,
                0,
                boardWidth,
                boardHeight
            );
        }
    }

#endregion

#region Unity Lifecycle

    private void Start()
    {
        isGameOver = false;
        
        score = 0;
        totalLines = 0;
        level = 1;

        UpdateScoreUI();

        // Önceki sahneden kalabilecek zaman ayarını normale döndür.
        Time.timeScale = 1f;

        // Daha önce kaydedilmiş yüksek puanı cihazdan oku.
        // Kayıt bulunmuyorsa başlangıç değeri 0 olur.
        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);


        isPaused = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // Game Over panelini oyun başlangıcında gizle.
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Mobil kontrolleri oyun başlangıcında aç.
        if (controls != null)
        {
            controls.SetActive(true);
        }

        // Bütün Tetromino hücrelerini hazırla.
        for (int i = 0; i < tetrominoes.Length; i++)
        {
            tetrominoes[i].Initialize();
        }

        // Oyun başlangıcında bildirim yazısını gizle.
        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(false);
        }
        OpenMainMenu();
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
        {
            return;
        }

        if (isInMainMenu || isGameOver)
        {
            return;
        }

        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && !isGameOver && !isPaused)
        {
            PauseGame();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && !isGameOver && !isPaused)
        {
            PauseGame();
        }
    }

#endregion

#region Menu and Pause

    public void StartGame()
    {
        // Oyun zaten başladıysa tekrar başlatma.
        if (!isInMainMenu)
        {
            return;
        }

        isInMainMenu = false;
        isPaused = false;

        // Menü yazılarını kapat.
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        // Skor, satır ve level yazılarını göster.
        if (statsGroup != null)
        {
            statsGroup.SetActive(true);
        }

        // NEXT kutusunu göster.
        if (nextPanel != null)
        {
            nextPanel.SetActive(true);
        }

        // Pause butonunu göster.
        if (pauseButton != null)
        {
            pauseButton.SetActive(true);
        }

        // Oyun zamanını başlat.
        Time.timeScale = 1f;

        // İlk aktif parçayı ve sıradaki parçayı şimdi oluştur.
        SpawnPiece();

        // Piece Update fonksiyonunu çalıştır.
        activePiece.enabled = true;
    }

    public void PauseGame()
    {
        if (isGameOver || isPaused || isInMainMenu)
        {
            return;
        }

        isPaused = true;

        // Oyun zamanını durdur.
        Time.timeScale = 0f;

        // Piece klavye kontrolünü durdur.
        activePiece.enabled = false;

        // Pause butonunu gizle.
        if (pauseButton != null)
        {
            pauseButton.SetActive(false);
        }

        // Atari kontrollerini görünür bırak.
        // MobileControlButton hangi tuşların çalışacağını belirleyecek.
        if (controls != null)
        {
            controls.SetActive(true);
        }

        // Pause panelini göster.
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    public void ResumeGame()
    {
        if (!isPaused || isGameOver)
        {
            return;
        }

        isPaused = false;

        // Pause panelini gizle.
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // Pause butonunu yeniden göster.
        if (pauseButton != null)
        {
            pauseButton.SetActive(true);
        }

        // Kontroller görünür kalmaya devam etsin.
        if (controls != null)
        {
            controls.SetActive(true);
        }

        // Piece kontrolünü tekrar aç.
        activePiece.enabled = true;

        // Oyun zamanını normale döndür.
        Time.timeScale = 1f;
    }

#endregion

#region Piece Generation

    // Karıştırılmış torbadan sıradaki parçayı verir.
    private TetrominoData GetTetrominoFromBag()
    {
        // Torbada parça kalmadıysa yedi parçayı yeniden doldur.
        if (tetrominoBag.Count == 0)
        {
            FillAndShuffleBag();
        }

        // Listenin sonundaki parçayı seç.
        int lastIndex = tetrominoBag.Count - 1;

        TetrominoData selectedData =
            tetrominoBag[lastIndex];

        // Seçilen parçayı torbadan çıkar.
        tetrominoBag.RemoveAt(lastIndex);

        return selectedData;
    }
    // Yedi parçayı torbaya ekler ve sıralarını karıştırır.
    private void FillAndShuffleBag()
    {
        // Önce eski torba içeriğini temizle.
        tetrominoBag.Clear();

        // Yedi Tetromino türünün tamamını torbaya ekle.
        for (int i = 0; i < tetrominoes.Length; i++)
        {
            tetrominoBag.Add(tetrominoes[i]);
        }

        // Fisher-Yates yöntemiyle torbadaki parçaları karıştır.
        for (int i = tetrominoBag.Count - 1; i > 0; i--)
        {
            // 0 ile i arasında rastgele bir konum seç.
            int randomIndex = Random.Range(0, i + 1);

            // Mevcut parçayı geçici olarak sakla.
            TetrominoData temporaryData =
                tetrominoBag[i];

            // Rastgele parçayı mevcut konuma taşı.
            tetrominoBag[i] =
                tetrominoBag[randomIndex];

            // Geçici parçayı rastgele seçilen konuma taşı.
            tetrominoBag[randomIndex] =
                temporaryData;
        }
    }


    // Rastgele yeni bir parça oluşturur.
    public void SpawnPiece()
    {
        // Oyun bittiyse yeni parça oluşturma.
        if (isGameOver)
        {
            return;
        }

        // Oyun ilk kez başlıyorsa sıradaki parçayı hazırla.
        if (!hasNextTetromino)
        {
            nextTetrominoData = GetTetrominoFromBag();
            hasNextTetromino = true;
        }

        // Daha önceden hazırlanan parçayı aktif parça yap.
        TetrominoData selectedData = nextTetrominoData;

        // Torbadan yeni bir sıradaki parça al.
        nextTetrominoData = GetTetrominoFromBag();

        // NEXT görselini yeni sıradaki parçaya göre güncelle.
        UpdateNextPieceUI();

        // Mevcut seviyeye uygun düşme süresini hesapla.
        float currentStepDelay = CalculateStepDelay();

        // Yeni parçanın düşme hızını ayarla.
        activePiece.SetStepDelay(currentStepDelay);

        activePiece.Initialize(
            this,
            spawnPosition,
            selectedData
        );

        // Yeni parçanın doğacağı alan boşsa parçayı göster.
        if (IsValidPosition(activePiece, spawnPosition))
        {
            Set(activePiece);
        }
        else
        {
            // Doğma alanı doluysa oyun biter.
            GameOver();
        }
    }


    // Oyunu durdurur ve Game Over panelini gösterir.
    private void GameOver()
    {
        isGameOver = true;
        isPaused = false;

        Time.timeScale = 1f;

        // Aktif parçanın normal hareketlerini durdur.
        activePiece.enabled = false;

        // Pause butonunu Game Over ekranında gizle.
        if (pauseButton != null)
        {
            pauseButton.SetActive(false);
        }

        // Fiziksel atari butonları görünmeye devam etsin.
        if (controls != null)
        {
            controls.SetActive(true);
        }

        // Puanları kontrol et ve yüksek skoru kaydet.
        UpdateGameOverScore();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }


    // Game Over ekranından yeni bir oyun başlatır.
    public void RestartGame()
    {
        Time.timeScale = 1f;

        isGameOver = false;
        isPaused = false;
        isInMainMenu = false;

        // Eski blokları oyun alanından kaldır.
        tilemap.ClearAllTiles();

        // Puan ve level değerlerini sıfırla.
        score = 0;
        totalLines = 0;
        level = 1;

        // 7-bag ve NEXT sistemini sıfırla.
        tetrominoBag.Clear();
        hasNextTetromino = false;

        // Yeniden başlatırken Pause panelini kapat.
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // Önceki satır bildirimi hâlâ çalışıyorsa durdur.
        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
            notificationCoroutine = null;
        }

        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(false);
        }

        // Game Over panelini gizle.
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Ana menü kapalı kalsın.
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        // Oyun arayüzlerini göster.
        if (statsGroup != null)
        {
            statsGroup.SetActive(true);
        }

        if (nextPanel != null)
        {
            nextPanel.SetActive(true);
        }

        if (pauseButton != null)
        {
            pauseButton.SetActive(true);
        }

        if (controls != null)
        {
            controls.SetActive(true);
        }

        // Skor yazılarını başlangıç değerlerine getir.
        UpdateScoreUI();

        // Yeni aktif ve sıradaki parçayı oluştur.
        SpawnPiece();

        activePiece.enabled = true;
    }

#endregion

#region Tilemap Operations

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector2Int cell = piece.cells[i];

            Vector3Int tilePosition = piece.position;
            tilePosition.x += cell.x;
            tilePosition.y += cell.y;

            tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }


    // Aktif parçanın eski görüntüsünü Tilemap üzerinden kaldırır.
    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector2Int cell = piece.cells[i];

            Vector3Int tilePosition = piece.position;
            tilePosition.x += cell.x;
            tilePosition.y += cell.y;

            tilemap.SetTile(tilePosition, null);
        }
    }


    // Parçanın verilen konumda bulunup bulunamayacağını kontrol eder.
    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector2Int cell = piece.cells[i];

            Vector3Int tilePosition = position;
            tilePosition.x += cell.x;
            tilePosition.y += cell.y;

            // Hücre oyun alanının dışına çıkıyorsa konum geçersizdir.
            if (!Bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }

            // Hücrede yerleşmiş başka bir blok varsa konum geçersizdir.
            if (tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }

        return true;
    }

#endregion

#region Line Clearing and Scoring

    public void CheckAndClearLines()
    {
        int row = Bounds.yMin;

        int clearedLineCount = 0;

        while (row < Bounds.yMax)
        {
            if (IsLineFull(row))
            {
                RemoveLine(row);
                clearedLineCount++;
            }
            else
            {
                row++;
            }
        }

        if (clearedLineCount > 0)
        {
            AddLineScore(clearedLineCount);
            ShowLineNotification(clearedLineCount);
        }
    }


    // Belirtilen satırdaki bütün hücrelerin dolu olup olmadığını kontrol eder.
    private bool IsLineFull(int row)
    {
        for (int column = Bounds.xMin;
             column < Bounds.xMax;
             column++)
        {
            Vector3Int tilePosition =
                new Vector3Int(column, row, 0);

            // Tek bir boş hücre bile varsa satır dolu değildir.
            if (!tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }

        return true;
    }


    // Dolu satırı siler ve üzerindeki blokları aşağı kaydırır.
    private void RemoveLine(int row)
    {
        // Dolu satırdaki bütün blokları sil.
        for (int column = Bounds.xMin;
             column < Bounds.xMax;
             column++)
        {
            Vector3Int tilePosition =
                new Vector3Int(column, row, 0);

            tilemap.SetTile(tilePosition, null);
        }

        // Silinen satırın üzerindeki blokları bir hücre aşağı taşı.
        for (int currentRow = row + 1;
             currentRow < Bounds.yMax;
             currentRow++)
        {
            for (int column = Bounds.xMin;
                 column < Bounds.xMax;
                 column++)
            {
                Vector3Int upperPosition =
                    new Vector3Int(column, currentRow, 0);

                TileBase upperTile =
                    tilemap.GetTile(upperPosition);

                tilemap.SetTile(upperPosition, null);

                Vector3Int lowerPosition =
                    new Vector3Int(column, currentRow - 1, 0);

                tilemap.SetTile(lowerPosition, upperTile);
            }
        }
    }

    // Aynı anda silinen satır sayısına göre puan ekler.
    private void AddLineScore(int clearedLineCount)
    {
        int earnedScore = 0;

        // Klasik Tetris puanlamasına yakın değerler kullanıyoruz.
        switch (clearedLineCount)
        {
            case 1:
                earnedScore = 100;
                break;

            case 2:
                earnedScore = 300;
                break;

            case 3:
                earnedScore = 500;
                break;

            case 4:
                earnedScore = 800;
                break;
        }

        // Kazanılan puanı mevcut seviyeyle çarp.
        score += earnedScore * level;

        // Toplam temizlenen satır sayısını artır.
        totalLines += clearedLineCount;

        // Her 10 satırda bir seviye yükselir.
        level = (totalLines / 10) + 1;

        UpdateScoreUI();
    }

    // Puan, satır ve seviye yazılarını günceller.
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Puan: " + score;
        }

        if (linesText != null)
        {
            linesText.text = "Satır: " + totalLines;
        }

        if (levelText != null)
        {
            levelText.text = "Level: " + level;
        }
    }

    // Yavaş veya anında düşürme sırasında kazanılan puanı ekler.
    public void AddDropScore(int earnedScore)
    {
        // Geçersiz veya sıfır puan gelirse işlem yapma.
        if (earnedScore <= 0)
        {
            return;
        }

        score += earnedScore;

        UpdateScoreUI();
    }

#endregion

#region UI Updates


    // Mevcut puanı yüksek skorla karşılaştırır ve Game Over yazılarını günceller.
    private void UpdateGameOverScore()
    {
        // Mevcut puan kayıtlı yüksek puandan fazlaysa yeni rekor kaydet.
        if (score > highScore)
        {
            highScore = score;

            PlayerPrefs.SetInt(HighScoreKey, highScore);
            PlayerPrefs.Save();
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = "PUAN: " + score;
        }

        if (highScoreText != null)
        {
            highScoreText.text = "EN YÜKSEK: " + highScore;
        }
    }



    // Mevcut seviyeye göre parçanın düşme süresini hesaplar.
    private float CalculateStepDelay()
    {
        // Her seviyede düşme süresini yüzde 15 azalt.
        float calculatedDelay =
            startingStepDelay *
            Mathf.Pow(speedMultiplier, level - 1);

        // Hesaplanan sürenin belirlediğimiz sınırın altına
        // düşmesini engelle.
        return Mathf.Max(calculatedDelay, minimumStepDelay);
    }


    // NEXT alanındaki hazır parça görselini günceller.
    private void UpdateNextPieceUI()
    {
        if (nextPieceImage == null)
        {
            return;
        }

        // Sıradaki parçaya ait hazır sprite'ı göster.
        nextPieceImage.sprite =
            nextTetrominoData.previewSprite;

        // Sprite bağlantısı yoksa Image nesnesini gizle.
        nextPieceImage.enabled =
            nextTetrominoData.previewSprite != null;

        // Görselin kendi oranını korumasını sağlar.
        nextPieceImage.preserveAspect = true;
    }


    // Silinen satır sayısına göre gösterilecek metni seçer.
    private void ShowLineNotification(int clearedLineCount)
    {
        if (notificationText == null)
        {
            return;
        }

        string notificationMessage;

        switch (clearedLineCount)
        {
            case 1:
                notificationMessage = "TEK SATIR!";
                break;

            case 2:
                notificationMessage = "ÇIFT SATIR!";
                break;

            case 3:
                notificationMessage = "ÜÇLÜ!";
                break;

            case 4:
                notificationMessage = "TETRIS!";
                break;

            default:
                return;
        }

        // Önceki bildirim hâlâ açıksa onu durdur.
        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
        }

        notificationCoroutine =
            StartCoroutine(
                DisplayNotification(notificationMessage)
            );
    }
    
    
    // Bildirimi belirli süre gösterip ardından gizler.
    private IEnumerator DisplayNotification(string message)
    {
        notificationText.text = message;
        notificationText.gameObject.SetActive(true);

        // Yazıyı tamamen görünür hâle getir.
        Color textColor = notificationText.color;
        textColor.a = 1f;
        notificationText.color = textColor;

        // Bildirimi kısa süre ekranda tut.
        yield return new WaitForSeconds(0.8f);

        notificationText.gameObject.SetActive(false);
        notificationCoroutine = null;
    }

#endregion

#region Main Menu

    // Oyun başlamadan önce atari ekranındaki menüyü gösterir.
    private void OpenMainMenu()
    {
        isInMainMenu = true;
        isPaused = false;

        // Menüde oyun zamanı ilerlemesin.
        Time.timeScale = 0f;

        // İlk parça henüz oluşturulmadığı için Piece çalışmasın.
        activePiece.enabled = false;

        // Menü panelini göster.
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }

        // Skor, satır ve level yazılarını gizle.
        if (statsGroup != null)
        {
            statsGroup.SetActive(false);
        }

        // NEXT kutusunun tamamını gizle.
        if (nextPanel != null)
        {
            nextPanel.SetActive(false);
        }

        // Pause butonunu gizle.
        if (pauseButton != null)
        {
            pauseButton.SetActive(false);
        }

        // Kontrolleri gizlemiyoruz.
        // Yön tuşları ile A ve B görünmeye devam edecek.
        if (controls != null)
        {
            controls.SetActive(true);
        }

        // Pause ve Game Over panellerini kapat.
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    // Mevcut oyunu temizleyerek ana menüye döner.
    public void ReturnToMenu()
    {
        Time.timeScale = 1f;

        // Yerleşmiş bütün blokları temizle.
        tilemap.ClearAllTiles();

        // Torba ve NEXT bilgilerini sıfırla.
        tetrominoBag.Clear();
        hasNextTetromino = false;

        // Puanları başlangıç değerlerine getir.
        score = 0;
        totalLines = 0;
        level = 1;

        isGameOver = false;
        isPaused = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(false);
        }

        UpdateScoreUI();

        // OpenMainMenu zamanlamayı durdurup gerekli UI'ları gizleyecek.
        OpenMainMenu();
    }

#endregion
}
