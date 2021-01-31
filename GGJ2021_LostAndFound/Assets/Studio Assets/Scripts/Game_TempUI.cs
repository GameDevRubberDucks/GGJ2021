using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Game_TempUI : MonoBehaviour
{
    [Header("Score")]
    public TextMeshProUGUI m_txtScore;

    [Header("Hearts")]
    public TextMeshProUGUI m_txtCurrentHeartCount;
    public TextMeshProUGUI m_txtProgressTowardsNextHeart;

    [Header("Traits")]
    public Image m_imgTargetVariation;
    public TextMeshProUGUI m_selectionTrait;
    public TextMeshProUGUI[] m_txtTraitProgress;

    [Header("End Screens")]
    public GameObject m_endScreen;
    public GameObject m_victoryMessage;
    public GameObject m_lossMessage;

    public void UpdateScoreUI(int _newScore)
    {
        m_txtScore.text = "SCORE: " + _newScore.ToString();
    }

    public void UpdateHeartCountUI(int _numHearts, int _progressTowardsNextHeart, int _peopleUntilNextHeart)
    {
        m_txtCurrentHeartCount.text = "HEARTS: " + _numHearts.ToString();
        m_txtProgressTowardsNextHeart.text = _progressTowardsNextHeart.ToString() + " / " + _peopleUntilNextHeart.ToString();
    }

    public void UpdateTargetVariation(Sprite _targetVariation)
    {
        m_imgTargetVariation.sprite = _targetVariation;
    }

    public void UpdateSelectableTrait(Person_Trait _selectTrait)
    {
        m_selectionTrait.text = "SELECTING: " + m_selectionTrait;
    }

    public void UpdateTraitProgress(int[] _traitCounters, int _numToCompleteTrait)
    {
        for (int i = 0; i < _traitCounters.Length; i++)
        {
            m_txtTraitProgress[i].text = ((Person_Trait)i).ToString() + ": " + _traitCounters[i].ToString() + " / ";
        }
    }

    public void ShowEndScreen(bool _victory)
    {
        m_endScreen.SetActive(true);

        m_victoryMessage.SetActive(_victory);
        m_lossMessage.SetActive(!_victory);
    }
}
