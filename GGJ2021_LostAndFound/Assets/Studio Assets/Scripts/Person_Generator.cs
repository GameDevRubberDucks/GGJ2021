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
    public Color[] m_possibleCharColours;
    public List<Person_TraitVariations> m_traitDescs = new List<Person_TraitVariations>((int)Person_Trait.Num_Traits);



    //--- Private Variables ---//
    private Person_Descriptor m_targetPersonDesc;
    private bool m_shouldSpawnTarget;
    private bool m_hasAlreadySpawnedTarget;



    //--- Unity Methods ---//
    private void Awake()
    {
        // Init the private variables
        m_shouldSpawnTarget = false;
        m_hasAlreadySpawnedTarget = false;
    }



    //--- Methods ---//
    public void GenerateTargetPerson()
    {
        // Randomly generate the target's description
        // This is the information used to describe the final target and therefore can only exist ONCE
        // No other generated people can have the exact same set of traits
        m_targetPersonDesc = RandomlyGenerate();
        m_targetPersonDesc.m_isFinalTarget = true;
    }

    public Person GenerateNewPerson(bool _makeFinalTarget = false)
    {
        // Either use the final target descriptor or just use a blank one
        Person_Descriptor newDesc = null;

        // Determine the traits that will make up this person
        if (_makeFinalTarget)
        {
            // If we are creating the final target, just use their traits
            newDesc = m_targetPersonDesc;
            m_hasAlreadySpawnedTarget = true;
        }
        else
        {
            // If not using the target, randomly generate a new person. However, it cannot be completely equivalent to the target person
            // So, if we happen to generate an identical person, retry until we get it right
            // This is kind of an iffy way to do this since it could *possibly* get stuck here for a couple of loop iterations
            // But it is very unlikely that this loop will trigger more than once 
            do {
                newDesc = RandomlyGenerate();
            }
            while (m_targetPersonDesc.IsEquivalent(newDesc));
        }

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

        // If we need to spawn the final target, we should randomly place them into the list of new people
        // Otherwise, just spawn people as normal
        if (m_shouldSpawnTarget)
        {
            // Randomly select the placement of the final person
            int finalTargetIndex = Random.Range(0, numSlots);

            // Fill in the spots before the target with randomly generated people
            for (int i = 0; i < finalTargetIndex; i++)
                newPeople.Add(GenerateNewPerson(false));

            // Place the target
            newPeople.Add(GenerateNewPerson(true));

            // Fill in the remaining spots after with more randomly generated people
            for (int i = finalTargetIndex + 1; i < numSlots; i++)
                newPeople.Add(GenerateNewPerson(false));

            // Reset the flag
            m_shouldSpawnTarget = false;
        }
        else
        {
            // If not making the final target on this one, just randomize everyone
            for (int i = 0; i < numSlots; i++)
                newPeople.Add(GenerateNewPerson());
        }

        // Add all of them to the grid
        m_gridManager.PlaceAllOnGrid(newPeople);
    }

    public void SpawnFinalTarget()
    {
        // Set the flag so when we next generate people, we include the final target
        // Don't set the flag if the target has already spawned though, as we only want them once
        if (!m_hasAlreadySpawnedTarget)
            m_shouldSpawnTarget = true;
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

        // Randomly select a colour
        newDesc.m_colour = m_possibleCharColours[Random.Range(0, m_possibleCharColours.Length)];
       
        // Return the generated descriptor
        return newDesc;
    }



    //--- Getters ---//
    public Person_Descriptor GetTargetPersonDesc() { return m_targetPersonDesc; }
}
