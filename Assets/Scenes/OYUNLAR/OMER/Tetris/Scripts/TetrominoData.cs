using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public struct TetrominoData
{
    public Tetromino tetromino;
    public Tile tile;
    public Sprite previewSprite;

    public Vector2Int[] cells { get; private set; }

    public void Initialize()
    {
        cells = TetrominoCells.Cells[tetromino];
    }
}
