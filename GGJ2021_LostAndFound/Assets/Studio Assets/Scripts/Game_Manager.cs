using UnityEngine;
using System.Collections.Generic;

public class Game_Manager : MonoBehaviour
{
    //--- Public Variables ---//
    [Header("Game Settings")]
    public int m_numStartingHearts;
    public int m_peopleUntilExtraHeart;
    public int m_maxHearts;
    public int m_peopleUntilTraitComplete;
    public int m_scorePerPerson;
    public float m_targetVariationMultiplier;



    //--- Private Variables ---//
    private Game_TempUI m_tempUI;
    private Person_Generator m_personGenerator;
    private Person_Selector m_personSelector;
    private SlotWheelManager m_spinner;
    private Person_Trait m_targetTrait;
    private int[] m_traitProgresses;
    private int m_currentScore;
    private int m_currentHeartCount;
    private int m_progressTowardsNextHeart;



    //--- Unity Functions ---//
    private void Awake()
    {
        // Init the private variables
        m_tempUI = FindObjectOfType<Game_TempUI>();
        m_personGenerator = FindObjectOfType<Person_Generator>();
        m_personSelector = FindObjectOfType<Person_Selector>();
        m_spinner = FindObjectOfType<SlotWheelManager>();
        m_traitProgresses = new int[(int)Person_Trait.Num_Traits];
        m_currentScore = 0;
        m_currentHeartCount = m_numStartingHearts;
        m_progressTowardsNextHeart = 0;
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
        SpinForNewTargetTrait();

        // Initialize all the UI to its default value
        InitUI();
    }

    public void InitUI()
    {
        // Initialize all of the UI elements to show the base values
        m_tempUI.UpdateScoreUI(m_currentScore);
        m_tempUI.UpdateHeartCountUI(m_currentHeartCount, m_progressTowardsNextHeart, m_peopleUntilExtraHeart);
        m_tempUI.UpdateTraitProgress(m_traitProgresses, m_peopleUntilTraitComplete);
        UpdateMugshotUI();
    }

    public void SpinForNewTargetTrait()
    {
        // Reset the spinner to get everything back onto it
        m_spinner.resetSpinner();

        // Delete all of the completed traits
        for (int i = 0; i < m_traitProgresses.Length; i++)
        {
            if (m_traitProgresses[i] >= m_peopleUntilTraitComplete)
                m_spinner.deleteTrait((Person_Trait)i);
        }

        // Spin the wheel and store the result
        var result = SpinWheel();

        // Set the new target trait
        m_targetTrait = result;

        // The trait should also be the new selection trait
        m_personSelector.SetNewSelectingTrait(result);

        // Update the UI to show the target trait and variation
        //var targetDesc = m_personGenerator.GetTargetPersonDesc();
        //var targetVariationImg = targetDesc.m_selectedTraits[(int)m_targetTrait].m_variationImg;
        //m_tempUI.UpdateTargetVariation(targetVariationImg, m_personGenerator.m_possibleCharColours[targetDesc.m_selectedTraits[(int)m_targetTrait].m_variationIndex]);
    }

    public void SpinForNewSelectionTrait()
    {
        // Reset the spinner to get everything back onto it
        m_spinner.resetSpinner();

        // Delete only the current target trait
        m_spinner.deleteTrait(m_targetTrait);

        // Spin the wheel and store the result
        var result = SpinWheel();

        // Set the new trait to select by
        m_personSelector.SetNewSelectingTrait(result);
    }

    public Person_Trait SpinWheel()
    {
        // Spin the wheel and get the result
        m_spinner.Spin();
        return m_spinner.getResult();
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
        bool targetTraitComplete = CalculateTargetTraitProgress(_chain);

        // Finally, we need to check if the chain contains the final target. If so, the game is over
        if (CheckForFinalTarget(_chain))
        {
            EndGame(true);
            return;
        }

        // Setup and fire the spinner with the correct number of traits on it
        if (CheckIfAllTraitsAreComplete())
        {
            // If all traits are done, the spinner should have everything on it as the player tries to finish
            m_spinner.resetSpinner();
            var result = SpinWheel();
            m_personSelector.SetNewSelectingTrait(result);
        }
        else
        {
            // Spin the wheel to determine either the next target trait or just the next selection  
            if (targetTraitComplete)
                SpinForNewTargetTrait();
            else
                SpinForNewSelectionTrait();
        }

        // Update the mugshot UI to show any changes to the traits
        UpdateMugshotUI();
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

        // Update the UI
        m_tempUI.UpdateScoreUI(m_currentScore);
    }

