using UnityEngine;

public class TestingScript : MonoBehaviour
{
    public int numRows = 3;
    public int numCols = 3;
	public Vector2 pointSpacing = Vector2.zero;

	public Grid<int> grid;

    void Start()
    {
		grid = new Grid<int>(numRows, numCols);

		foreach (GridPoint<int> point in grid)
		{
			Debug.Log(point.Index + "->" + point.data);
		}
	}

    void Update()
    {
		if (numRows != grid.NumRows || numCols != grid.NumCols)
		{
			grid = new Grid<int>(numRows, numCols);
		}

		grid.pointSpacing = pointSpacing;
		grid.DrawDebugCells(Color.blue);
		grid.DrawDebugGrid(Color.red);

		Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		GridIndex mouseIndex = grid.GetPointFromWorldPosition(mousePosition);

		Debug.Log(mouseIndex);
	}
}
