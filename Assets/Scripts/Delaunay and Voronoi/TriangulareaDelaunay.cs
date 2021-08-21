using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GK {
	public class TriangulareaDelaunay 
	{

		/// <summary>
		/// Lista varfurilor triangularii
		/// </summary>
		public readonly List<Vector2> Varfuri;

		/// <summary>
		/// Lista triunghiurilor triangularii. Indexii array-ului de varfuri.
		/// </summary>
		public readonly List<int> Triunghiuri;

		internal TriangulareaDelaunay() 
		{
			Varfuri = new List<Vector2>();
			Triunghiuri = new List<int>();
		}

		internal void Clear() 
		{
			Varfuri.Clear();
			Triunghiuri.Clear();
		}

		
	}
}
