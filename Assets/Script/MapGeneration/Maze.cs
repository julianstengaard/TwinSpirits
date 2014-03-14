using UnityEngine;
using System.Collections;
using System;

public class Maze 
{
	public int width;
	public int height;
	public int MaxSteps {get; private set;}

	private MazeCell[,] cells;

	public Maze (int width, int height)
	{
		this.width = width;
		this.height = height;

		cells = new MazeCell[width, height];

		//Fill the cells-array
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				cells[x,y] = new MazeCell(x, y);
			}
		}
	}

	public void GenerateMazeFromCell(int cellColumn, int cellRow, int mazeLength, int maxPathIterationLenght)
	{
		int visitedCells = 1;
		int totalCells = 0;
		int maxPathLength = 0;

		if (mazeLength > 0)
		{
			totalCells = mazeLength;
		}
		else
		{
			totalCells = height * width;
		}

		if (maxPathIterationLenght > 0)
		{
			maxPathLength = maxPathIterationLenght;
		}
		else
		{
			maxPathLength = height * width;
		}

		MazeCell currentCell = cells[cellColumn, cellRow];
		currentCell.generationNumber = 0;
		currentCell.stepsFromOrigin	 = 0;

		// This is used to pick a random side with a wall.
		long seed = DateTime.Now.Ticks;
		System.Random random = new System.Random( (int) seed );
		
		Stack cellStack = new Stack();
		cellStack.Clear();

		int currentPathLength = 0;
		int generationNumber = 0;

		while (visitedCells < totalCells)
		{
			ArrayList neighborCells = GetCellNeighbors(currentCell);

			if (neighborCells.Count > 0 && currentPathLength < maxPathLength)
			{
				generationNumber++;

				int randomNeighbor = random.Next(0, neighborCells.Count);

				MazeCell neighborCell = (MazeCell) neighborCells[randomNeighbor];
				neighborCell.stepsFromOrigin = currentCell.stepsFromOrigin + 1;
				neighborCell.generationNumber = generationNumber;

				MaxSteps = MaxSteps < neighborCell.stepsFromOrigin ? neighborCell.stepsFromOrigin : MaxSteps;

				currentCell.CreateDoor(neighborCell);
				cellStack.Push(currentCell);
				currentCell = neighborCell;

				currentPathLength++;
				visitedCells++;
			}
			else
			{
				// Has no walls, so we dont' need it :)
				currentCell = (MazeCell) cellStack.Pop();
				currentPathLength = 0;
			}
		}
	}

	private ArrayList GetCellNeighbors(MazeCell mazeCell)
	{
		ArrayList neighbors = new ArrayList();

		for (int currentRow = -1; currentRow <= 1; currentRow++)
		{
			for (int currentColumn = -1; currentColumn <= 1; currentColumn++)
			{
				if ((mazeCell.row + currentRow < height) 		&&
			    	(mazeCell.column + currentColumn < width)	&&
			    	(mazeCell.row + currentRow >= 0)			&&
			    	(mazeCell.column + currentColumn >= 0)		&&
				    ((currentColumn == 0) ^ (currentRow == 0)) )
				{
					if( cells[mazeCell.column + currentColumn, mazeCell.row + currentRow].HasNoDoors() )
					{
						neighbors.Add( cells[mazeCell.column + currentColumn, mazeCell.row + currentRow] );
					}
				}
			}
		}
		return neighbors;
	}

	public MazeCell GetCell(int column, int row)
	{
		return cells[column, row];
	}

	public void PrintToConsole()
	{
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				cells[x,y].PrintToConsole();
			}
		}
	}

	public void PrintStatsToConsole()
	{
		int isolatedCells = 0;
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				if (cells[x,y].HasNoDoors())
				{
					isolatedCells++;
				}
			}
		}
		Debug.Log("Created a maze with " + width*height + " cells (" + isolatedCells + " isolated)");
	}

}