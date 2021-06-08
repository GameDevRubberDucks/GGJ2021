using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG;

public class Grid_Manager : MonoBehaviour
{
    //--- Public Variables ---//
    public int m_gridSize;
    public float m_gridCellSize;
    public float m_gridSpacing;

    [Header("New Grid System")]
    public Grid_Layout m_activeLayout;
    public Transform m_gridCellParent;
    public GameObject m_gridCellObjPrefab;
    public Color m_inactiveCellColour;
    public Color m_activeCellColour;



    //--- Private Variables ---//
    //private List<List<Person>> m_gridCols;
    private List<List<Grid_Cell>> m_gridCols;



    //--- Unity Methods ---//
    private void Awake()
    {
        // Init the private variables
        //m_gridCols = new List<List<Person>>();
        //for (int i = 0; i < m_gridSize; i++)
        //    m_gridCols.Add(new List<Person>());

        m_gridCols = m_activeLayout.GenerateGridCols();
        GenerateGridVisuals();
    }



    //--- Methods ---//
    public void GenerateGridVisuals()
    {
        // TODO: Merge this with the person placement code since they both function on the same logic
        //....

        // Determine the position of the bottom left of the grid so all objects can be placed relative to it
        Vector2 gridParentPos = this.GetComponent<RectTransform>().anchoredPosition;
        float halfGridSize = (m_gridCellSize * m_gridSize / 2.0f) - (m_gridCellSize / 2.0f) + (m_gridSpacing * m_gridSize / 2.0f) - (m_gridSpacing / 2.0f);
        Vector2 spawnedGridBottomLeft = gridParentPos - new Vector2(halfGridSize, halfGridSize);

        // Spawn all of the cell objects into the scene
        for (int col = 0; col < m_gridCols.Count; col++)
        {
            for (int row = 0; row < m_gridCols[col].Count; row++)
            {
                // Spawn in the grid cell object
                var gridCellInfo = m_gridCols[col][row];
                var gridObj = Instantiate(m_gridCellObjPrefab, m_gridCellParent);

                // Change its name and colour so we know if the cell is supposed to be active or not
                bool isActive = (gridCellInfo.CellType == Grid_CellType.Available);
                gridObj.name = "Grid Cell - " + ((isActive) ? "Active" : "Inactive");
                gridObj.GetComponent<Image>().color = (isActive) ? m_activeCellColour : m_inactiveCellColour;

                // Update the cell's position so it matches correctly
                Vector2 posOffset = new Vector2(col * (m_gridCellSize + m_gridSpacing), row * (m_gridCellSize + m_gridSpacing));
                Vector2 finalPosition = spawnedGridBottomLeft + posOffset;
                gridObj.GetComponent<RectTransform>().anchoredPosition = finalPosition;

                // Adjust the person's width and height so it matches the grid cell size
                gridObj.GetComponent<RectTransform>().sizeDelta = new Vector2(m_gridCellSize, m_gridCellSize);
            }
        }
    }

    public int GetNumEmptyGridLocations()
    {
        //// Count all of the empty spaces by checking how full the rows are
        //int numEmptySpaces = 0;
        //foreach (var gridCol in m_gridCols)
        //    numEmptySpaces += (m_gridSize - gridCol.Count);

        //return numEmptySpaces;

        // Count all of the empty, eligible spaces where people can still be placed 
        int numEmptySpaces = 0;
        foreach(var gridCol in m_gridCols)
        {
            foreach(var gridCell in gridCol)
            {
                // If the cell is active but unfilled, count it
                // Do not count it if it is inactive OR if there is a person already in it
                numEmptySpaces += (gridCell.IsOpen) ? 1 : 0;
            }
        }
        return numEmptySpaces;
    }

