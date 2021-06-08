using UnityEngine;
using System.Collections;
public enum Grid_CellType
{
    Available,
    Unavailable,

    Count
}

public class Grid_Cell
{

    private Grid_CellType m_cellType;
    private Grid_Cell m_upperConnection;
    private Grid_Cell m_lowerConnection;
    private Person m_person;

    public Grid_CellType CellType { get => m_cellType; set => m_cellType = value; }
    public Grid_Cell Previous { get => m_upperConnection; set => m_upperConnection = value; }
    public Grid_Cell Next { get => m_lowerConnection; set => m_lowerConnection = value; }
    public Person Person { get => m_person; set => m_person = value; }
    public bool IsOpen { get => CellType == Grid_CellType.Available && Person == null; }
}
