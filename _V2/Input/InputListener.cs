namespace AFV2
{
    using AF;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.InputSystem;

    public class InputListener : MonoBehaviour
    {
        Vector2 move;
        public Vector2 Move
        {
            get { return move; }
        }

        public UnityEvent onNavigate;

        Vector2 look;
        public Vector2 Look
        {
            get { return look; }
        }

        bool cursorInputForLook = true;
        Vector2 scaleVector = new(1, 1);

        bool sprint;
        public bool Sprint
        {
            get { return sprint; }
        }

        bool jump;
        public bool Jump
        {
            get { return jump; }
        }

        [Header("Events")]
        public UnityEvent onRightAttack;
        public UnityEvent onLeftAttack;
        public UnityEvent onChangeCombatStance;
        public UnityEvent onMenu;
        public UnityEvent onInteract;

        [Header("Components")]
        [SerializeField] GameSettings gameSettings;
        [SerializeField] PlayerCamera playerCamera;

        public void OnMove(InputValue value)
        {
            move = value.Get<Vector2>();
        }

        public void OnLook(InputValue value)
        {
            if (cursorInputForLook)
            {
                look = value.Get<Vector2>();
            }

            if (scaleVector.x != gameSettings.cameraSensitivity)
            {
                scaleVector = new(gameSettings.cameraSensitivity, gameSettings.cameraSensitivity);
            }

            look.Scale(scaleVector);
        }

        public void OnSprint(InputValue value)
        {
            sprint = value.isPressed;
        }

        public void OnJump(InputValue value)
        {
            jump = value.isPressed;
            CancelInvoke(nameof(ResetJump)); // Cancel previous invoke if any
            Invoke(nameof(ResetJump), Time.deltaTime); // Schedule reset
        }

        void ResetJump() => jump = false;


        public void OnZoomIn(InputValue value)
        {
            if (value.isPressed)
            {
                float scrollDelta = value.Get<float>();

                playerCamera.ZoomOut(scrollDelta);
            }
        }

        public void OnZoomOut(InputValue value)
        {
            if (value.isPressed)
            {
                float scrollDelta = value.Get<float>();

                playerCamera.ZoomIn(scrollDelta);
            }
        }

        public void OnLightAttack(InputValue value)
        {
            if (value.isPressed)
                onRightAttack.Invoke();
        }

        public void OnBlock(InputValue value)
        {
            if (value.isPressed)
                onLeftAttack.Invoke();
        }


        public void OnChangeCombatStance(InputValue value)
        {
            onChangeCombatStance?.Invoke();
        }

        public void OnMenu(InputValue value)
        {
            if (value.isPressed)
            {
                onMenu?.Invoke();
            }
        }

        public void OnSwitchSpell(InputValue value)
        {
            if (value.isPressed)
            {
                onNavigate?.Invoke();
            }
        }

        public void OnSwitchConsumable(InputValue value)
        {
            if (value.isPressed)
            {
                onNavigate?.Invoke();
            }
        }

        public void OnSwitchWeapon(InputValue value)
        {
            if (value.isPressed)
            {
                onNavigate?.Invoke();
            }
        }

        public void OnSwitchShield(InputValue value)
        {
            if (value.isPressed)
            {
                onNavigate?.Invoke();
            }
        }

        public void OnInteract(InputValue value)
        {
            if (value.isPressed)
            {
                onInteract?.Invoke();
            }
        }
    }
}
