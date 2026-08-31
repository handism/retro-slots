namespace UnityEngine {
    public class MonoBehaviour {
        public GameObject gameObject { get; }
        public Transform transform { get; }
    }
    public class GameObject {
        public T AddComponent<T>() where T : new() => new T();
        public void SetActive(bool v) {}
        public Transform transform { get; }
        public string name;
        public GameObject(string n, params System.Type[] types) {}
        public T GetComponent<T>() => default;
        public T GetComponentInChildren<T>() => default;
    }
    public class Transform {
        public void SetParent(Transform p, bool b) {}
        public void SetAsLastSibling() {}
        public Vector3 localScale;
    }
    public struct Vector3 {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 one => new Vector3(1,1,1);
    }
    public struct Vector2 {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
    }
    public struct Vector4 {
        public float x, y, z, w;
        public Vector4(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
    }
    public struct Color {
        public float r,g,b,a;
        public Color(float r,float g,float b,float a=1) { this.r=r; this.g=g; this.b=b; this.a=a; }
        public static Color white => new Color(1,1,1);
        public static Color yellow => new Color(1,1,0);
    }
    public class SerializeField : System.Attribute {}
    public class Header : System.Attribute { public Header(string name) {} }
    public static class Time { public static float unscaledTime; public static int frameCount; }
    public class RectTransform : Transform {
        public Vector2 anchorMin, anchorMax, pivot, sizeDelta, anchoredPosition;
    }
    public class Object {
        public static GameObject Instantiate(GameObject g, Transform t) => g;
        public static void DestroyImmediate(Object o) {}
        public static T FindFirstObjectByType<T>() => default;
    }
    public static class Debug { public static void LogWarning(string m) {} }
}
namespace UnityEngine.UI {
    public class Image : UnityEngine.Component { public UnityEngine.Color color; public bool raycastTarget; }
    public class Button : UnityEngine.Component {
        public ButtonClickedEvent onClick;
        public ColorBlock colors;
        public bool interactable;
        public class ButtonClickedEvent {
            public void AddListener(System.Action a) {}
            public void RemoveAllListeners() {}
        }
    }
    public struct ColorBlock {
        public UnityEngine.Color normalColor, highlightedColor, pressedColor, selectedColor, disabledColor;
    }
}
namespace UnityEngine.EventSystems {
    public class EventTrigger : UnityEngine.Component {
        public System.Collections.Generic.List<Entry> triggers = new();
        public class Entry {
            public EventTriggerType eventID;
            public TriggerEvent callback = new();
        }
        public class TriggerEvent { public void AddListener(System.Action<BaseEventData> a) {} }
    }
    public class BaseEventData {}
    public enum EventTriggerType { PointerDown, PointerUp, PointerExit }
}
namespace UnityEngine { public class Component {
    public T GetComponent<T>() => default;
    public GameObject gameObject { get; }
    public Transform transform { get; }
    public T GetComponentInChildren<T>() => default;
} }
namespace TMPro {
    public class TMP_Text : UnityEngine.Component {
        public string text; public float fontSize, fontSizeMax, fontSizeMin, characterSpacing;
        public bool enableAutoSizing;
        public TextWrappingModes textWrappingMode;
        public TextOverflowModes overflowMode;
        public UnityEngine.Vector4 margin;
        public UnityEngine.Color color;
    }
    public enum TextWrappingModes { NoWrap }
    public enum TextOverflowModes { Truncate }
}
namespace DG.Tweening {
    public static class DOTween {
        public static Tweener To(DG.Tweening.Core.DOGetter<long> getter, DG.Tweening.Core.DOSetter<long> setter, long endValue, float duration) => new Tweener();
        public static void Kill(object target, bool complete) {}
    }
    public class Tweener {
        public Tweener SetEase(Ease ease) => this;
        public Tweener SetUpdate(bool update) => this;
        public Tweener OnComplete(System.Action a) => this;
    }
    public enum Ease { OutQuad, OutCubic, OutBack, InQuad }
    public static class ShortcutExtensions {
        public static Tweener DOPunchScale(this UnityEngine.Transform t, UnityEngine.Vector3 p, float d, int v, float e) => new Tweener();
        public static Tweener DOScaleY(this UnityEngine.Transform t, float y, float d) => new Tweener();
    }
}
namespace DG.Tweening.Core {
    public delegate T DOGetter<out T>();
    public delegate void DOSetter<in T>(T pNewValue);
}
namespace SlotGame.Audio {
    public class AudioManager {}
    public enum SEType { ButtonClick }
    public static class AudioManagerExtensions {
        public static void PlaySE(this AudioManager m, SEType t) {}
    }
}
namespace SlotGame.Data {
    public class RetroColorTheme {
        public UnityEngine.Color spinButtonTop, spinButtonBottom, autoSpinButtonTop, autoSpinButtonBottom;
        public UnityEngine.Color autoSpinPopupBackground;
        public UnityEngine.Color betSelectedTop, betSelectedBottom, betUnselectedTop, betUnselectedBottom;
        public UnityEngine.Color betSelectedHighlight, betUnselectedHighlight, betSelectedPressed, betUnselectedPressed;
        public UnityEngine.Color betSelectedLabelColor, betUnselectedLabelColor;
        public UnityEngine.Color spinStopButtonTop, spinStopButtonBottom;
    }
}
public class UIGradient : UnityEngine.Component {
    public void SetColors(UnityEngine.Color t, UnityEngine.Color b) {}
}
