//https://github.com/rnjsxorwns12/unity-infinite-scroll-list
using System;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class InfiniteScrollList : MonoBehaviour
{
    [Serializable]
    public class Position
    {
        [Range(0, 1)]
        public float left;
        [Range(0, 1)]
        public float right;
        [Range(0, 1)]
        public float top;
        [Range(0, 1)]
        public float bottom;
        public Position(float left, float right, float top, float bottom)
        {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }
        public Position() : this(0, 0, 0, 0) { }
    }
    [Serializable]
    public class Item
    {
        [Range(0.01f, 1)]
        public float height = 0.1f;
        [Range(0, 1)]
        public float interval = 0.01f;
        public Texture backgroundNormal;
        public Texture backgroundSelected;
        public Position padding = new Position();
        public TextAnchor textAnchor = TextAnchor.MiddleCenter;
        public Font font;
        public FontStyle fontStyle = FontStyle.Normal;
        public Color fontColorNormal = Color.gray;
        public Color fontColorSelected = Color.white;
    }
    public static InfiniteScrollList Instance;
    public delegate void SelectedIndexChangedEvent(int index);
    private SelectedIndexChangedEvent selectedIndexChangedEvent;
    [SerializeField, Range(0,1)]
    private float pivot = 0.5f;
    [SerializeField]
    private Position margin = new Position();
    [SerializeField]
    private Position padding = new Position();
    [SerializeField]
    private Texture background;
    [SerializeField]
    private Item item = new Item();
    private GUIStyle labelStyle = new GUIStyle();
    private float drawPosition;
    private float marqueeEnd;
    private float marqueePos;
    private float touchPos;
    private float touchTime;
    private float touchSpeed;
    private int selectedIndex = -1;
    [HideInInspector]
    public List<string> List;

    public SelectedIndexChangedEvent SelectedIndexChanged
    {
        set{ selectedIndexChangedEvent = value; }
    }
    public int SelectedIndex
    {
        get { return GetIndex(drawPosition); }
        set
        {
            if (List == null || value < 0 || value >= List.Count) return;
            DrawPosition = Screen.height * Mathf.Max(0, 1 - margin.bottom - margin.top) * (item.height + item.interval) * value;
            CheckIfSelectedIndexChanged();
        }
    }

    private float DrawPosition
    {
        set
        {
            int drawSize = 1;
            if (List != null) drawSize = Mathf.Max(drawSize, List.Count);
            float limit = Screen.height * Mathf.Max(0, 1 - margin.bottom - margin.top) * (item.height + item.interval) * drawSize;
            value %= limit;
            if (value < 0) value += limit;
            drawPosition = value;
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    void OnGUI()
    {
        Rect bg = new Rect(
            Screen.width * margin.left,
            Screen.height * margin.top,
            Screen.width * Mathf.Max(0, 1 - margin.right - margin.left),
            Screen.height * Mathf.Max(0, 1 - margin.bottom - margin.top)
        );
        if (bg.width == 0 || bg.height == 0) return;
        if (background != null) GUI.DrawTexture(bg, background);
        Rect rect = new Rect(
            bg.width * padding.left,
            bg.height * padding.top,
            bg.width * Mathf.Max(0, 1 - padding.right - padding.left),
            bg.height * Mathf.Max(0, 1 - padding.bottom - padding.top)
        );
        if (rect.width == 0 || rect.height == 0) return;
        GUI.BeginGroup(bg);
        GUI.BeginGroup(rect);
        float itemHeight = bg.height * item.height;
        float itemTotalHeight = itemHeight + bg.height * item.interval;
        float currY = drawPosition + itemTotalHeight / 2 - bg.height * pivot + rect.y;
        int currIndex = (int)((drawPosition + itemTotalHeight / 2) / itemTotalHeight);
        labelStyle.font = item.font;
        labelStyle.fontStyle = item.fontStyle;
        labelStyle.alignment = item.textAnchor;
        labelStyle.fontSize = (int)(itemHeight * Mathf.Max(0, 1 - item.padding.bottom - item.padding.top));
        for (int i = -Mathf.CeilToInt((currIndex * itemTotalHeight - currY) / itemTotalHeight); i <= Mathf.CeilToInt((currY + rect.height - (currIndex + 1) * itemTotalHeight) / itemTotalHeight) ; i++)
        {
            float y = (currIndex + i) * itemTotalHeight;
            Texture itemBg = i == 0 ? item.backgroundSelected : item.backgroundNormal;
            if (itemBg != null) GUI.DrawTexture(new Rect(0, y - currY + (itemTotalHeight - itemHeight) / 2, rect.width, itemHeight), itemBg);
            if (List != null && List.Count > 0)
            {
                Rect itemContentRect = new Rect(
                    rect.width * item.padding.left,
                    y - currY + (itemTotalHeight - itemHeight) / 2 + itemHeight * item.padding.top,
                    rect.width * Mathf.Max(0, 1 - item.padding.right - item.padding.left),
                    labelStyle.fontSize
                );
                GUI.BeginGroup(itemContentRect);
                labelStyle.normal.textColor = i == 0 ? item.fontColorSelected : item.fontColorNormal;
                string content = List[GetIndexInRange(currIndex + i)];
                bool marquee = false;
                if (i == 0)
                {
                    float contentWidth = labelStyle.CalcSize(new GUIContent(content)).x;
                    if (itemContentRect.width < contentWidth)
                    {
                        marqueeEnd = contentWidth;
                        marquee = true;
                    }
                    else marqueeEnd = 0;
                }
                GUI.Label(new Rect(marquee ? -marqueePos : 0, 0, marquee ? marqueeEnd : itemContentRect.width, itemContentRect.height), content, labelStyle);
                GUI.EndGroup();
            }
        }
        GUI.EndGroup();
        GUI.EndGroup();
    }

    private int GetIndexInRange(int indexOutOfRange)
    {
        int index = indexOutOfRange % List.Count;
        if (index < 0) index += List.Count;
        return index;
    }

    private int GetIndex(float drawPosition)
    {
        if (List != null && List.Count > 0)
        {
            float itemTotalHeight = Screen.height * Mathf.Max(0, 1 - margin.bottom - margin.top) * (item.height + item.interval);
            return GetIndexInRange((int)((drawPosition + itemTotalHeight / 2) / itemTotalHeight));
        }
        return -1;
    }
    private void CheckIfSelectedIndexChanged()
    {
        if (selectedIndex != SelectedIndex) SelectedIndexChange(SelectedIndex);
    }
    private void SelectedIndexChange(int index)
    {
        selectedIndex = index;
        if (index < 0) return;
        marqueePos = 0;
        selectedIndexChangedEvent?.Invoke(index);
    }
    private int touchId = -1;
    private bool Touch(Vector3 pos)
    {
        float x = Screen.width * margin.left;
        float width = Screen.width * Mathf.Max(0, 1 - margin.right - margin.left);
        float y = Screen.height * margin.bottom;
        float height = Screen.height * Mathf.Max(0, 1 - margin.bottom - margin.top);
        return x < pos.x && pos.x < x + width && y < pos.y && pos.y < y + height;
    }
    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0) && Touch(Input.mousePosition))
        {
            touchTime = Time.deltaTime;
            touchSpeed = 0;
            touchPos = Input.mousePosition.y;
            touchId = 0;
        }
        else if (touchId == 0)
        {
            if (Input.GetMouseButton(0))
            {
                float mouseDeltaPos = Input.mousePosition.y - touchPos;
                touchPos = Input.mousePosition.y;
                DrawPosition = drawPosition + mouseDeltaPos;
                if (touchSpeed == 0 || (touchSpeed > 0 && mouseDeltaPos > 0) || (touchSpeed < 0 && mouseDeltaPos < 0))
                {
                    touchSpeed += mouseDeltaPos;
                    touchTime += Time.deltaTime;
                }
                else
                {
                    touchSpeed = mouseDeltaPos;
                    touchTime = Time.deltaTime;
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                touchSpeed /= touchTime;
                touchPos = touchSpeed;
                touchTime = 0;
                touchId = -1;
            }
        }
#elif UNITY_ANDROID || UNITY_IOS
        foreach (Touch t in Input.touches)
        {
            if (t.phase == TouchPhase.Began && Touch(t.position))
            {
                touchTime = Time.deltaTime;
                touchSpeed = 0;
                touchPos = Input.mousePosition.y;
                touchId = t.fingerId;
            }
            else if (touchId == t.fingerId)
            {
                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                {
                    touchSpeed /= touchTime;
                    touchPos = touchSpeed;
                    touchTime = 0;
                    touchId = -1;
                }
                else
                {
                    float mouseDeltaPos = Input.mousePosition.y - touchPos;
                    touchPos = Input.mousePosition.y;
                    DrawPosition = drawPosition + mouseDeltaPos;
                    if (touchSpeed == 0 || (touchSpeed > 0 && mouseDeltaPos > 0) || (touchSpeed < 0 && mouseDeltaPos < 0))
                    {
                        touchSpeed += mouseDeltaPos;
                        touchTime += Time.deltaTime;
                    }
                    else
                    {
                        touchSpeed = mouseDeltaPos;
                        touchTime = Time.deltaTime;
                    }
                }
            }
        }
#endif
        if (touchTime == 0)
        {
            DrawPosition = drawPosition + touchSpeed * Time.deltaTime;
            float mouseSpeedTemp = touchSpeed - touchPos * Time.deltaTime;
            if (touchSpeed < 0) touchSpeed = Mathf.Min(0, mouseSpeedTemp);
            else if (touchSpeed > 0) touchSpeed = Mathf.Max(0, mouseSpeedTemp);
        }
        CheckIfSelectedIndexChanged();
        marqueePos += labelStyle.fontSize * 2 * Time.deltaTime;
        if (marqueePos > marqueeEnd) marqueePos = 0;
    }
}
