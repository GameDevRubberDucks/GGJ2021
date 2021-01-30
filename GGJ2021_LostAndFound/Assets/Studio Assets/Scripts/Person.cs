using UnityEngine;

public class Person : MonoBehaviour
{
    //--- Public Variables ---//
    public SpriteRenderer[] m_traitRenderers = new SpriteRenderer[(int)Person_Trait.Num_Traits];



    //--- Private Variables ---//
    private Person_Descriptor m_descriptor;



    //--- Public Methods ---//
    public void ApplyDescription(Person_Descriptor _descriptor)
    {
        // Store the new descriptor
        this.m_descriptor = _descriptor;

        // Update the visuals
        foreach (var traitInfo in m_descriptor.m_selectedTraits)
            m_traitRenderers[(int)traitInfo.m_trait].sprite = traitInfo.m_variationImg;
    }
}
