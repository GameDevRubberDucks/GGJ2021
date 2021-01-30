using UnityEngine;

public class Person : MonoBehaviour
{
    //--- Private Variables ---//
    private Person_Descriptor m_descriptor;



    //--- Public Methods ---//
    public void ApplyDescription(Person_Descriptor _descriptor)
    {
        // Store the new descriptor
        this.m_descriptor = _descriptor;

        // TODO: Update the visuals
        // ...
    }
}
