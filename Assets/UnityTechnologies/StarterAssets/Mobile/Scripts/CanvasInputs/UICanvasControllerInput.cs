using UnityEngine;
using UnityEngine.UI;

namespace StarterAssets
{
    public class UICanvasControllerInput : MonoBehaviour
    {

        [Header("Output")]
        public StarterAssetsInputs starterAssetsInputs;

        public Joystick moveJoystick;
        public Joystick mouseJoystick;
        public Slider HorizontalSens;
        public Slider VderticalSens;

        /*public void VirtualMoveInput(Vector2 virtualMoveDirection)
        {
            starterAssetsInputs.MoveInput(new Vector2(moveJoystick.Horizontal, moveJoystick.Vertical));
        }

        public void VirtualLookInput(Vector2 virtualLookDirection)
        {
            starterAssetsInputs.LookInput(new Vector2(mouseJoystick.Horizontal, mouseJoystick.Vertical));
        }*/

        public void VirtualJumpInput(bool virtualJumpState)
        {
            starterAssetsInputs.JumpInput(virtualJumpState);
        }

        public void VirtualSprintInput(bool virtualSprintState)
        {
            starterAssetsInputs.SprintInput(virtualSprintState);
        }

        private void Update()
        {
            starterAssetsInputs.MoveInput(new Vector2(moveJoystick.Horizontal, moveJoystick.Vertical));
            starterAssetsInputs.LookInput(new Vector2(mouseJoystick.Horizontal * HorizontalSens.value, -(mouseJoystick.Vertical * VderticalSens.value)));
        }

    }

}
