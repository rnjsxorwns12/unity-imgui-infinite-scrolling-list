# Unity Infinite Scrolling List

## How to use
1. Copy the files into your existing unity project asset folder
2. Attach ```InfiniteScrollingList.cs``` script to your any object
3. Then you can now access ```InfiniteScrollingList.Instance``` from another script.

### InfiniteScrollingList.Instance.List
```Gets and sets a list.```
```C#
using UnityEngine;
using System.Collections.Generic;
public class ExampleClass : MonoBehaviour
{
    void Start()
    {
        InfiniteScrollingList.Instance.List = new List<string>();
        InfiniteScrollingList.Instance.List.Add("Example 1");
        InfiniteScrollingList.Instance.List.Add("Example 2");
        InfiniteScrollingList.Instance.List.Add("Example 3");
    }
}
```
### InfiniteScrollingList.Instance.SelectedIndex
```
Gets and sets the selected index.
A value of negative one (-1) is returned if the list is empty.
```
```C#
using UnityEngine;
public class ExampleClass : MonoBehaviour
{
    void Start()
    {
        Debug.Log(InfiniteScrollingList.Instance.SelectedIndex);
    }
}
```
### InfiniteScrollingList.Instance.SelectedIndexChanged
```
Your code is triggered when the selected Index has been changed.
```
```C#
using UnityEngine;
public class ExampleClass : MonoBehaviour
{
    void Start()
    {
        InfiniteScrollingList.Instance.SelectedIndexChanged = SelectedIndexChanged;
    }

    void SelectedIndexChanged(int index)
    {
        //Your code
    }
}
```
## Example
[![Example](https://img.youtube.com/vi/V-R03WTA5B4/0.jpg)](https://youtu.be/V-R03WTA5B4)
