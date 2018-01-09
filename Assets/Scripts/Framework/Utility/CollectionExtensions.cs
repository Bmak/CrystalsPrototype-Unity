using System;
using System.Collections.Generic;

public static class CollectionExtensions
{
    public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> items)
	{
		foreach (T item in items)
		{
            hashSet.Add(item);
		}
	}

	public static void Shuffle<T>(this List<T> list, JavaRandom random = null)
	{
		// This is an implementation of the Fisher Yates shuffle algorithm
		if (random == null) {
			random = new JavaRandom(Environment.TickCount);
		}
		for (int i = list.Count - 1; i > 0; i--) {
			int j = random.nextInt(i + 1);
			T val = list[i];
			list[i] = list[j];
			list[j] = val;
		}
	}
}