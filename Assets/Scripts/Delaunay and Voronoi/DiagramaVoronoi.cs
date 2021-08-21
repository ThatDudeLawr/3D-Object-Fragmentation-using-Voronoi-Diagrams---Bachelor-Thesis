
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GK 
{
	public class DiagramaVoronoi 
	{
		/// <summary>
		/// Diagrama Voronoi este calculata pe baza unei Triangulari Delaunay.
		/// Aceasta este o referinta la triangulare.
		/// </summary>
		public readonly TriangulareaDelaunay Triangulare;

		/// <summary>
		/// Muchiile diagramei Voronoi grupate pe celule si ordonate CCW (invers acelor de ceas) 
		/// </summary>
		public readonly List<Muchie> Muchii;

		/// <summary>
		/// Varfurile diagramei Voronoi
		/// </summary>
		public readonly List<Vector2> Varfuri;

		/// <summary>
		/// Celulele diagramei Voronoi
		/// </summary>
		public readonly List<Vector2> Celule;

		/// <summary>
		/// O lista cu primele muchii ale celulelor, in care index-ul reprezinta celula
		/// </summary>
		public readonly List<int> PrimaMuchieACelulei;

		internal DiagramaVoronoi() 
		{
			Triangulare = new TriangulareaDelaunay();
			Muchii = new List<Muchie>();
			Varfuri = new List<Vector2>();
			Celule = Triangulare.Varfuri;
			PrimaMuchieACelulei = new List<int>();

		}

		internal void Clear()
		{
			Triangulare.Clear();
			Celule.Clear();
			PrimaMuchieACelulei.Clear();
			Muchii.Clear();
			Varfuri.Clear();
		}

		/// <summary>
		/// Tipul de muchie al unei muchii din diagrama Voronoi.
		/// O dreapta, reprezinta o linie infinita in ambele directii, 
		/// o semidreapta este o muchie ce incepe dintr-un varf si este infinita in cealalta directie,
		/// un "segment" este o muchie oarecare.
		///
		/// Exista 2 feluri de muchii, in sensul acelor de ceas (CW) si in sens invers (CCW)
		/// Segmentele sunt in sensul acelor de ceas insa semidreptele pot fi atat CW cat si CCW
		/// asa ca vom folosii 2 tipuri de muchii diferite pentru semidrepte.
		/// <summary>
		public enum TipDeMuchie
		{
			Dreapta,
			SemidreaptaCW,
			SemidreaptaCCW,
			Segment
		}

		/// <summary>
		/// Muchia din Diagrama Voronoi
		/// </summary>
		public struct Muchie 
		{

			/// <summary>
			/// Muchia poate fi: Dreapta, SemiDreaptaCW, SemiDreaptaCCW sau Segment
			/// </summary>
			readonly public TipDeMuchie tipDeMuchie;

			/// <summary>
			/// Primul varf al unei muchii.
			///
			/// Daca muchia este o dreapta, este un punct oarecare pe ea.
			/// Daca muchia este o semidreapta, punctul este originea semidreptei.
			/// Daca muchia este un segment, este unul dintre cele 2 capete.
			/// </summary>
			readonly public int varf1;

			/// <summary>
			/// Al doilea varf al unei muchii.
			///
			/// Este egal cu -1 in cazul in care muchia nu este un segment.
			/// </summary>
			readonly public int varf2;

			/// <summary>
			/// Celula asociata Muchiei (index catre array-ul Celule din obiectul parinte, DiagramaVoronoi)
			/// </summary>
			readonly public int celula;

			/// <summary>
			/// Vectorul de directie a unei drepte sau semidrepte. 
			/// Nu este normalized.
			/// Pentru segmente vectorul este egal cu NaN pe ambele coordonate. 
			/// </summary>
			public Vector2 directie;
			
			/// <summary>
			/// Constructor de muchie
			/// </summary>
			public Muchie(TipDeMuchie tipDeMuchie, int celula, int varf1, int varf2, Vector2 directie) 
			{
				this.celula = celula;
				this.tipDeMuchie = tipDeMuchie;
				this.directie = directie;
				this.varf1 = varf1;
				this.varf2 = varf2;
			}


		}
	}
}
