using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct Person_TraitVariations
{
    public Person_Trait m_trait;
    public Sprite[] m_variationImgs;
}

public class Person_Generator : MonoBehaviour
{
    //--- Public Variables ---//
    public GameObject m_personPrefab;
    public Grid_Manager m_gridManager;
    public List<Person_TraitVariations> m_traitDescs = new List<Person_TraitVariations>((int)Person_Trait.Num_Traits);



    //--- Private Variables ---//
    private Person_Descriptor m_targetPersonDesc;



    //--- Unity Methods ---//
    private void Start()
    {
        // Generate the target person for this round
        // This is the person that nobody else can look exactly like
        GenerateTargetPerson();
        Debug.Log("Target Person:\n" + m_targetPersonDesc.ToString());

        // Spawn all of the necessary people to fill the grid
        GenerateToFillGrid();
    }

    private void Update()
    {
        // TEMP: Spawn a new person by pressing space
        if (Input.GetKeyDown(KeyCode.Space))
            GenerateNewPerson();
    }



    //--- Methods ---//
    public void GenerateTargetPerson()
    {
        // Randomly generate the target's description
        // This is the information used to describe the final target and therefore can only exist ONCE
        // No other generated people can have the exact same set of traits
        m_targetPersonDesc = RandomlyGenerate();
    }

    public Person GenerateNewPerson()
    {
        // Randomly generate a new person. However, it cannot be completely equivalent to the target person
        // So, if we happen to generate an identical person, retry until we get it right
        // This is kind of an iffy way to do this since it could *possibly* get stuck here for a while
        // But it is very unlikely that this loop will trigger more than once 
        Person_Descriptor newDesc = new Person_Descriptor();
        do {
            newDesc = RandomlyGenerate();
        }
        while (m_targetPersonDesc.IsEquivalent(newDesc));

        // Spawn a new person
        Person newPerson = Instantiate(m_personPrefab, m_gridManager.transform).GetComponent<Person>();
        newPerson.ApplyDescription(newDesc);

        // Return the person
        return newPerson;
    }

    public void GenerateToFillGrid()
    {
        // Find out how many slots are empty within the grid
        int numSlots = m_gridManager.GetNumEmptyGridLocations();

        // Generate enough people to fill every one of the slots
        List<Person> newPeople = new List<Person>();
        for (int i = 0; i < numSlots; i++)
            newPeople.Add(GenerateNewPerson());

        // Add all of them to the grid
        m_gridManager.PlaceAllOnGrid(newPeople);
    }



    //--- Utility Functions ---//
    private Person_Descriptor RandomlyGenerate()
    {
        // Create a new desciptor
        Person_Descriptor newDesc = new Person_Descriptor();

        // Fill the descriptor with random values for each of the traits
        for (int traitIndex = 0; traitIndex < (int)Person_Trait.Num_Traits; traitIndex++)
        {
            // Randomly select a variation of the trait from within the list of image variations
            int randomVariationIndex = Random.Range(0, m_traitDescs[traitIndex].m_variationImgs.Length);
            Sprite randomVariationImg = m_traitDescs[traitIndex].m_variationImgs[randomVariationIndex];

            // Assign the image to the trait in the descriptor
            newDesc.m_selectedTraits[traitIndex].m_trait = (Person_Trait)traitIndex;
            newDesc.m_selectedTraits[traitIndex].m_variationIndex = randomVariationIndex;
            newDesc.m_selectedTraits[traitIndex].m_variationImg = randomVariationImg;
        }
       
        // Return the generated descriptor
        return newDesc;
    }
}
