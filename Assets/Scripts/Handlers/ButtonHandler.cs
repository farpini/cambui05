using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonHandler : InteractionHandler
{
    public ButtonType type;
    public Action<ButtonType> OnButtonClicked;

    public void OnButtonSelected()
    {
        OnButtonClicked?.Invoke(type);
    }
}

public enum ButtonType
{
    Start, Next, Previous
}
