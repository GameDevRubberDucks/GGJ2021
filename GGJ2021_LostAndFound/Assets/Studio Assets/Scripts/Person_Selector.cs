using UnityEngine;
using System.Collections.Generic;

public class Person_Selector : MonoBehaviour
{
    //--- Private Variables ---//
    private List<Person> m_selectedPeople;
    [SerializeField]private Person_Trait m_currentSelectingTrait; // TEMP: Serialized for now, will need to be connected to the spinner
    private int m_currentSelectingVariation;



    //--- Unity Functions ---//
    private void Awake()
    {
        // Init the private variables
        m_selectedPeople = new List<Person>();
        m_currentSelectingVariation = -1;
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
        if (m_selectedPeople.Count <= 1)
            ClearSelection(true);
        else
            DeleteAllInSelection();
    }

    public void ClearSelection(bool _markAsUnselected = false)
    {
        if (_markAsUnselected)
        {
            foreach (var person in m_selectedPeople)
                person.SetSelectionState(Person_SelectedState.Unselected);
        }

        m_selectedPeople.Clear();
    }

    public void TryToAddSelection(Person _person)
    {
        // If the person is in the chain already, we can't add them to the selection 
        if (m_selectedPeople.Contains(_person))
        {
            // Instead, if they are the second to last person in the chain, we need to eliminate the current chain end
            // This is because the player is backtracking and trying to undo a bit of their chain
            if (m_selectedPeople.IndexOf(_person) == m_selectedPeople.Count - 2)
                RemoveFromSelection(m_selectedPeople[m_selectedPeople.Count - 1]);
        }

        // If the person is too far from the end of the chain (ie: not a direct neighbour), they cannot be selected
        else if (!CheckDistanceToChainEnd(_person))
            _person.SetSelectionState(Person_SelectedState.Unselected);

        // If the person does not have the correct matching trait, they cannot be added and are actually ineligible
        else if (!CheckEligibility(_person))
            _person.SetSelectionState(Person_SelectedState.Ineligible);

        // If the person has passed all of the above, they can be added to the selection
        else
            AddToSelection(_person);
    }

    public bool IsSelecting()
    {
        return m_selectedPeople.Count > 0;
    }



    //--- Utility Functions ---//
    private void AddToSelection(Person _newPerson, bool _firstPerson = false)
    {
        // Add the person and set their selection state accordingly
        m_selectedPeople.Add(_newPerson);
        _newPerson.SetSelectionState(_firstPerson ? Person_SelectedState.Start_Of_Chain : Person_SelectedState.Part_Of_Chain);
    }

    private void RemoveFromSelection(Person _toRemove)
    {
        // Remove the person and set their selection state accordingly
        m_selectedPeople.Remove(_toRemove);
        _toRemove.SetSelectionState(Person_SelectedState.Unselected);
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
            Destroy(person.gameObject);

        FindObjectOfType<Person_Generator>().GenerateToFillGrid();

        ClearSelection();
    }
}