    public void CalculateLives(List<Person> _chain)
    {
        // Keep track of the number of people the player has cleared and give an extra life if the right amount has been reached
        // Once the heart limit is reached, we should cap their progress towards another one
        if (m_currentHeartCount < m_maxHearts)
        {
            // Increase the progress
            m_progressTowardsNextHeart += _chain.Count;

            // If the threshold has been reached, move to the next heart
            if (m_progressTowardsNextHeart >= m_peopleUntilExtraHeart)
            {
                // Increase the number of lives and then save the remainder over for the next one
                m_currentHeartCount += m_progressTowardsNextHeart / m_peopleUntilExtraHeart;

                // If we are JUST NOW over the heart limit, we should cap progress at 0
                // Otherwise, we should save the remainder over for the next one
                m_progressTowardsNextHeart = (m_currentHeartCount >= m_maxHearts) ? 0 : m_progressTowardsNextHeart % m_peopleUntilExtraHeart;
            }
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

        // Update the UI
        m_tempUI.UpdateHeartCountUI(m_currentHeartCount, m_progressTowardsNextHeart, m_peopleUntilExtraHeart);

        // If we have run out of lives, we should end the game
        if (m_currentHeartCount <= 0)
            EndGame(false);
    }

    public bool CalculateTargetTraitProgress(List<Person> _chain)
    {
        // Find out how many people in the chain posess the target trait
        int numTargets = 0;
        foreach(var person in _chain)
        {
            if (CheckForTargetTrait(person))
                numTargets++;
        }

        // Increase the progress towards the trait
        int targetTraitIndex = (int)m_targetTrait;
        m_traitProgresses[targetTraitIndex] += numTargets;

        // Determine if the trait was completed or not
        bool traitCompleted = false;

        // If we have reached the end of the trait progress, we can move to the next trait
        if (m_traitProgresses[targetTraitIndex] >= m_peopleUntilTraitComplete)
        {
            // The trait was completed so we should select a new target trait
            traitCompleted = true;

            // Cap the trait so it doesn't go above the max amount
            m_traitProgresses[targetTraitIndex] = m_peopleUntilTraitComplete;

            // TODO: Show feedback for completing the trait
            // ...

            // If all of the traits are done, we need to spawn the final target in the next wave of people
            // Also, we will need to reset the spinner to the full list. Otherwise, we need to set the spinner to determine the next target trait
            if (CheckIfAllTraitsAreComplete())
                m_personGenerator.SpawnFinalTarget();
        }

        // Update the UI
        m_tempUI.UpdateTraitProgress(m_traitProgresses, m_peopleUntilTraitComplete);

        // Return if the target trait was completed on this turn
        return traitCompleted;
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
        m_tempUI.ShowEndScreen(_victory);
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

    private bool CheckIfAllTraitsAreComplete()
    {
        // Check to see if all traits have passed the required threshold
        // If one of them fails, just return false
        foreach (var traitProgress in m_traitProgresses)
        {
            if (traitProgress < m_peopleUntilTraitComplete)
                return false;
        }

        // If we got here, then all the traits are finished so return true
        return true;
    }

    private void UpdateMugshotUI()
    {
        // Get the target description so we can use all of its information in the mughost
        var targetDesc = m_personGenerator.GetTargetPersonDesc();

        // Get all of the necessary sprites
        Sprite[] spriteList = new Sprite[(int)Person_Trait.Num_Traits];
        for (int i = 0; i < spriteList.Length; i++)
            spriteList[i] = targetDesc.m_selectedTraits[i].m_variationImg;

        // Determine all of the necessary colours
        Color[] colorList = new Color[(int)Person_Trait.Num_Traits];
        for (int i = 0; i < colorList.Length; i++)
            colorList[i] = m_personGenerator.m_possibleCharColours[targetDesc.m_selectedTraits[i].m_variationIndex];

        // Determine which traits have been completed
        bool[] completedTraitList = new bool[(int)Person_Trait.Num_Traits];
        for (int i = 0; i < completedTraitList.Length; i++)
            completedTraitList[i] = m_traitProgresses[i] >= m_peopleUntilTraitComplete;

        // Determine the target trait
        Person_Trait targetTrait = m_targetTrait;

        // Determine if all of the traits are filled and we are in the final target state
        bool allTraitsDone = CheckIfAllTraitsAreComplete();

        // Update the mugshot UI with all of the information
        m_tempUI.UpdateTargetVariations(spriteList, colorList, completedTraitList, targetTrait, allTraitsDone);
    }
}
