using System;
using System.Collections.Generic;
using UnityEngine;

public class Heap<T>   {
    global::Node[] items;
    int currentItemCount;
    
    public Heap(int maxHeapSize)
    {
        items = new global::Node[maxHeapSize];
    }

    public void Add(Node item)
    {
        
        item.heapIndex = currentItemCount;
        
        items[currentItemCount] = item;
        SortUp(item);
        currentItemCount++;
    }
    public Node RemoveFirst()
    {
        Node firstItem = items[0];

        currentItemCount--;

        items[0] = items[currentItemCount];
        items[0].heapIndex = 0;
        SortDown(items[0]);
        return firstItem;
    }
    public void UpdateItem (Node item)
    {
        SortUp(item);
    }

    public int Count
    {
        get
        {
            return currentItemCount;
        }
    }
    public bool Contains(Node item)
    {
        return Equals(items[item.heapIndex], item);
    }

    private void SortDown(Node item)
    {
        while (true)
        {
            int childLeft = item.heapIndex * 2 + 1;
            int childRight = item.heapIndex * 2 + 2;

            int swapIndex = 0;

            if (childLeft < currentItemCount)
            {
                swapIndex = childLeft;

                if (childRight < currentItemCount)
                {
                    if (items[childLeft].CompareTo(items[childRight]) < 0)
                    {
                        swapIndex = childRight;
                    }
                }
                if (item.CompareTo(items[swapIndex]) < 0)
                {
                    Swap(item, items[swapIndex]);
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;

            }
        }
    }

    void SortUp(Node item)
    {
        int parentIndex = (item.heapIndex - 1) / 2;

        while (true)
        {
            Node parentItem = items[parentIndex];
            if (item.CompareTo(parentItem)>0)
            {
                Swap(item, parentItem);
            }
            else
            {
                break;
            }
            parentIndex = (item.heapIndex - 1)/ 2;
        }
    }

    void Swap(Node itemA, Node itemB)
    {
        items[itemA.heapIndex] = itemB;
        items[itemB.heapIndex] = itemA;

        int itemAIndex = itemA.heapIndex;
        itemA.heapIndex = itemB.heapIndex;
        itemB.heapIndex = itemAIndex;
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
