using TMPro;
using UnityEngine;

public class MessageInstantiatorController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageComponent;

    public void SetMessage (string msg)
    {
        messageComponent.text = msg;
    }
}
