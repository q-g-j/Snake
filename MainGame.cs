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

public class MainGame : Node
{
    private Vector2 screenSize;
    private Vector2 tileMapSize;
    private Vector2 cellSize;
    private TileMap tileMap;
    private readonly int SNAKE = 0;
    private readonly int APPLE = 1;
    private Vector2 applePosition;
    private List<Vector2> snakeBody;
    private Direction headDirection;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        screenSize = GetViewport().Size;
        tileMap = GetNode<TileMap>("TileMap");
        cellSize = tileMap.CellSize;
        tileMapSize = new Vector2(tileMap.GetViewportRect().Size.x / cellSize.x, tileMap.GetViewportRect().Size.y / cellSize.y);

        snakeBody = new List<Vector2>()
        {
            new Vector2(8, 10),
            new Vector2(7, 10),
            new Vector2(6, 10)
        };

        headDirection = Direction.Right;
    }

    private Vector2 GetApplePosition()
    {
        float x = 0;
        float y = 0;

        bool doRun = true;

        while (doRun)
        {
            GD.Randomize();
            x = GD.Randi() % ((int)(tileMapSize.x));
            y = GD.Randi() % ((int)(tileMapSize.y));

            doRun = false;
            foreach (Vector2 pos in snakeBody)
            {
                if (pos - new Vector2(x, y) == Vector2.Zero)
                {
                    GD.Print("here");
                    doRun = true;
                    break;
                }
            }
        }

        return new Vector2(x, y);
    }

    private void DrawApple()
    {
        tileMap.SetCell(((int)applePosition.x), ((int)applePosition.y), APPLE, false, false, false);
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

    private void DrawSnake()
    {
        for (int i = 0; i < snakeBody.Count; i++)
        {
            if (i == 0)
            {
                if (headDirection == Direction.Right)
                {
                    tileMap.SetCell(((int)snakeBody[0].x), ((int)snakeBody[0].y), SNAKE, false, false, false, new Vector2(6, 0));
                }
                else if (headDirection == Direction.Left)
                {
                    tileMap.SetCell(((int)snakeBody[0].x), ((int)snakeBody[0].y), SNAKE, false, false, false, new Vector2(5, 0));
                }
                else if (headDirection == Direction.Up)
                {
                    tileMap.SetCell(((int)snakeBody[0].x), ((int)snakeBody[0].y), SNAKE, false, false, false, new Vector2(5, 1));
                }
                else if (headDirection == Direction.Down)
                {
                    tileMap.SetCell(((int)snakeBody[0].x), ((int)snakeBody[0].y), SNAKE, false, false, false, new Vector2(6, 1));
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
        DeleteCells(SNAKE);

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

        if (newHead - applePosition == Vector2.Zero)
        {
            tileMap.SetCell(((int)applePosition.x), ((int)applePosition.y), -1);
            applePosition = GetApplePosition();
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
        
        if (snakeBody[0] - applePosition == Vector2.Zero)
        {
            tileMap.SetCell(((int)applePosition.x), ((int)applePosition.y), -1);
            applePosition = GetApplePosition();
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
        DrawApple();
    }

    public override void _Input(InputEvent inputEvent)
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
