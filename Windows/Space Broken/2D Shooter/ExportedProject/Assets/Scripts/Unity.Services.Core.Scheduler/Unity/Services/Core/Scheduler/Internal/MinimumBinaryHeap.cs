using System;

namespace Unity.Services.Core.Scheduler.Internal
{
	internal class MinimumBinaryHeap<T> where T : IComparable<T>
	{
		private const float k_IncreaseFactor = 1.5f;

		private const float k_DecreaseFactor = 2f;

		private readonly int m_MinimumCapacity;

		private T[] m_HeapArray;

		private int m_Count;

		public int Count => m_Count;

		public T Min => m_HeapArray[0];

		public MinimumBinaryHeap(int capacity = 10)
		{
			if (capacity <= 0)
			{
				throw new ArgumentException("capacity must be more than 0");
			}
			m_MinimumCapacity = capacity;
			m_HeapArray = new T[capacity];
			m_Count = 0;
		}

		public void Insert(T data)
		{
			IncreaseHeapCapacityWhenFull();
			int num = m_Count;
			m_HeapArray[m_Count] = data;
			m_Count++;
			while (num != 0 && m_HeapArray[num].CompareTo(m_HeapArray[Parent(num)]) < 0)
			{
				Swap(ref m_HeapArray[num], ref m_HeapArray[Parent(num)]);
				num = Parent(num);
			}
		}

		private void IncreaseHeapCapacityWhenFull()
		{
			if (m_Count == m_HeapArray.Length)
			{
				T[] array = new T[(int)Math.Ceiling((float)Count * 1.5f)];
				Array.Copy(m_HeapArray, array, m_Count);
				m_HeapArray = array;
			}
		}

		public void Remove(T data)
		{
			for (int num = GetKey(data); num != 0; num = Parent(num))
			{
				Swap(ref m_HeapArray[num], ref m_HeapArray[Parent(num)]);
			}
			ExtractMin();
		}

		public T ExtractMin()
		{
			if (m_Count <= 0)
			{
				throw new InvalidOperationException("Can not ExtractMin: BinaryHeap is empty.");
			}
			T result = m_HeapArray[0];
			if (m_Count == 1)
			{
				m_Count--;
				m_HeapArray[0] = default(T);
				return result;
			}
			m_Count--;
			m_HeapArray[0] = m_HeapArray[m_Count];
			m_HeapArray[m_Count] = default(T);
			MinHeapify(0);
			DecreaseHeapCapacityWhenSpare();
			return result;
		}

		private void DecreaseHeapCapacityWhenSpare()
		{
			if (m_Count > m_MinimumCapacity && (float)m_Count < (float)m_HeapArray.Length / 2f)
			{
				T[] array = new T[m_Count];
				Array.Copy(m_HeapArray, array, m_Count);
				m_HeapArray = array;
			}
		}

		private int GetKey(T data)
		{
			int result = -1;
			for (int i = 0; i < m_Count; i++)
			{
				if (m_HeapArray[i].Equals(data))
				{
					result = i;
					break;
				}
			}
			return result;
		}

		private void MinHeapify(int key)
		{
			int num = LeftChild(key);
			int num2 = RightChild(key);
			int num3 = key;
			if (num < m_Count && m_HeapArray[num].CompareTo(m_HeapArray[num3]) < 0)
			{
				num3 = num;
			}
			if (num2 < m_Count && m_HeapArray[num2].CompareTo(m_HeapArray[num3]) < 0)
			{
				num3 = num2;
			}
			if (num3 != key)
			{
				Swap(ref m_HeapArray[key], ref m_HeapArray[num3]);
				MinHeapify(num3);
			}
		}

		private static void Swap(ref T lhs, ref T rhs)
		{
			T val = lhs;
			lhs = rhs;
			rhs = val;
		}

		private static int Parent(int key)
		{
			return (key - 1) / 2;
		}

		private static int LeftChild(int key)
		{
			return 2 * key + 1;
		}

		private static int RightChild(int key)
		{
			return 2 * key + 2;
		}
	}
}
