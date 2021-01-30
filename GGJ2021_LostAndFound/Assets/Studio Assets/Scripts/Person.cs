using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum Person_SelectedState
{
    Unselected,
    Highlighted,

    Start_Of_Chain,
    Part_Of_Chain,

    Ineligible,

    Num_States
}

public class Person : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    //--- Public Variables ---//
    public Image[] m_traitRenderers = new Image[(int)Person_Trait.Num_Traits];
    public Image m_selectionStateIndicator;
    public Color[] m_indicatorColours = new Color[(int)Person_SelectedState.Num_States];



    //--- Private Variables ---//
    private Person_Descriptor m_descriptor;
    private Person_Selector m_selector;
    private Person_SelectedState m_selectedState;



    //--- Unity Functions ---//
    private void Awake()
    {
        // Init the private variables
        m_selector = FindObjectOfType<Person_Selector>();
        m_selectedState = Person_SelectedState.Unselected;
    }



    //--- Methods ---//
    public void ApplyDescription(Person_Descriptor _descriptor)
    {
        // Store the new descriptor
        this.m_descriptor = _descriptor;

        // Update the visuals
        foreach (var traitInfo in m_descriptor.m_selectedTraits)
            m_traitRenderers[(int)traitInfo.m_trait].sprite = traitInfo.m_variationImg;
    }

    public void SetSelectionState(Person_SelectedState _state)
    {
        m_selectedState = _state;
        m_selectionStateIndicator.color = m_indicatorColours[(int)m_selectedState];
    }



    //--- Mouse Handling Interfaces ---//
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Try to add this person to the selection when dragging over it, assuming there is actually a selection happening
        // Otherwise, just highlight it
        if (m_selector.IsSelecting())
            m_selector.TryToAddSelection(this);
        else
            SetSelectionState(Person_SelectedState.Highlighted);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // If not selected, just revert to the standard non-highlighted state
        // Otherwise, stay selected
        if (m_selectedState == Person_SelectedState.Highlighted || m_selectedState == Person_SelectedState.Ineligible)
            SetSelectionState(Person_SelectedState.Unselected);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Begin a new selection by clicking, with this person as the first element
        m_selector.StartNewSelection(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Submit the selection to the game systems when releasing the mouse button
        m_selector.SubmitSelection();
    }



    //--- Getters ---//
    public Person_Descriptor GetDescriptor() { return m_descriptor; }
}
