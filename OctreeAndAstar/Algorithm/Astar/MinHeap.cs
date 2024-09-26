using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using Vector3 = UnityEngine.Vector3;

namespace OctreeAndAstar
{
    public class MinHeap
    {
        private List<Point> heap = new List<Point>();

        public void Add(Point point)
        {
            heap.Add(point);
            HeapifyUp(heap.Count - 1);
        }

        public Point RemoveMin()
        {
            if (heap.Count == 0) return null;

            Point min = heap[0];
            Point last = heap[heap.Count - 1];
            heap.RemoveAt(heap.Count - 1);

            if (heap.Count > 0)
            {
                heap[0] = last;
                HeapifyDown(0);
            }

            return min;
        }

        public Point Peek()
        {
            if (heap.Count == 0) return null;
            return heap[0];
        }

        private void HeapifyUp(int index)
        {
            int parentIndex = (index - 1) / 2;
            if (index > 0 && heap[index].f < heap[parentIndex].f)
            {
                Swap(index, parentIndex);
                HeapifyUp(parentIndex);
            }
        }

        private void HeapifyDown(int index)
        {
            int leftChild = 2 * index + 1;
            int rightChild = 2 * index + 2;
            int smallest = index;

            if (leftChild < heap.Count && heap[leftChild].f < heap[smallest].f)
            {
                smallest = leftChild;
            }

            if (rightChild < heap.Count && heap[rightChild].f < heap[smallest].f)
            {
                smallest = rightChild;
            }

            if (smallest != index)
            {
                Swap(index, smallest);
                HeapifyDown(smallest);
            }
        }

        private void Swap(int i, int j)
        {
            Point temp = heap[i];
            heap[i] = heap[j];
            heap[j] = temp;
        }

        public void Clear()
        {
            heap.Clear();
        }
        public bool ContainsPosition(Vector3 vector3)
        {
            for (int i = 0; i < heap.Count; i++)
            {
                if (heap[i].position == vector3)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
