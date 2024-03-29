using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

enum Direction
{
    Left,
    Right,
    Up,
    Down,
    None
}

public class Main : Node
{
    private Vector2 screenSize;
    private Vector2 tileMapSize;
    private Vector2 cellSize;
    private TileMap tileMap;
    private readonly int SNAKE = 0;
    private readonly int FRUIT = 1;
    private Vector2 fruitPosition;
    private List<Vector2> snakeBody;
    private Direction headDirection;
    private bool isCollision = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        screenSize = GetViewport().Size;
        tileMap = GetNode<TileMap>("TileMap");
        cellSize = tileMap.CellSize;
        tileMapSize = new Vector2(tileMap.GetViewportRect().Size.x / cellSize.x, tileMap.GetViewportRect().Size.y / cellSize.y);

        snakeBody = new List<Vector2>()
        {
            new Vector2(10, 10),
            new Vector2(9, 10),
            new Vector2(8, 10),
            // new Vector2(7, 10),
            // new Vector2(6, 10),
            // new Vector2(5, 10),
            // new Vector2(4, 10),
            // new Vector2(3, 10),
            // new Vector2(2, 10),
            // new Vector2(1, 10),
            // new Vector2(0, 10),
        };

        headDirection = Direction.Right;

        fruitPosition = GetRandomFruitPosition();
    }

    private List<Vector2> GetFreeCells()
    {
        List<Vector2> freeCells = new List<Vector2>();
        var usedCells = tileMap.GetUsedCells();

        for (int i = 0; i < tileMapSize.x - 1; i++)
        {
            for (int j = 0; j < tileMapSize.y - 1; j++)
            {
                var v = new Vector2(i, j);
                bool isInUsedCells = false;
                foreach (var usedCell in usedCells)
                {
                    if (v - (Vector2)usedCell == Vector2.Zero)
                    {
                        isInUsedCells = true;
                        break;
                    }
                }
                if (! isInUsedCells)
                {
                    freeCells.Add(v);
                }
            }
        }

        return freeCells;
    }

    private Vector2 GetRandomFruitPosition()
    {
        var freeCells = GetFreeCells();

        GD.Randomize();
        var randIndex = (int)(GD.Randi() % (freeCells.Count - 1));
        return freeCells[randIndex];
    }

    private void DrawFruit()
    {
        tileMap.SetCell(((int)fruitPosition.x), ((int)fruitPosition.y), FRUIT, false, false, false);
    }

    private Direction GetAdjacentPartPosition(int currentIndex, bool isPreviousPart)
    {
        int signModifier = isPreviousPart ? -1 : 1;

        if (snakeBody[currentIndex].x == 0 && snakeBody[currentIndex + 1 * signModifier].x == tileMapSize.x - 1)
        {
            return Direction.Left;
        }
        if (snakeBody[currentIndex].x == tileMapSize.x - 1 && snakeBody[currentIndex + 1 * signModifier].x == 0)
        {
            return Direction.Right;
        }
        if (snakeBody[currentIndex].y == 0 && snakeBody[currentIndex + 1 * signModifier].y == tileMapSize.y - 1)
        {
            return Direction.Up;
        }
        if (snakeBody[currentIndex].y == tileMapSize.y - 1 && snakeBody[currentIndex + 1 * signModifier].y == 0)
        {
            return Direction.Down;
        }
        // is subsequent part's position left of current part:
        if (snakeBody[currentIndex + 1 * signModifier].x < snakeBody[currentIndex].x && snakeBody[currentIndex + 1 * signModifier].y == snakeBody[currentIndex].y)
        {
            return Direction.Left;
        }
        // is subsequent part's position right of current part:
        if (snakeBody[currentIndex + 1 * signModifier].x > snakeBody[currentIndex].x && snakeBody[currentIndex + 1 * signModifier].y == snakeBody[currentIndex].y)
        {
            return Direction.Right;
        }
        // is subsequent part's position above of current part:
        if (snakeBody[currentIndex + 1 * signModifier].x == snakeBody[currentIndex].x && snakeBody[currentIndex + 1 * signModifier].y < snakeBody[currentIndex].y)
        {
            return Direction.Up;
        }
        // is subsequent part's position below of current part:
        else if (snakeBody[currentIndex + 1 * signModifier].x == snakeBody[currentIndex].x && snakeBody[currentIndex + 1 * signModifier].y > snakeBody[currentIndex].y)
        {
            return Direction.Down;
        }

        return Direction.None;
    }

    private Direction GetSubsequentPartPosition(int index)
    {
        return GetAdjacentPartPosition(index, false);
    }

    private Direction GetPreviousPartPosition(int index)
    {
        return GetAdjacentPartPosition(index, true);
    }

    private void GameOver()
    {
        GetNode<Timer>("Timer").Stop();
        
        Sprite sprite = new Sprite();
        Texture snake = tileMap.TileSet.TileGetTexture(SNAKE);
        sprite.Texture = snake;
        sprite.RegionEnabled = true;

        int offsetX = 0;
        int offsetY = 0;

        if (headDirection == Direction.Right)
        {
            offsetX = 6 * 40;
            offsetY = 0 * 40;
        }
        else if (headDirection == Direction.Left)
        {
            offsetX = 5 * 40;
            offsetY = 0 * 40;
        }
        else if (headDirection == Direction.Up)
        {
            offsetX = 5 * 40;
            offsetY = 1 * 40;
        }
        else if (headDirection == Direction.Down)
        {
            offsetX = 6 * 40;
            offsetY = 1 * 40;
        }

        AddChild(sprite);
        sprite.RegionRect = new Rect2(new Vector2(offsetX, offsetY), new Vector2(cellSize.x, cellSize.y));
        sprite.Position = new Vector2(snakeBody[0].x * 40 + 20, snakeBody[0].y * 40 + 20);
    }

    private void DrawSnake()
    {
        DeleteCells(SNAKE);

        Vector2 headTexture = Vector2.Zero;

        for (int i = 0; i < snakeBody.Count; i++)
        {
            if (i == 0)
            {
                if (headDirection == Direction.Right)
                {
                    headTexture = new Vector2(6, 0);
                }
                else if (headDirection == Direction.Left)
                {
                    headTexture = new Vector2(5, 0);
                }
                else if (headDirection == Direction.Up)
                {
                    headTexture = new Vector2(5, 1);
                }
                else if (headDirection == Direction.Down)
                {
                    headTexture = new Vector2(6, 1);
                }
            }
            else if (i == snakeBody.Count - 1)
            {
                if (GetPreviousPartPosition(i) == Direction.Right)
                {
                    tileMap.SetCell(((int)snakeBody[i].x), ((int)snakeBody[i].y), SNAKE, false, false, false, new Vector2(1, 0));
                }
                else if (GetPreviousPartPosition(i) == Direction.Left)
                {
                    tileMap.SetCell(((int)snakeBody[i].x), ((int)snakeBody[i].y), SNAKE, false, false, false, new Vector2(2, 0));
                }
                else if (GetPreviousPartPosition(i) == Direction.Down)
                {
                    tileMap.SetCell(((int)snakeBody[i].x), ((int)snakeBody[i].y), SNAKE, false, false, false, new Vector2(3, 0));
                }
                else if (GetPreviousPartPosition(i) == Direction.Up)
                {
                    tileMap.SetCell(((int)snakeBody[i].x), ((int)snakeBody[i].y), SNAKE, false, false, false, new Vector2(4, 0));
                }
            }
            else
            {
                // previous == left && subsequent == right ||
                // previous == right && subsequent == right
                if (
                    (GetPreviousPartPosition(i) == Direction.Left && GetSubsequentPartPosition(i) == Direction.Right) ||
                    (GetPreviousPartPosition(i) == Direction.Right && GetSubsequentPartPosition(i) == Direction.Left)
                )
                {
                    tileMap.SetCell(((int)snakeBody[i].x), ((int)snakeBody[i].y), SNAKE, false, false, false, new Vector2(0, 0));
                }
                // previous == down && subsequent == up ||
                // previous == up && subsequent == down
                else if (
                    (GetPreviousPartPosition(i) == Direction.Down && GetSubsequentPartPosition(i) == Direction.Up) ||
                    (GetPreviousPartPosition(i) == Direction.Up && GetSubsequentPartPosition(i) == Direction.Down)
                )
                {
                    tileMap.SetCell(((int)snakeBody[i].x), ((int)snakeBody[i].y), SNAKE, false, false, false, new Vector2(0, 1));
                }
                // previous == left && subsequent == up ||
                // previous == up && subsequent == left
                else if (
                    (GetPreviousPartPosition(i) == Direction.Left && GetSubsequentPartPosition(i) == Direction.Up) ||
                    (GetPreviousPartPosition(i) == Direction.Up && GetSubsequentPartPosition(i) == Direction.Left)
                )
                {
                    tileMap.SetCell(((int)snakeBody[i].x), ((int)snakeBody[i].y), SNAKE, false, false, false, new Vector2(4, 1));
                }
                // previous == left && subsequent == down ||
                // previous == down && subsequent == left
                else if (
                    (GetPreviousPartPosition(i) == Direction.Left && GetSubsequentPartPosition(i) == Direction.Down) ||
                    (GetPreviousPartPosition(i) == Direction.Down && GetSubsequentPartPosition(i) == Direction.Left)
                )
                {
                    tileMap.SetCell(((int)snakeBody[i].x), ((int)snakeBody[i].y), SNAKE, false, false, false, new Vector2(2, 1));
                }
                // previous == down && subsequent == right ||
                // previous == right && subsequent == down
                else if (
                    (GetPreviousPartPosition(i) == Direction.Down && GetSubsequentPartPosition(i) == Direction.Right) ||
                    (GetPreviousPartPosition(i) == Direction.Right && GetSubsequentPartPosition(i) == Direction.Down)
                )
                {
                    tileMap.SetCell(((int)snakeBody[i].x), ((int)snakeBody[i].y), SNAKE, false, false, false, new Vector2(1, 1));
                }
                // previous == up && subsequent == right ||
                // previous == right && subsequent == up
                else if (
                    (GetPreviousPartPosition(i) == Direction.Up && GetSubsequentPartPosition(i) == Direction.Right) ||
                    (GetPreviousPartPosition(i) == Direction.Right && GetSubsequentPartPosition(i) == Direction.Up)
                )
                {
                    tileMap.SetCell(((int)snakeBody[i].x), ((int)snakeBody[i].y), SNAKE, false, false, false, new Vector2(3, 1));
                }
            }


            for (int j = 1; j < snakeBody.Count; j++)
            {
                if (snakeBody[0] - snakeBody[j] == Vector2.Zero)
                {
                    isCollision = true;
                    break;
                }
            }
            if (i == 0 && ! isCollision)
            {
                tileMap.SetCell(((int)snakeBody[0].x), ((int)snakeBody[0].y), SNAKE, false, false, false, headTexture);
            }
        }
    }

    private void DeleteCells(int type)
    {
        var allCells = tileMap.GetUsedCellsById(type);
        foreach (var item in allCells)
        {
            tileMap.SetCell(((int)((Vector2)item).x), ((int)((Vector2)item).y), -1);
        }
    }

    private void MoveSnake()
    {
        var direction = Vector2.Zero;

        if (headDirection == Direction.Left)
        {
            direction += new Vector2(-1, 0);
        }
        else if (headDirection == Direction.Right)
        {
            direction += new Vector2(1, 0);
        }
        else if (headDirection == Direction.Up)
        {
            direction += new Vector2(0, -1);
        }
        else if (headDirection == Direction.Down)
        {
            direction += new Vector2(0, 1);
        }        

        var newHead = snakeBody[0] + direction;

        List<Vector2> newBodyList = null;

        if (newHead - fruitPosition == Vector2.Zero)
        {
            tileMap.SetCell(((int)fruitPosition.x), ((int)fruitPosition.y), -1);
            fruitPosition = GetRandomFruitPosition();
            newBodyList = snakeBody.Take(snakeBody.Count).ToList();
        }
        else
        {
            newBodyList = snakeBody.Take(snakeBody.Count - 1).ToList();
        }

        newBodyList.Insert(0, newHead);
        snakeBody = newBodyList;

        for (int i = 0; i < snakeBody.Count; i++)
        {
            if (snakeBody[i].x == -1)
            {
                snakeBody[i] = new Vector2(tileMapSize.x - 1, snakeBody[i].y);
            }
            else if (snakeBody[i].x == tileMapSize.x)
            {
                snakeBody[i] = new Vector2(0, snakeBody[i].y);
            }
            else if (snakeBody[i].y == -1)
            {
                snakeBody[i] = new Vector2(snakeBody[i].x, tileMapSize.y - 1);
            }
            else if (snakeBody[i].y == tileMapSize.y)
            {
                snakeBody[i] = new Vector2(snakeBody[i].x, 0);
            }
        }
        
        if (snakeBody[0] - fruitPosition == Vector2.Zero)
        {
            tileMap.SetCell(((int)fruitPosition.x), ((int)fruitPosition.y), -1);
            fruitPosition = GetRandomFruitPosition();
            newBodyList = snakeBody.Take(snakeBody.Count).ToList();
        }
        else
        {
            newBodyList = snakeBody.Take(snakeBody.Count - 1).ToList();
        }
    }

    public void OnTimerTimeOut()
    {
        MoveSnake();
        DrawSnake();

        if (! isCollision)
        {
            DrawFruit();
        }
        else
        {
            GameOver();
        }
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (! isCollision)
        {
            if (inputEvent.IsActionPressed("turn_left"))
            {
                if (GetSubsequentPartPosition(0) == Direction.Left) return;
                headDirection = Direction.Left;
            }
            else if (inputEvent.IsActionPressed("turn_right"))
            {
                if (GetSubsequentPartPosition(0) == Direction.Right) return;
                headDirection = Direction.Right;
            }
            else if (inputEvent.IsActionPressed("turn_up"))
            {
                if (GetSubsequentPartPosition(0) == Direction.Up) return;
                headDirection = Direction.Up;
            }
            else if (inputEvent.IsActionPressed("turn_down"))
            {
                if (GetSubsequentPartPosition(0) == Direction.Down) return;
                headDirection = Direction.Down;
            }
        }
    }
}
