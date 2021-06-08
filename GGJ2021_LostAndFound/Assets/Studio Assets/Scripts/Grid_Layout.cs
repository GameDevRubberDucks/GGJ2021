using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Layout", menuName = "Custom/GridLayout", order = 1)]
public class Grid_Layout : ScriptableObject
{
    public readonly Vector2Int MAX_GRID_SIZE = new Vector2Int(9, 9);

    public string[] m_gridRows;

    private string[] TransposeGrid(string[] _rows)
    {
        // TEMP: Convert the rows from the inspector fields to columns since the game logic works on columns
        // NOTE: This won't be necessary later when the custom inspector is added
        string[] cols = new string[MAX_GRID_SIZE.x];

        for (int col = 0; col < MAX_GRID_SIZE.x; col++)
        {
            for (int row = 0; row < MAX_GRID_SIZE.y; row++)
            {
                var rowObj = _rows[row];

                cols[col] += rowObj[col];
            }
        }

        return cols;
    }

    // TODO: Expose an interface for this through an editor script so that the level can be more easily edited
    public List<List<Grid_Cell>> GenerateGridCols()
    {
        // TEMP: Tranpose the rows in the inspector so we have a list of columns instead since that is how the movement logic works
        string[] gridColStrs = TransposeGrid(m_gridRows);

        // Create a list to hold all of the cells
        List<List<Grid_Cell>> gridCols = new List<List<Grid_Cell>>();

        // Generate the cells in each of the columns, determining if they are available to be filled or not
        for (int colNum = 0; colNum < gridColStrs.Length; colNum++)
        {
            string col = gridColStrs[colNum];
            var newColList = new List<Grid_Cell>();

            foreach(var cell in col)
            {
                Grid_Cell cellObj = new Grid_Cell();
                cellObj.CellType = (cell == 'O') ? Grid_CellType.Available : Grid_CellType.Unavailable;

                newColList.Add(cellObj);
            }

            gridCols.Add(newColList);
        }

        // Now that all of the cells have been created, we need to connect the different cells together
        // This will dictate how the pieces fall within the grid (they will skip over unavailable ones)
        foreach(var col in gridCols)
        {
            // Move the window over for which cell we are currently trying to connect
            for(int rowNum = 0; rowNum < col.Count; rowNum++)
            {
                // If the cell is not open for people to be placed into, we can just skip it
                var currentCell = col[rowNum];
                if (currentCell.CellType == Grid_CellType.Unavailable)
                    continue;

                // Otherwise, we need to find the neighbouring open cells, if there are any
                // Move up from the current cell to see if there is another cell above it that is open
                // If so, the closest one will be the upper connection
                for(int rowNumUp = rowNum - 1; rowNumUp >= 0; rowNumUp--)
                {
                    if (col[rowNumUp].CellType == Grid_CellType.Available)
                    {
                        currentCell.Previous = col[rowNumUp];
                        break;
                    }
                }

                // Now, move down from the current cell to see if there is another cell below that is open
                for (int rowNumDown = rowNum + 1; rowNumDown < col.Count; rowNumDown++)
                {
                    if (col[rowNumDown].CellType == Grid_CellType.Available)
                    {
                        currentCell.Next = col[rowNumDown];
                        break;
                    }
                }
            }
        }

        // Now that the grid information has been generated, we can return it all
        return gridCols;
    }
}
