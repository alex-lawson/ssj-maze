using System.Collections;
using System.Collections.Generic;

public static class ArrayUtil
{
	public static void ShuffleArray<T>(ref T[] targetArray)
	{
		for (int i = 0; i < targetArray.Length; i++)
		{
			int r = UnityEngine.Random.Range(i, targetArray.Length);
			T tmp = targetArray[i];
			targetArray[i] = targetArray[r];
			targetArray[r] = tmp;
		}
	}
}