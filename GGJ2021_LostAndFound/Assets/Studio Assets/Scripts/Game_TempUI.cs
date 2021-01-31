using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Game_TempUI : MonoBehaviour
{
    [Header("Score")]
    public TextMeshProUGUI m_txtScore;

    [Header("Hearts")]
    public TextMeshProUGUI m_txtProgressTowardsNextHeart;
    public Image m_fillProgressTowardsNextHeart;
    public Image[] heartContainers;

    [Header("Traits")]
    public Image m_imgTargetVariation;
    public TextMeshProUGUI m_selectionTrait;
    public Image m_selectionTraitImage;
    public Sprite[] m_availableTraitImageSelections;
    public TextMeshProUGUI[] m_txtTraitProgress;
    public Image[] m_fillTraitProgress;
    public Image m_mugshotStarIndicator;
    public Image[] m_mugshotTraitIndicators;

    [Header("End Screens")]
    public GameObject m_endScreen;
    public GameObject m_victoryMessage;
    public GameObject m_lossMessage;

    [Header("Spinner")]
    public GameObject m_spinnerCover;
    public float m_spinnerDuration;

    public void UpdateScoreUI(int _newScore)
    {
        m_txtScore.text = _newScore.ToString();
    }

    public void InitHeartCountUI(int _startHeartCount)
    {
        for (int i = 0; i < heartContainers.Length; i++)
        {
            if (i < _startHeartCount)
                heartContainers[i].GetComponent<Animator>().SetTrigger("NewHeart");
        }
    }

    public void ToggleHeart(bool _filled, int _heartID)
    {
        var anim = heartContainers[_heartID].GetComponent<Animator>();
        string triggerName = (_filled) ? "NewHeart" : "LostHeart";
        anim.SetTrigger(triggerName);
    }

    public void UpdateHeartCountUI(int _numHearts, int _progressTowardsNextHeart, int _peopleUntilNextHeart)
    {
        m_txtProgressTowardsNextHeart.text = _progressTowardsNextHeart.ToString() + "\n---\n" + _peopleUntilNextHeart.ToString();
        m_fillProgressTowardsNextHeart.fillAmount = (float)_progressTowardsNextHeart / (float)_peopleUntilNextHeart;

        //fill in the hearts based on the heart count
        for (int i = 0; i < heartContainers.Length; i++)
        {
            if (i < _numHearts)
                //filled color
                heartContainers[i].color = new Color(255.0f / 255.0f, 130.0f / 255.0f, 124.0f / 255.0f);
            else
                //empty color
                heartContainers[i].color = new Color(77.0f / 255.0f, 59.0f / 255.0f, 79.0f / 255.0f);


        }

    }

    //public void UpdateTargetVariation(Sprite _targetVariation, Color _color)
    //{
    //    m_imgTargetVariation.sprite = _targetVariation;
    //    m_imgTargetVariation.color = _color;
    //}

    public void UpdateTargetVariations(Sprite[] _targetVariations, Color[] _colors, bool[] _completedTraits, Person_Trait _targetTrait, bool _allTraitsDone)
    {
        // Firstly, if all of the traits are completed, we should show them all in full colour and also show the star indicator
        if (_allTraitsDone)
        {
            // Enable the star indicator
            m_mugshotStarIndicator.gameObject.SetActive(true);

            // Turn on all of the indicator traits with their colours, but don't animate them
            for (int i = 0; i < m_mugshotTraitIndicators.Length; i++)
            {
                m_mugshotTraitIndicators[i].sprite = _targetVariations[i];
                m_mugshotTraitIndicators[i].GetComponent<Animator>().enabled = false;

                // Colour but not the eyes
                if ((Person_Trait)i != Person_Trait.Eyes)
                    m_mugshotTraitIndicators[i].color = _colors[i];
            }
        }

        // If the traits are not all complete yet, we should only show the target one in colour and animated
        // The completed ones should be greyed out and the ones that haven't been started yet should be hidden entirely
        // Also, the star should not appear behind them
        else
        {
            // Disable the star indicator
            m_mugshotStarIndicator.gameObject.SetActive(false);

            // Loop through all of the trait images and update them accordingly
            for (int i = 0; i < m_mugshotTraitIndicators.Length; i++)
            {
                var currentTrait = (Person_Trait)i;
                var currentTraitImg = m_mugshotTraitIndicators[i];

                // Assign the correct sprite
                currentTraitImg.sprite = _targetVariations[i];

                // If this trait hasn't been completed yet and is not the current target, it should not even show up
                if (!_completedTraits[i] && currentTrait != _targetTrait)
                {
                    currentTraitImg.gameObject.SetActive(false);
                    continue;
                }
                // If is completed already, it should show up but just be greyed out and unanimated
                else if (_completedTraits[i])
                {
                    currentTraitImg.GetComponent<Animator>().enabled = false;
                    currentTraitImg.gameObject.SetActive(true);
                    currentTraitImg.color = Color.white;
                }
                // If this is the current active trait, we want to have the animator on and also use its actual colour (except for eyes which are always white)
                else if (currentTrait == _targetTrait)
                {
                    currentTraitImg.GetComponent<Animator>().enabled = true;
                    currentTraitImg.gameObject.SetActive(true);

                    if (currentTrait == Person_Trait.Eyes)
                        currentTraitImg.color = Color.white;
                    else
                        currentTraitImg.color = _colors[i];
                }
            }
        }
    }

    public void UpdateSelectableTrait(Person_Trait _selectTrait)
    {
        m_selectionTrait.text = "SELECTING: " + _selectTrait.ToString();
        m_selectionTraitImage.sprite = m_availableTraitImageSelections[(int)_selectTrait];
    }

    public void UpdateTraitProgress(int[] _traitCounters, int _numToCompleteTrait)
    {
        for (int i = 0; i < _traitCounters.Length; i++)
        {
            m_txtTraitProgress[i].text = _traitCounters[i].ToString() + " / " + _numToCompleteTrait.ToString();
            m_fillTraitProgress[i].fillAmount = (float)_traitCounters[i] / (float)_numToCompleteTrait;
        }
    }

    public void ShowEndScreen(bool _victory)
    {
        m_endScreen.SetActive(true);

        m_victoryMessage.SetActive(_victory);
        m_lossMessage.SetActive(!_victory);
    }

    public void StartSpinner()
    {
        // Show the spinner object
        m_spinnerCover.SetActive(true);

        // Hide it after a certain amount of time
        Invoke("EndSpinner", m_spinnerDuration);
    }

    public void EndSpinner()
    {
        // Hide the spinner cover
        m_spinnerCover.SetActive(false);
    }
}
