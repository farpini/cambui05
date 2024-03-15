using System;
using UnityEngine;

public class PlayerHandler : ClientHandler
{
    public float cameraSpeed = 70f;
    public float cameraVerticalRotationMax = 40f;

    public Transform headTransform;
    public Transform sitingTransform;

    public void Update ()
    {
        if (!isClientInitialized)
        {
            return;
        }

        CheckWaypointClick();
        CheckMovement();
        UpdatePosition();
        ChangeAnimator();
    }

    public override void SetCamera (bool isStand, bool initRotation = false)
    {
        Camera.main.transform.SetParent(isStand ? headTransform : sitingTransform);
        Camera.main.transform.localPosition = Vector3.zero;
        if (initRotation)
        {
            Camera.main.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
        }
    }

    private void CheckWaypointClick ()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            var layerMask = LayerMask.GetMask("Waypoint");

            if (Physics.Raycast(ray, out var hitInfo, 1000f, layerMask))
            {
                if (hitInfo.transform.gameObject.GetComponent<WaypointHandler>())
                {
                    var waypointHandler = hitInfo.transform.gameObject.GetComponent<WaypointHandler>();
                    OnWaypointClicked?.Invoke(waypointHandler);
                }
            }
        }
    }

    private void CheckMovement ()
    {
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            var playerRotation = transform.eulerAngles;
            var rotationValue = playerRotation.y;
            rotationValue += Time.deltaTime * cameraSpeed;
            playerRotation.y = rotationValue;
            transform.eulerAngles = playerRotation;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            var playerRotation = transform.eulerAngles;
            var rotationValue = playerRotation.y;
            rotationValue -= Time.deltaTime * cameraSpeed;
            playerRotation.y = rotationValue;
            transform.eulerAngles = playerRotation;
        }

        var verticalRotationForNegative = 360f - cameraVerticalRotationMax;

        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            var cameraRotation = Camera.main.gameObject.transform.eulerAngles;
            var cameraRotationX = cameraRotation.x;
            cameraRotationX -= Time.deltaTime * cameraSpeed;
            if (cameraRotationX > cameraVerticalRotationMax && cameraRotationX < verticalRotationForNegative)
            {
                cameraRotationX = cameraRotationX > cameraVerticalRotationMax ? verticalRotationForNegative : cameraVerticalRotationMax;
            }
            cameraRotation.x = cameraRotationX;
            Camera.main.gameObject.transform.eulerAngles = cameraRotation;
        }
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            var cameraRotation = Camera.main.gameObject.transform.eulerAngles;
            var cameraRotationX = cameraRotation.x;
            cameraRotationX += Time.deltaTime * cameraSpeed;
            if (cameraRotationX > cameraVerticalRotationMax && cameraRotationX < verticalRotationForNegative)
            {
                cameraRotationX = cameraRotationX > cameraVerticalRotationMax ? cameraVerticalRotationMax : verticalRotationForNegative;
            }
            cameraRotation.x = cameraRotationX;
            Camera.main.gameObject.transform.eulerAngles = cameraRotation;
        }
    }
}