using System;
using UnityEngine;

public class PhraseButton : MonoBehaviour
{
    public int Index;
    public Action<PhraseButton> OnPressed;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        OnPressed?.Invoke(this);    
    }
}
