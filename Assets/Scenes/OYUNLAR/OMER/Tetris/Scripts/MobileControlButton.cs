using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public enum MobileControlAction
{
    MoveLeft,
    MoveRight,
    SoftDrop,
    HardDrop,
    RotateClockwise,
    RotateCounterClockwise
}

public class MobileControlButton : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler
{
    [Header("Bağlantılar")]
    [SerializeField]
    private Piece activePiece;

    [SerializeField]
    private MobileControlAction action;

    [SerializeField]
    private Board board;

    [Header("Basılı Tutma Ayarları")]
    [SerializeField]
    private bool repeatWhileHeld;

    [SerializeField]
    private float initialDelay = 0.25f;

    [SerializeField]
    private float repeatInterval = 0.08f;

    private bool isPressed;
    private Coroutine repeatCoroutine;

    public void OnPointerDown(PointerEventData eventData)
    {
        // Aynı basışın birden fazla kez başlamasını engelle.
        if (isPressed)
        {
            return;
        }

        isPressed = true;

        // Butona basıldığı anda ilk hareketi gerçekleştir.
        ExecuteAction();

        // Bu butonda basılı tutma açıksa tekrar işlemini başlat.
        if (repeatWhileHeld)
        {
            repeatCoroutine = StartCoroutine(RepeatAction());
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopRepeating();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopRepeating();
    }

    private IEnumerator RepeatAction()
    {
        // İlk hareketten sonra kısa süre bekle.
        yield return new WaitForSecondsRealtime(initialDelay);

        // Butona basılı tutulduğu sürece hareketi tekrarla.
        while (isPressed)
        {
            ExecuteAction();

            yield return new WaitForSecondsRealtime(repeatInterval);
        }
    }

    private void ExecuteAction()
    {
        if (board == null)
        {
            return;
        }

        // Ana menüde yalnızca A butonu oyunu başlatır.
        if (board.isInMainMenu)
        {
            if (action == MobileControlAction.RotateCounterClockwise)
            {
                board.StartGame();
            }

            return;
        }

        // Game Over ekranında A yeniden başlatır, B menüye döner.
        if (board.isGameOver)
        {
            if (action == MobileControlAction.RotateCounterClockwise)
            {
                board.RestartGame();
            }
            else if (action == MobileControlAction.RotateClockwise)
            {
                board.ReturnToMenu();
            }

            return;
        }

        // Pause ekranında A devam ettirir, B ana menüye döner.
        if (board.isPaused)
        {
            if (action == MobileControlAction.RotateCounterClockwise)
            {
                board.ResumeGame();
            }
            else if (action == MobileControlAction.RotateClockwise)
            {
                board.RestartGame();
            }

            // Pause sırasında yön ve düşürme işlemlerini çalıştırma.
            return;
        }

        if (activePiece == null)
        {
            return;
        }

        // Normal oyun sırasında butonların asıl görevleri çalışır.
        switch (action)
        {
            case MobileControlAction.MoveLeft:
                activePiece.MoveLeft();
                break;

            case MobileControlAction.MoveRight:
                activePiece.MoveRight();
                break;

            case MobileControlAction.SoftDrop:
                activePiece.SoftDrop();
                break;

            case MobileControlAction.HardDrop:
                activePiece.HardDrop();
                break;

            case MobileControlAction.RotateClockwise:
                activePiece.RotateClockwise();
                break;

            case MobileControlAction.RotateCounterClockwise:
                activePiece.RotateCounterClockwise();
                break;
        }
    }

    private void StopRepeating()
    {
        isPressed = false;

        if (repeatCoroutine != null)
        {
            StopCoroutine(repeatCoroutine);
            repeatCoroutine = null;
        }
    }

    private void OnDisable()
    {
        StopRepeating();
    }
}
