using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonHandler : InteractionHandler
{
    public ButtonType type;
    public int classId;
    public Action<ButtonType, int> OnButtonClicked;

    public void OnButtonSelected()
    {
        OnButtonClicked?.Invoke(type, classId);
    }
}

public enum ButtonType
{
    Start, Next, Previous
}
