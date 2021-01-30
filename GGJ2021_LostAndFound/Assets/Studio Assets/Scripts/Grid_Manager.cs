using UnityEngine;
using System.Collections.Generic;
using DG;

public class Grid_Manager : MonoBehaviour
{
    //--- Public Variables ---//
    public int m_gridSize;
    public float m_gridCellSize;
    public float m_gridSpacing;



    //--- Private Variables ---//
    private List<List<Person>> m_gridCols;



    //--- Unity Methods ---//
    private void Awake()
    {
        // Init the private variables
        m_gridCols = new List<List<Person>>();
        for (int i = 0; i < m_gridSize; i++)
            m_gridCols.Add(new List<Person>());
    }



    //--- Methods ---//
    public int GetNumEmptyGridLocations()
    {
        // Count all of the empty spaces by checking how full the rows are
        int numEmptySpaces = 0;
        foreach (var gridCol in m_gridCols)
            numEmptySpaces += (m_gridSize - gridCol.Count);

        return numEmptySpaces;
    }

    public bool PlaceOnGrid(Person _person)
    {
        // If there are no empty spaces, just back out
        if (GetNumEmptyGridLocations() == 0)
            return false;

        // Otherwise, find all of the columns that have a spot so we can randomly select one
        List<int> colsWithSpace = new List<int>();
        for(int i = 0; i < m_gridCols.Count; i++)
        {
            if (m_gridCols[i].Count < m_gridSize)
                colsWithSpace.Add(i);
        }

        // Randomly select one
        int randomColIndex = colsWithSpace[Random.Range(0, colsWithSpace.Count)];

        // Add the person to the list
        m_gridCols[randomColIndex].Add(_person);

        // Return true since it worked
        return true;
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
        // Search the lists and find which one contains the person
        List<Person> containingCol = null;
        foreach(var gridCol in m_gridCols)
        {
            if (gridCol.Contains(_person))
            {
                containingCol = gridCol;
                break;
            }
        }

        // If none of the columns had the person, return false to say it failed
        if (containingCol == null)
            return false;

        // Remove the person from the column
        containingCol.Remove(_person);

        // Return true to say it worked
        return true;
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

    public void UpdateGridPlacements()
    {
        // Determine the position of the bottom left of the grid so all objects can be placed relative to it
        Vector2 gridParentPos = this.GetComponent<RectTransform>().anchoredPosition;
        float halfGridSize = (m_gridCellSize * m_gridSize / 2.0f) - (m_gridCellSize / 2.0f) + (m_gridSpacing * m_gridSize / 2.0f) - (m_gridSpacing / 2.0f);
        Vector2 spawnedGridBottomLeft = gridParentPos - new Vector2(halfGridSize, halfGridSize);

        // Loop through all of the people and place them at the correct position in the world
        for (int col = 0; col < m_gridCols.Count; col++)
        {
            for(int row = 0; row < m_gridCols[col].Count; row++)
            {
                // Grab the ref to the person
                var person = m_gridCols[col][row];

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
