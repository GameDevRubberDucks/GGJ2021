using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct Person_TraitDesc
{
    public string m_name;
    public Sprite[] m_imgs;
}

public class Person_Generator : MonoBehaviour
{
    //--- Public Variables ---//
    public List<Person_TraitDesc> m_traitDescs = new List<Person_TraitDesc>((int)Person_Trait.Num_Traits);



    //--- Private Variables ---//
    private Person_Descriptor m_targetPersonDesc;



    //--- Public Methods ---//
    public void GenerateTargetPerson()
    {
        // Randomly generate the target's description
        // This is the information used to describe the final target and therefore can only exist ONCE
        // No other generated people can have the exact same set of traits
        m_targetPersonDesc = RandomlyGenerate();
    }

    public Person_Descriptor GenerateNewPersonDesc()
    {
        // Randomly generate a new person. However, it cannot be completely equivalent to the target person
        // So, if we happen to generate an identical person, retry until we get it right
        // This is kind of an iffy way to do this since it could *possibly* get stuck here for a while
        // But it is very unlikely that this loop will trigger more than once 
        Person_Descriptor newDesc = new Person_Descriptor();
        do {
            newDesc = RandomlyGenerate();
        }
        while (!m_targetPersonDesc.IsEquivalent(newDesc));

        // Return the newly generated person
        return newDesc;
    }



    //--- Utility Functions ---//
    private Person_Descriptor RandomlyGenerate()
    {
        // Create a new desciptor
        Person_Descriptor newDesc = new Person_Descriptor();

        // Fill the descriptor with random values for each of the traits
        newDesc.m_selectedTraits[(int)Person_Trait.Hairstyle]   = Random.Range(0, m_traitDescs[(int)Person_Trait.Hairstyle].m_imgs.Length);
        newDesc.m_selectedTraits[(int)Person_Trait.Eyes]        = Random.Range(0, m_traitDescs[(int)Person_Trait.Eyes].m_imgs.Length);
        newDesc.m_selectedTraits[(int)Person_Trait.Nose]        = Random.Range(0, m_traitDescs[(int)Person_Trait.Nose].m_imgs.Length);
        newDesc.m_selectedTraits[(int)Person_Trait.Mouth]       = Random.Range(0, m_traitDescs[(int)Person_Trait.Mouth].m_imgs.Length);
        newDesc.m_selectedTraits[(int)Person_Trait.Shirt]       = Random.Range(0, m_traitDescs[(int)Person_Trait.Shirt].m_imgs.Length);

        // Return the generated descriptor
        return newDesc;
    }
}
