using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct GridIndex
{
    /// <summary>
    /// Row of the point, 0+
    /// </summary>
    public int row;

    /// <summary>
    /// Col of the point, 0+
    /// </summary>
    public int col;

	public static GridIndex zero = new GridIndex() { row = 0, col = 0 };

	public override bool Equals(object obj)
	{
		GridIndex other = (GridIndex)obj;
		return row == other.row && col == other.col;
	}

	public override string ToString()
	{
		return row+","+col;
	}
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="DataType">Struct to hold data relevant to the point</typeparam>
public class GridPoint<DataType>
{
	public GridPoint(GridIndex pointIndex)
    {
        _index = pointIndex;
    }

    public GridPoint(int row, int col)
    {
        _index.row = row;
        _index.col = col;
    }

	public GridIndex Index => _index;
    protected GridIndex _index = GridIndex.zero;

    public DataType data;
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="DataType">Struct to use as point data</typeparam>
public class Grid<DataType> : IEnumerable<GridPoint<DataType>>
{
    public Vector2 pointSpacing = Vector2.zero;
    public Vector3 topLeft = Vector3.zero;
    public int NumRows => points.Length > 0 ? points.GetLength(0) : -1;
	public int NumCols => points.Length > 0 ? points.GetLength(1) : -1;
	public Vector3 BottomRight => topLeft + new Vector3(pointSpacing.x * NumCols, -pointSpacing.y * NumRows, 0); 


	protected GridPoint<DataType>[,] points;

	public Grid(int numRows, int numCols)
    {
        if (numRows > 0 && numCols > 0)
        {
            points = new GridPoint<DataType>[numRows, numCols];
            for (int row = 0; row < numRows; ++row)
            {
                for (int col = 0; col < numCols; ++col)
                {
                    GridIndex pointIndex = new GridIndex() { row = row, col = col };

					// Initialize point with index and no data
					points[row, col] = new GridPoint<DataType>(pointIndex);
                }
            }
        }
    }

	public Grid(int numRows, int numCols, DataType defaultData)
	{
		if (numRows > 0 && numCols > 0)
		{
			points = new GridPoint<DataType>[numRows, numCols];
			for (int row = 0; row < numRows; ++row)
			{
				for (int col = 0; col < numCols; ++col)
				{
					GridIndex pointIndex = new GridIndex() { row = row, col = col };
					
					// Initialize point with index and default data
					GridPoint<DataType> gridPoint = new GridPoint<DataType>(pointIndex);
                    gridPoint.data = defaultData;

					points[row, col] = gridPoint;
				}
			}
		}
	}

	public bool IsValidIndex(GridIndex index)
    {
        return points.Length >  0
            && index.row    >= 0
            && index.row    <  points.GetLength(0)
            && index.col    >= 0
            && index.col    <  points.GetLength(1);
	}

    public GridPoint<DataType> GetPoint(GridIndex index)
    {
        return points[index.row, index.col];
    }

	public List<GridIndex> GetNeightbors(GridIndex index, bool sorted = true)
	{
		List<GridIndex> output = new List<GridIndex>();

		bool firstRow = index.row == 0;
		bool lastRow = index.row == NumRows - 1;
		bool firstCol = index.col == 0;
		bool lastCol = index.col == NumCols - 1;

		// row above the point
		if (!firstRow)
		{
			if (!firstCol) 
			{
				output.Add(new GridIndex { row = index.row - 1, col = index.col - 1 }); 
			}
			
			output.Add(new GridIndex { row = index.row - 1, col = index.col }); 
			
			if (!lastCol) 
			{ 
				output.Add(new GridIndex { row = index.row - 1, col = index.col + 1 });
			}
		}

		// col to the right of the point
		if (!lastCol)
		{
			output.Add(new GridIndex { row = index.row, col = index.col + 1 });

			if (!lastRow)
			{
				output.Add(new GridIndex { row = index.row + 1, col = index.col + 1 });
			}
		}

		// row below the point
		if (!lastRow)
		{
			output.Add(new GridIndex { row = index.row + 1, col = index.col });

			if (!firstCol)
			{
				output.Add(new GridIndex { row = index.row + 1, col = index.col - 1 });
			}
		}

		// directly to the left of the point
		if (!firstRow)
		{
			output.Add(new GridIndex { row = index.row - 1, col = index.col });
		}

		return output;
	}

	public Vector3 GetWorldPosition(GridIndex index)
	{
		if (pointSpacing.magnitude > 0)
		{
			float x = topLeft.x + pointSpacing.x * index.col;
			float y = topLeft.y - pointSpacing.y * index.row;
			return new Vector3(x, y, 0);
		}

		return topLeft;
	}

	public Vector3[] GetCellWorldCorners(GridIndex index)
	{
		Vector3 cellCenter = GetWorldPosition(index);
		return new Vector3[] {
			cellCenter + new Vector3(-pointSpacing.x/2, pointSpacing.y/2, 0),
			cellCenter + new Vector3(pointSpacing.x/2,  pointSpacing.y/2, 0),
			cellCenter + new Vector3(pointSpacing.x/2,  -pointSpacing.y/2, 0),
			cellCenter + new Vector3(-pointSpacing.x/2, -pointSpacing.y/2, 0)
		};
	}

	public bool IsWorldPositionInGrid(Vector3 worldPosition)
	{
		return worldPosition.x >= topLeft.x && worldPosition.x <= BottomRight.x
			&& worldPosition.y <= topLeft.y && worldPosition.y >= BottomRight.y;
	}

	public bool IsWorldPositionInCell(Vector3 worldPosition)
	{
		Vector3 cellTopLeft = BottomRight + new Vector3(-0.5f * pointSpacing.x, 0.5f * pointSpacing.y, 0);
		Vector3 cellBottomRight = BottomRight + new Vector3(0.5f * pointSpacing.x, -0.5f * pointSpacing.y, 0);
		return worldPosition.x >= cellTopLeft.x  && worldPosition.x <= cellBottomRight.x
			&& worldPosition.y <= cellTopLeft.y && worldPosition.y >= cellBottomRight.y;
	}

	public GridIndex GetPointFromWorldPosition(Vector3 worldPosition)
	{
		Vector3 adjustedPosition = worldPosition - topLeft;
		return new GridIndex() 
		{
			row = (int)Math.Round(-adjustedPosition.y / pointSpacing.y),
			col = (int)Math.Round(adjustedPosition.x / pointSpacing.x)
		};
	}

	public void DrawDebugCells(Color color)
	{
		foreach (GridPoint<DataType> point in this)
		{
			Vector3 center = GetWorldPosition(point.Index);

			Vector3[] corners = GetCellWorldCorners(point.Index);

			Debug.DrawLine(corners[0], corners[1], color);
			Debug.DrawLine(corners[1], corners[2], color);
			Debug.DrawLine(corners[2], corners[3], color);
			Debug.DrawLine(corners[3], corners[0], color);
		}
	}

	public void DrawDebugGrid(Color color)
	{
		foreach (GridPoint<DataType> point in this)
		{
			Vector3 center = GetWorldPosition(point.Index);

			float crossSize = 0.1f;
			Debug.DrawLine(
				center + new Vector3(-crossSize, -crossSize, 0), 
				center + new Vector3(crossSize, crossSize, 0), 
				color);
			Debug.DrawLine(
				center + new Vector3(-crossSize, crossSize), 
				center + new Vector3(crossSize, -crossSize), 
				color);

			List<GridIndex> neighbors = GetNeightbors(point.Index);
			foreach (GridIndex index in neighbors)
			{
				if (index.row > point.Index.row || index.col > point.Index.col)
				{
					Vector3 neighborLocation = GetWorldPosition(index);
					Debug.DrawLine(center, neighborLocation, color);
				}
			}
		}
	}

	public IEnumerator<GridPoint<DataType>> GetEnumerator()
	{
		return new GridEnumerator<DataType>(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return (IEnumerator)GetEnumerator();
	}
}

public class GridEnumerator<DataType> : IEnumerator<GridPoint<DataType>>
{
	private Grid<DataType> grid;
    private GridIndex index = new GridIndex() { row = 0, col = -1 };

	public GridEnumerator(Grid<DataType> targetGrid)
	{
		grid = targetGrid;
	}

	object IEnumerator.Current => Current;

	public GridPoint<DataType> Current
	{
		get
		{
			try
			{
				return grid.GetPoint(index);
			}
			catch (IndexOutOfRangeException)
			{
				throw new InvalidOperationException();
			}
		}
	}

	public bool MoveNext()
	{
		++index.col;
		if (index.col >= grid.NumCols)
		{
			index.col = 0;
			++index.row;
			if (index.row >= grid.NumRows)
			{
				return false;
			}
		}

		return true;
	}

	public void Reset()
	{
		index = new GridIndex() { row = 0, col = -1 };
	}

	public void Dispose()
	{
		// No need to dispose for now
	}
}
