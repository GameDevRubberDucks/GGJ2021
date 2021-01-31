using System.Text;
using UnityEngine;

public enum Person_Trait
{
    Hairstyle,
    Eyes,
    Nose,
    Mouth,
    Shirt,

    Num_Traits
}

public struct Person_TraitImg
{
    public Person_Trait m_trait;
    public int m_variationIndex;
    public Sprite m_variationImg;
}

public class Person_Descriptor
{
    //--- Public Variables ---//
    public Person_TraitImg[] m_selectedTraits;
    public Vector2Int m_gridLoc;
    public bool m_isFinalTarget;



    //--- Constructors ---//
    public Person_Descriptor()
    {
        m_selectedTraits = new Person_TraitImg[(int)Person_Trait.Num_Traits];
        m_gridLoc = Vector2Int.zero;
        m_isFinalTarget = false;
    }

    

    //--- Methods ---//
    public bool IsEquivalent(Person_Descriptor _other)
    {
        // If one of the traits doesn't match, return false since they are not completely equivalent
        for (int i = 0; i < m_selectedTraits.Length; i++)
        {
            if (m_selectedTraits[i].m_variationIndex != _other.m_selectedTraits[i].m_variationIndex)
                return false;
        }
        
        // If there are no differences at all, return true since they are completely equivalent
        return true;
    }

    public override string ToString()
    {
        StringBuilder str = new StringBuilder();

        foreach (var trait in m_selectedTraits)
            str.Append(trait.m_trait.ToString() + ": " + trait.m_variationIndex.ToString() + "   ");

        return str.ToString();
    }
}
