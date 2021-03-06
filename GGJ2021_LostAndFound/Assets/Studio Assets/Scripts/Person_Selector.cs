using UnityEngine;
using System.Collections.Generic;

public class Person_Selector : MonoBehaviour
{
    //--- Public Variables ---//
    public Color m_selectionOkay;
    public Color m_selectionInvalid;



    //--- Private Variables ---//
    private Game_Manager m_gameManager;
    private Game_TempUI m_tempUI;
    private LineRenderer m_selectionLineRenderer;
    private List<Person> m_selectedPeople;
    private Person_Trait m_currentSelectingTrait;
    private int m_currentSelectingVariation;



    //--- Unity Functions ---//
    private void Awake()
    {
        // Init the private variables
        m_gameManager = FindObjectOfType<Game_Manager>();
        m_tempUI = FindObjectOfType<Game_TempUI>();
        m_selectionLineRenderer = GetComponent<LineRenderer>();
        m_selectedPeople = new List<Person>();
        m_currentSelectingVariation = -1;

        // Clear the line renderer by default
        UpdateSelectionLine();
    }

    private void Update()
    {
        // Right click to clear the entire selection
        if (Input.GetMouseButton(1))
            ClearSelection(true);
    }



    //--- Methods ---//
    public void StartNewSelection(Person _firstPerson)
    {
        // Clear any existing selection
        ClearSelection();

        // Based on the current trait we are selecting by (aka: the spinner result),
        // We need to find out what variation this person has. Only people with the same variation will be eligible
        m_currentSelectingVariation = _firstPerson.GetDescriptor().m_selectedTraits[(int)m_currentSelectingTrait].m_variationIndex;

        // Add the person as the first in the list
        AddToSelection(_firstPerson, true);
    }

    public void SubmitSelection()
    {
        // If there is only one person selected, we can't submit so just clear the selection
        // Otherwise, we should send the selection to the game manager to calculate points, lives, etc
        if (m_selectedPeople.Count <= 2)
        {
            ClearSelection(true);
        }
        else
        {
            m_gameManager.HandleChainCompletion(m_selectedPeople);
            DeleteAllInSelection();
        }
    }

    public void ClearSelection(bool _markAsUnselected = false)
    {
        if (_markAsUnselected)
        {
            foreach (var person in m_selectedPeople)
                person.SetSelectionState(Person_SelectedState.Unselected);
        }

        m_selectedPeople.Clear();
        UpdateSelectionLine();
    }

    public void TryToAddSelection(Person _person)
    {
        // If the person is in the chain already, we can't add them to the selection 
        if (m_selectedPeople.Contains(_person))
        {
            // Instead, we should clear the selection all the way to that point
            int indexOfPerson = m_selectedPeople.IndexOf(_person);
            while (m_selectedPeople.Count > (indexOfPerson + 1))
                RemoveFromSelection(m_selectedPeople[m_selectedPeople.Count - 1]);
        }

        // If the person is too far from the end of the chain (ie: not a direct neighbour), they cannot be selected
        else if (!CheckDistanceToChainEnd(_person))
            _person.SetSelectionState(Person_SelectedState.Unselected);

        // If the person does not have the correct matching trait, they cannot be added and are actually ineligible
        else if (!CheckEligibility(_person))
        {
            _person.SetSelectionState(Person_SelectedState.Ineligible);
            _person.PlayWrongSelection();
        }
        // If the person has passed all of the above, they can be added to the selection
        else
        {
            AddToSelection(_person);
            _person.PlayRightSelection();
        }
    }

    public bool IsSelecting()
    {
        return m_selectedPeople.Count > 0;
    }

    public void SetNewSelectingTrait(Person_Trait _newTrait)
    {
        // Update the UI to show what trait we can select with
        m_tempUI.UpdateSelectableTrait(_newTrait);
        m_currentSelectingTrait = _newTrait;
    }

    public void SkipTurn()
    {
        // We can force the turn to skip by submitting an empty chain
        // This should really only be used by the player if the can't find a match
        // This should be *really* rare, but if we don't have it, the game would soft-lock
        m_gameManager.HandleChainCompletion(new List<Person>());
    }



    //--- Utility Functions ---//
    private void AddToSelection(Person _newPerson, bool _firstPerson = false)
    {
        // Add the person and set their selection state accordingly
        m_selectedPeople.Add(_newPerson);
        _newPerson.SetSelectionState(_firstPerson ? Person_SelectedState.Start_Of_Chain : Person_SelectedState.Part_Of_Chain);

        UpdateSelectionLine();
    }

    private void RemoveFromSelection(Person _toRemove)
    {
        // Remove the person and set their selection state accordingly
        m_selectedPeople.Remove(_toRemove);
        _toRemove.SetSelectionState(Person_SelectedState.Unselected);

        UpdateSelectionLine();
    }

    private bool CheckDistanceToChainEnd(Person _person)
    {
        // Get the person at the end of the chain and find out where they are in the grid
        Person lastPerson = m_selectedPeople[m_selectedPeople.Count - 1];
        Vector2 lastPersonGridLoc = lastPerson.GetDescriptor().m_gridLoc;

        // Do the same for the new person
        Vector2 newPersonGridLoc = _person.GetDescriptor().m_gridLoc;

        // Check if the two grid locations are within one cell of eachother
        if (Mathf.Abs((newPersonGridLoc.x - lastPersonGridLoc.x)) <= 1)
        {
            if (Mathf.Abs((newPersonGridLoc.y - lastPersonGridLoc.y)) <= 1)
            {
                // Return true if both axes are within one cell of eachother
                return true;
            }
        }

        // Return false if they are too far apart
        return false;
    }

    private bool CheckEligibility(Person _person)
    {
        // If the person has the exact same variation for the currently selecting trait, we can add them to the chain
        return m_currentSelectingVariation == _person.GetDescriptor().m_selectedTraits[(int)m_currentSelectingTrait].m_variationIndex;
    }

    private void DeleteAllInSelection()
    {
        FindObjectOfType<Grid_Manager>().RemoveAllFromGrid(m_selectedPeople);

        foreach (var person in m_selectedPeople)
        {
            Destroy(person.gameObject, 1.0f);
            person.GetComponentInChildren<Animator>().SetTrigger("Delete");
            person.Playdeleted();
        }

        FindObjectOfType<Person_Generator>().GenerateToFillGrid();

        ClearSelection();
    }

    private void UpdateSelectionLine()
    {
        // If there is only one point, double it to make a little block
        if (m_selectedPeople.Count == 1)
        {
            m_selectionLineRenderer.positionCount = 2;
            m_selectionLineRenderer.SetPosition(0, m_selectedPeople[0].transform.position);
            m_selectionLineRenderer.SetPosition(1, m_selectedPeople[0].transform.position);
        }
        else
        {
            // Update the line renderer to fit the new number of selection points
            m_selectionLineRenderer.positionCount = m_selectedPeople.Count;

            // Set all of the points in the line
            for (int i = 0; i < m_selectedPeople.Count; i++)
                m_selectionLineRenderer.SetPosition(i, m_selectedPeople[i].transform.position);
        }

        // Change the colour to indicate if the selection will work or not
        Color lineColor = (m_selectedPeople.Count > 2) ? m_selectionOkay : m_selectionInvalid;
        m_selectionLineRenderer.startColor = lineColor;
        m_selectionLineRenderer.endColor = lineColor;
    }
}
