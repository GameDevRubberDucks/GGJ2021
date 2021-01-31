using UnityEngine;
using System.Collections.Generic;

public class Game_Manager : MonoBehaviour
{
    //--- Public Variables ---//
    [Header("Game Settings")]
    public int m_numStartingHearts;
    public int m_peopleUntilExtraHeart;
    public int m_peopleUntilTraitComplete;
    public int m_scorePerPerson;
    public float m_targetVariationMultiplier;



    //--- Private Variables ---//
    private Person_Generator m_personGenerator;
    private Person_Selector m_personSelector;
    private SlotWheelManager m_spinner;
    private Person_Trait m_targetTrait;
    private bool[] m_completedTraits;
    private int m_currentScore;
    private int m_currentHeartCount;
    private int m_progressTowardsNextHeart;
    private int m_progressTowardsTargetTrait;



    //--- Unity Functions ---//
    private void Awake()
    {
        // Init the private variables
        m_personGenerator = FindObjectOfType<Person_Generator>();
        m_personSelector = FindObjectOfType<Person_Selector>();
        m_spinner = FindObjectOfType<SlotWheelManager>();
        m_completedTraits = new bool[(int)Person_Trait.Num_Traits];
        m_currentScore = 0;
        m_currentHeartCount = m_numStartingHearts;
        m_progressTowardsNextHeart = 0;
        m_progressTowardsTargetTrait = 0;
    }

    private void Start()
    {
        StartGame();
    }



    //--- Methods ---//
    public void StartGame()
    {
        // Set up the person generator
        m_personGenerator.GenerateTargetPerson();
        m_personGenerator.GenerateToFillGrid();

        // Spin the wheel to determine the first trait to select by
        m_spinner.resetSpinner();
        SpinWheelForNewTrait(true);
    }

    public void SpinWheelForNewTrait(bool _saveAsNewTargetTrait)
    {
        // Spin the wheel and get its result
        m_spinner.Spin();
        var spinnerResult = m_spinner.getResult();

        // Send the trait to the selection system
        m_personSelector.SetNewSelectingTrait(spinnerResult);

        // If this is the start of a new target trait, we also need to save it there
        if (_saveAsNewTargetTrait)
            m_targetTrait = spinnerResult;
    }

    public void HandleChainCompletion(List<Person> _chain)
    {
        // If the current target trait is still on the spinner (ie: that was the first turn of the new trait), we should take it off
        m_spinner.deleteTrait(m_targetTrait);

        // We need to calculate and add the score
        CalculateChainScore(_chain);

        // We need to determine the number of lives leftover
        CalculateLives(_chain);

        // We need to look for progress towards the target trait
        CalculateTargetTraitProgress(_chain);

        // Finally, we need to check if the chain contains the final target. If so, the game is over
        if (CheckForFinalTarget(_chain))
        {
            EndGame(true);
            return;
        }

        // Spin the wheel for the next trait
        SpinWheelForNewTrait(false);
    }

    public void CalculateChainScore(List<Person> _chain)
    {
        // For every person in the chain, we give X points
        int chainScore = _chain.Count * m_scorePerPerson;

        // If there is anyone in the chain that has the variation we are looking for, apply the multiplier
        // Note: If there are multiple people, the multiplier gets applied multiple times
        foreach (var person in _chain)
        {
            if (CheckForTargetTrait(person))
                chainScore = Mathf.RoundToInt(chainScore * m_targetVariationMultiplier);
        }

        // Update the score
        m_currentScore += chainScore;

        // TODO: Update the UI
        // ...
    }

    public void CalculateLives(List<Person> _chain)
    {
        // Keep track of the number of people the player has cleared and give an extra life if the right amount has been reached
        m_progressTowardsNextHeart += _chain.Count;
        if (m_progressTowardsNextHeart >= m_peopleUntilExtraHeart)
        {
            // Increase the number of lives and then save the remainder over for the next one
            m_currentHeartCount += m_progressTowardsNextHeart / m_peopleUntilExtraHeart;
            m_progressTowardsNextHeart = m_progressTowardsNextHeart % m_peopleUntilExtraHeart;
        }

        // Check to see if anyone in the chain has the target trait
        bool loseLife = true;
        foreach (var person in _chain)
        {
            if (CheckForTargetTrait(person))
            {
                loseLife = false;
                break;
            }
        }

        // If nobody has the trait, we should lose one life
        if (loseLife)
            m_currentHeartCount--;

        // TODO: Update the UI
        // ...

        // If we have run out of lives, we should end the game
        if (m_currentHeartCount <= 0)
            EndGame(false);
    }

    public void CalculateTargetTraitProgress(List<Person> _chain)
    {
        // Find out how many people in the chain posess the target trait
        int numTargets = 0;
        foreach(var person in _chain)
        {
            if (CheckForTargetTrait(person))
                numTargets++;
        }

        // Increase the progress towards the trait
        m_progressTowardsTargetTrait += numTargets;

        // If we have reached the end of the trait progress, we can move to the next trait
        if (m_progressTowardsTargetTrait >= m_peopleUntilTraitComplete)
        {
            // Mark the trait as completed
            m_completedTraits[(int)m_targetTrait] = true;

            // TODO: Show feedback for completing the trait
            // ...

            // Reset the progress towards the next trait
            m_progressTowardsTargetTrait = 0;

            // Check to see if all of the traits have been completed
            bool allTraitsDone = true;
            foreach(var traitComplete in m_completedTraits)
            {
                if (!traitComplete)
                {
                    allTraitsDone = false;
                    break;
                }
            }

            // If all of the traits are done, we need to spawn the final target in the next wave of people
            // Also, we will need to reset the spinner to the full list. Otherwise, we need to set the spinner to determine the next target trait
            if (allTraitsDone)
            {
                m_personGenerator.SpawnFinalTarget();
                m_spinner.resetSpinner();
            }
            else
            {
                SpinWheelForNewTrait(true);
            }
        }
    }

    public bool CheckForFinalTarget(List<Person> _chain)
    {
        // If anyone in the list is the final target, return true
        foreach(var person in _chain)
        {
            if (person.GetDescriptor().m_isFinalTarget)
                return true;
        }

        // Otherwise, return false since none of the people are the final target
        return false;
    }

    public void EndGame(bool _victory)
    {
        if (_victory)
            Debug.Log("YOU WON");
        else
            Debug.Log("YOU LOST");
    }



    //--- Utility Functions ---//
    private bool CheckForTargetTrait(Person _person)
    {
        // Determine the target variation
        var targetDesc = m_personGenerator.GetTargetPersonDesc();
        int targetTraitVariation = targetDesc.m_selectedTraits[(int)m_targetTrait].m_variationIndex;

        // Determine the variation on the person in question
        var personDesc = _person.GetDescriptor();
        int personTraitVariation = personDesc.m_selectedTraits[(int)m_targetTrait].m_variationIndex;

        // If they match, return true. If they don't match, return false
        return (personTraitVariation == targetTraitVariation);
    }
}
