using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonHandler : MonoBehaviour
{
    public ButtonType type;
}

public enum ButtonType
{
    Start, Next, Previous
}