    public bool PlaceOnGrid(Person _person)
    {
        // If there are no empty spaces, just back out
        if (GetNumEmptyGridLocations() == 0)
            return false;

        //// Otherwise, find all of the columns that have a spot so we can randomly select one
        //List<int> colsWithSpace = new List<int>();
        //for(int i = 0; i < m_gridCols.Count; i++)
        //{
        //    if (m_gridCols[i].Count < m_gridSize)
        //        colsWithSpace.Add(i);
        //}

        //// Randomly select one
        //int randomColIndex = colsWithSpace[Random.Range(0, colsWithSpace.Count)];

        //// Add the person to the list
        //m_gridCols[randomColIndex].Add(_person);

        //// Return true since it worked
        //return true;



        // Otherwise, find all of the columns that have at least one empty spot so we can randomly select one
        List<int> colsWithSpace = new List<int>();
        for (int i = 0; i < m_gridCols.Count; i++)
        {
            // Check if any of the cells in the column are empty. If they are, add the column to the list
            // NOTE: Should be able to check only the first active cell, since that would be the topmost usable cell in the column. Come back and change this later to see if it will work
            foreach (var cell in m_gridCols[i])
            {
                if (cell.IsOpen)
                {
                    colsWithSpace.Add(i);
                    break;
                }
            }
        }

        // Randomly select a column
        int randomColIndex = colsWithSpace[Random.Range(0, colsWithSpace.Count)];
        List<Grid_Cell> selectedCol = m_gridCols[randomColIndex];

        // Now, find the lowest open spot in that column, as this is where the person should be placed
        for (int i = selectedCol.Count - 1; i >= 0; i--)
        {
            // Since we are iterating backwards, the first open cell we come across is the bottom one
            if (selectedCol[i].IsOpen)
            {
                // Now, actually place the person into that cell
                selectedCol[i].Person = _person;

                // Return true to indicate that the insertion succeeded
                return true;
            }
        }

        // If there were no open cells in the column, return false to indicate that the insertion failed
        return false;
    }

    public bool PlaceAllOnGrid(List<Person> _people)
    {
        // Add all of the people
        foreach(var person in _people)
        {
            if (!this.PlaceOnGrid(person))
                return false;
        }

        // Update all of the grid positions
        UpdateGridPlacements();

        // Return true since it worked
        return true;
    }
     
    public bool RemoveFromGrid(Person _person)
    {
        //// Search the lists and find which one contains the person
        //List<Person> containingCol = null;
        //foreach(var gridCol in m_gridCols)
        //{
        //    if (gridCol.Contains(_person))
        //    {
        //        containingCol = gridCol;
        //        break;
        //    }
        //}

        //// If none of the columns had the person, return false to say it failed
        //if (containingCol == null)
        //    return false;

        //// Remove the person from the column
        //containingCol.Remove(_person);

        //// Return true to say it worked
        //return true;



        // Search the columns and find which one contains the person
        foreach(var gridCol in m_gridCols)
        {
            foreach(var gridCell in gridCol)
            {
                // If we found the right cell, unlist the person and back out
                if (gridCell.Person == _person)
                {
                    gridCell.Person = null;
                    return true;
                }
            }
        }

        // If none of the cells had the person, return false to say it failed
        return false;
    }

    public bool RemoveAllFromGrid(List<Person> _people)
    {
        // Remove them all from the grid
        foreach(var person in _people)
        {
            if (!this.RemoveFromGrid(person))
                return false;
        }

        // Update all of the grid positions
        UpdateGridPlacements();

        // Return true to say it worked
        return true;
    }

