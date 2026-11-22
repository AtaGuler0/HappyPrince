using UnityEngine.InputSystem;

namespace MutluPrens.Player
{
    /// <summary>Bir karede okunan uçuş girdisi.</summary>
    public struct FlightInput
    {
        public float Throttle; // +1 ileri, -1 geri/yavaşla
        public float Turn;     // +1 sağa, -1 sola
        public float Vertical; // +1 yüksel, -1 alçal
        public bool Boost;
    }

    /// <summary>Kırlangıç'ı kim yönetiyorsa (klavye veya otomatik test pilotu) bu arayüzü uygular.</summary>
    public interface IFlightInputSource
    {
        FlightInput ReadFlight();
        bool InteractPressedThisFrame();
        bool InteractHeld();
    }

    /// <summary>Yeni Input System ile doğrudan klavye okuması.</summary>
    public sealed class KeyboardFlightInput : IFlightInputSource
    {
        public FlightInput ReadFlight()
        {
            var input = new FlightInput();
            var k = Keyboard.current;
            if (k == null) return input;

            input.Throttle = Axis(k.wKey.isPressed || k.upArrowKey.isPressed,
                                  k.sKey.isPressed || k.downArrowKey.isPressed);
            input.Turn = Axis(k.dKey.isPressed || k.rightArrowKey.isPressed,
                              k.aKey.isPressed || k.leftArrowKey.isPressed);
            input.Vertical = Axis(k.spaceKey.isPressed,
                                  k.leftCtrlKey.isPressed || k.rightCtrlKey.isPressed || k.cKey.isPressed);
            input.Boost = k.leftShiftKey.isPressed || k.rightShiftKey.isPressed;
            return input;
        }

        public bool InteractPressedThisFrame()
        {
            var k = Keyboard.current;
            return k != null && k.eKey.wasPressedThisFrame;
        }

        public bool InteractHeld()
        {
            var k = Keyboard.current;
            return k != null && k.eKey.isPressed;
        }

        private static float Axis(bool positive, bool negative) => (positive ? 1f : 0f) - (negative ? 1f : 0f);
    }
}
