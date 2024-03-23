using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackManager : MonoBehaviour
{

    private readonly List<StackableObject> stackables = new();

    public void Register(StackableObject stackable)
    {
        stackables.Add(stackable);
    }

    public void Unregister(StackableObject stackable)
    {
        stackables.Remove(stackable);
    }


}
