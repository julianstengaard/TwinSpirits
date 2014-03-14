using UnityEngine;
using System.Collections;

public class MazeCell
{
	public int column;
	public int row;
	public int stepsFromOrigin;
	public int generationNumber;
	public bool[] doors;

	public MazeCell (int column, int row, bool[] doors)
	{
		this.column = column;
		this.row = row;
		this.doors = doors;
	}

	//Short constructor for locked MazeCell
	public MazeCell (int column, int row) :	this(column, row, new bool[4]{false, false, false, false})
	{
	}

	public void CreateDoor(MazeCell mazeCell)
	{
		int sideToCreateDoorOn = FindAdjacentCellWall(mazeCell);
		doors[sideToCreateDoorOn] = true;
		
		int oppositeDoorSide = (sideToCreateDoorOn + 2) % 4;
		mazeCell.doors[oppositeDoorSide] = true;
	}

	public bool HasNoDoors()
	{
		for (int i = 0; i < 4; i++)
		{
			if(doors[i] == true)
			{
				return false;
			}
		}
		return true;
	}

	public int FindAdjacentCellWall(MazeCell mazeCell)
	{
		if( mazeCell.row == this.row )
		{
			if( mazeCell.column < this.column)
			{
				return 0;
			}
			else
			{
				return 2;
			}
		}
		else
		{
			if( mazeCell.row < this.row )
			{
				return 1;
			}
			else
			{
				return 3;
			}
		}
	}

	public void PrintToConsole()
	{
		Debug.Log("MazeCell at (" + column + ", " + row + ") = " 
		      + doors[0] + "," + doors[1] + "," + doors[2] + "," + doors[3]);
	}

}
