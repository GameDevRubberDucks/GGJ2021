using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotWheelManager : MonoBehaviour
{
    //public
    public GameObject elementPrfab;

    public GameObject[] availableElements;

    //private
    private Person_Trait result;
    private int deletedTraitCounter = 0;

    // Start is called before the first frame update
    void Start()
    {
        resetSpinner();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Spin()
    {
        int pick = Random.Range(0, availableElements.Length);
        //randomly pick an element amuost the available elements
        if (availableElements[pick] != null)
        {
            result = availableElements[pick].GetComponent<SlotElement>().trait;
            deletedTraitCounter++;
        }
        else if(deletedTraitCounter < (int)Person_Trait.Num_Traits)
            Spin();      
    }

    public Person_Trait getResult()
    {
        return result;
    }

    public void resetSpinner()
    {
        //reset to a new list
        availableElements = new GameObject[(int)Person_Trait.Num_Traits];
        //delet child objects
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        //reset deleted trait counter
        deletedTraitCounter = 0;

        //popular the list
        for (int i = 0; i < (int)Person_Trait.Num_Traits; i++)
        {
            //instantiate new element gameobject, then place in the contrainer;
            GameObject temp = Instantiate(elementPrfab);
            temp.transform.SetParent(transform);
            //link SlotElement component to the list;
            availableElements[i] = temp;
            availableElements[i].GetComponent<SlotElement>().trait = (Person_Trait)i;
            availableElements[i].GetComponent<SlotElement>().setLabel();
        }
    }

    public void deleteTrait(Person_Trait t)
    {
        for (int i = 0; i < availableElements.Length; i++)
        {
            if (availableElements[i] != null && availableElements[i].GetComponent<SlotElement>().trait == t)
            {
                Destroy(availableElements[i]);
                availableElements[i] = null;
            }
        }
    }

    //testing function
    public void testDeleteTrait()
    {
        Spin();
        deleteTrait(result);
    }
}