    public void ShufflePeopleDown()
    {
        // Shuffle everyone one column at a time
        foreach(var col in m_gridCols)
        {
            // Start from the bottom of the column and slowly slide people down if there is an empty gap below
            //for(int thisCellIdx = col.Count - 1; thisCellIdx >= 0; thisCellIdx--) 
            for (int thisCellIdx = 0; thisCellIdx < col.Count; thisCellIdx++)
            {
                // Only try to slide the person if the cell is actually active and currently holds someone
                var thisCellObj = col[thisCellIdx];
                if (thisCellObj.CellType == Grid_CellType.Available && thisCellObj.Person != null)
                {
                    // If there is in fact someone in the cell, we need to search below it to see if there is a new slot to move into
                    // Start from the bottom of the column and work our way up. The first open slot we find would be the bottom one
                    //for (int nextCellIdx = col.Count - 1; nextCellIdx > thisCellIdx; nextCellIdx--)
                    for (int nextCellIdx = 0; nextCellIdx < thisCellIdx; nextCellIdx++)
                    {
                        // If the slot is open and active, we can move this person down into it and then move to the next cell
                        var nextCellObj = col[nextCellIdx];
                        if (nextCellObj.IsOpen)
                        {
                            // Move the person from this cell to the next one
                            nextCellObj.Person = thisCellObj.Person;
                            thisCellObj.Person = null;
                        }
                    }
                }
                
            }
        }
    }

    public void UpdateGridPlacements()
    {
        ShufflePeopleDown();

        // Determine the position of the bottom left of the grid so all objects can be placed relative to it
        Vector2 gridParentPos = this.GetComponent<RectTransform>().anchoredPosition;
        float halfGridSize = (m_gridCellSize * m_gridSize / 2.0f) - (m_gridCellSize / 2.0f) + (m_gridSpacing * m_gridSize / 2.0f) - (m_gridSpacing / 2.0f);
        Vector2 spawnedGridBottomLeft = gridParentPos - new Vector2(halfGridSize, halfGridSize);

        //// Loop through all of the people and place them at the correct position in the world
        //for (int col = 0; col < m_gridCols.Count; col++)
        //{
        //    for(int row = 0; row < m_gridCols[col].Count; row++)
        //    {
        //        // Grab the ref to the person
        //        var person = m_gridCols[col][row];

        //        // Change its name so we know which col and row it is
        //        person.name = "Person (Col: " + col.ToString() + " Row: " + row.ToString() + ")";

        //        // Apply the grid location to the person so it has it in the data
        //        person.GetDescriptor().m_gridLoc = new Vector2Int(col, row);

        //        // Update the person's position so it matches correctly
        //        Vector2 posOffset = new Vector2(col * (m_gridCellSize + m_gridSpacing), row * (m_gridCellSize + m_gridSpacing));
        //        Vector2 finalPosition = spawnedGridBottomLeft + posOffset;
        //        //person.GetComponent<RectTransform>().anchoredPosition = finalPosition;
        //        DG.Tweening.DOTweenModuleUI.DOAnchorPos(person.GetComponent<RectTransform>(), finalPosition, 0.5f);

        //        // Adjust the person's width and height so it matches the grid cell size
        //        person.GetComponent<RectTransform>().sizeDelta = new Vector2(m_gridCellSize, m_gridCellSize);
        //    }
        //}

        // Loop through all of the people and place them at the correct position in the world
        for (int col = 0; col < m_gridCols.Count; col++)
        {
            for (int row = 0; row < m_gridCols[col].Count; row++)
            {
                // Grab the ref to the person
                var person = m_gridCols[col][row].Person;

                if (person != null)
                {
                    // Change its name so we know which col and row it is
                    person.name = "Person (Col: " + col.ToString() + " Row: " + row.ToString() + ")";

                    // Apply the grid location to the person so it has it in the data
                    person.GetDescriptor().m_gridLoc = new Vector2Int(col, row);

                    // Update the person's position so it matches correctly
                    Vector2 posOffset = new Vector2(col * (m_gridCellSize + m_gridSpacing), row * (m_gridCellSize + m_gridSpacing));
                    Vector2 finalPosition = spawnedGridBottomLeft + posOffset;
                    //person.GetComponent<RectTransform>().anchoredPosition = finalPosition;
                    DG.Tweening.DOTweenModuleUI.DOAnchorPos(person.GetComponent<RectTransform>(), finalPosition, 0.5f);

                    // Adjust the person's width and height so it matches the grid cell size
                    person.GetComponent<RectTransform>().sizeDelta = new Vector2(m_gridCellSize, m_gridCellSize);
                }
            }
        }
    }
}
