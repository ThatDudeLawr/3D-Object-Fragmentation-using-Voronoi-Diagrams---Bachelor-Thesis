
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace GK 
{
	public class GeneratorDiagramaVoronoi 
	{

		GeneratorTriangulareDelaunay generatorDelaunay;
		ComparatorDePuncte cmp;
		List<VarfTriunghi> puncte;

		/// <summary>
		/// Constructor pentru GeneratorDiagramaVoronoi
		/// </summary>
		public GeneratorDiagramaVoronoi() 
		{
			puncte = new List<VarfTriunghi>();
			generatorDelaunay = new GeneratorTriangulareDelaunay();
			cmp = new ComparatorDePuncte();
		}

		/// <summary>
		/// Calculeaza diagrama Voronoi si o returneaza
		/// </summary>
		public DiagramaVoronoi CalculeazaDiagrama(IList<Vector2> inputVarfuri, bool VoronoiCentreDeGreutate)
		{
			DiagramaVoronoi diagrama = null;
			CalculeazaDiagrama(inputVarfuri, ref diagrama, VoronoiCentreDeGreutate);
			return diagrama;
		}

		/// <summary>
		/// Returneaza diagrama Voronoi printr-o referinta
		/// </summary>
		public void CalculeazaDiagrama(IList<Vector2> inputVarfuri, ref DiagramaVoronoi diagrama, bool VoronoiCentreDeGreutate) 
		{
			// TODO: special case for 1 points
			// TODO: special case for 2 points
			// TODO: special case for 3 points
			// TODO: special case for collinear points
			if (inputVarfuri.Count < 3) 
			{
				throw new NotImplementedException("Obiectul are sub 3 varfuri!");
			}

			if (diagrama == null) 
			{
				diagrama = new DiagramaVoronoi();
			}

            TriangulareaDelaunay triangulare = diagrama.Triangulare;

			diagrama.Clear();

			Profiler.BeginSample("Triangulare Delaunay");
			generatorDelaunay.CalculeazaTriangulare(inputVarfuri, ref triangulare);
			Profiler.EndSample();

			puncte.Clear();

            List<Vector2> varfuri = triangulare.Varfuri;
            List<int> triunghiuri = triangulare.Triunghiuri;
            List<Vector2> centreCircumscrise = diagrama.Varfuri;
            List<DiagramaVoronoi.Muchie> muchii = diagrama.Muchii;


			if (triunghiuri.Count > puncte.Capacity)   
				puncte.Capacity = triunghiuri.Count; 

			if (triunghiuri.Count > muchii.Capacity) 
				muchii.Capacity = triunghiuri.Count; 


			for (int ti = 0; ti < triunghiuri.Count; ti+=3) 
			{
                Vector2 p0 = varfuri[triunghiuri[ti]];
                Vector2 p1 = varfuri[triunghiuri[ti+1]];
                Vector2 p2 = varfuri[triunghiuri[ti+2]];

				// Triunghiul este in ordine inversa acelor de ceas CCW
				Debug.Assert(Geometrie.EstePunctulLaStangaDreptei(p2, p0, p1));

				centreCircumscrise.Add(Geometrie.CentruCircumscris(p0, p1, p2));

			}


			for (int ti = 0; ti < triunghiuri.Count; ti+=3) 
			{
				puncte.Add(new VarfTriunghi(triunghiuri[ti],   ti));
				puncte.Add(new VarfTriunghi(triunghiuri[ti+1], ti));
				puncte.Add(new VarfTriunghi(triunghiuri[ti+2], ti));
			}

			cmp.triunghiuri = triunghiuri;
			cmp.varfuri = varfuri;

			Profiler.BeginSample("Sortare");
			puncte.Sort(cmp);
			Profiler.EndSample();

			cmp.triunghiuri = null;
			cmp.varfuri = null;

			for (int i = 0; i < puncte.Count; i++) 
			{
				diagrama.PrimaMuchieACelulei.Add(muchii.Count);

                int start = i;
                int end = -1;

				for (int j = i+1; j < puncte.Count; j++) 
				{
					if (puncte[i].indexPunct != puncte[j].indexPunct) 
					{
						end = j - 1;
						break;
					}
				}

				if (end == -1) 
				{
					end = puncte.Count - 1;
				}

				i = end;

                int count = end - start;

				Debug.Assert(count >= 0);

				for (int ptiCurr = start; ptiCurr <= end; ptiCurr++) 
				{
					bool esteMuchie;

                    int ptiNext = ptiCurr + 1;

					if (ptiNext > end) 
						ptiNext = start;

                    VarfTriunghi ptCurr = puncte[ptiCurr];
                    VarfTriunghi ptNext = puncte[ptiNext];

                    int tiCurr = ptCurr.indexTriunghi;
                    int tiNext = ptNext.indexTriunghi;

                    Vector2 p0 = varfuri[ptCurr.indexPunct];

					if (count == 0) 
					{
						esteMuchie = true;
					} 
					else if (count == 1) 
					{
						Vector2 cNext, cCurr;
						
						cCurr = Geometrie.CentrulDeGreutateTriunghi(varfuri[triunghiuri[tiCurr]], varfuri[triunghiuri[tiCurr + 1]], varfuri[triunghiuri[tiCurr + 2]]);
						cNext = Geometrie.CentrulDeGreutateTriunghi(varfuri[triunghiuri[tiNext]], varfuri[triunghiuri[tiNext+1]], varfuri[triunghiuri[tiNext+2]]);

						esteMuchie = Geometrie.EstePunctulLaStangaDreptei(cCurr, p0, cNext);
					} 
					else 
					{
						esteMuchie = !MuchieComuna(triunghiuri, tiCurr, tiNext);
					}

					if (esteMuchie) 
					{
						Vector2 v0, v1;

						if (ptCurr.indexPunct == triunghiuri[tiCurr]) 
						{
							v0 = varfuri[triunghiuri[tiCurr+2]] - varfuri[triunghiuri[tiCurr+0]];
						} 
						else if (ptCurr.indexPunct == triunghiuri[tiCurr+1]) 
						{
							v0 = varfuri[triunghiuri[tiCurr+0]] - varfuri[triunghiuri[tiCurr+1]];
						} 
						else 
						{
							Debug.Assert(ptCurr.indexPunct == triunghiuri[tiCurr+2]);
							v0 = varfuri[triunghiuri[tiCurr+1]] - varfuri[triunghiuri[tiCurr+2]];
						}

						if (ptNext.indexPunct == triunghiuri[tiNext]) 
						{
							v1 = varfuri[triunghiuri[tiNext+0]] - varfuri[triunghiuri[tiNext+1]];
						} 
						else if (ptNext.indexPunct == triunghiuri[tiNext+1]) 
						{
							v1 = varfuri[triunghiuri[tiNext+1]] - varfuri[triunghiuri[tiNext+2]];
						}
						else 
						{
							Debug.Assert(ptNext.indexPunct == triunghiuri[tiNext+2]);
							v1 = varfuri[triunghiuri[tiNext+2]] - varfuri[triunghiuri[tiNext+0]];
						}

						muchii.Add(new DiagramaVoronoi.Muchie(
							DiagramaVoronoi.TipDeMuchie.SemidreaptaCCW,
							ptCurr.indexPunct,
							tiCurr / 3,
							-1,
							Geometrie.Roteste90DeGradeCW(v0)
						));

						muchii.Add(new DiagramaVoronoi.Muchie(
							DiagramaVoronoi.TipDeMuchie.SemidreaptaCW,
							ptCurr.indexPunct,
							tiNext / 3,
							-1,
							Geometrie.Roteste90DeGradeCW(v1)
						));
					} 
					else 
					{
						Vector2 v2nan = new Vector2(float.NaN, float.NaN);
						if (!Geometrie.SuntVarfuriIdentice(centreCircumscrise[tiCurr/3], centreCircumscrise[tiNext/3])) 
						{
							muchii.Add(new DiagramaVoronoi.Muchie(
								DiagramaVoronoi.TipDeMuchie.Segment,
								ptCurr.indexPunct,
								tiCurr / 3,
								tiNext / 3,
								v2nan
							));
						}
					}
				}
			}
		}
		/// <summary>
		/// Verifica daca exista muchie comuna intre 2 triunghiuri
		/// </summary>
		/// <param name="tris">Lista cu toate triunghiurile </param>
		/// <param name="ti0">Indexul de unde incep punctele triunghiului </param>
		/// <param name="ti1">Indexul de unde incep punctele triunghiului cu care se face verificarea </param>
		static bool MuchieComuna(List<int> tris, int ti0, int ti1) 
		{
			int x0 = tris[ti0];
            int x1 = tris[ti0+1];
            int x2 = tris[ti0+2];

            int y0 = tris[ti1];
            int y1 = tris[ti1+1];
            int y2 = tris[ti1+2];

			int n = 0;

			if (x0 == y0 || x0 == y1 || x0 == y2) 
				n++;
			if (x1 == y0 || x1 == y1 || x1 == y2) 
				n++;
			if (x2 == y0 || x2 == y1 || x2 == y2) 
				n++;

			if (n >= 2)
				return true;
			else
				return false;

		}


		struct VarfTriunghi 
		{
			public readonly int indexPunct;
			public readonly int indexTriunghi;

			public VarfTriunghi(int iPunct, int iTriunghi) 
			{
				indexPunct = iPunct;
				indexTriunghi = iTriunghi;
			}

		}

		class ComparatorDePuncte : IComparer<VarfTriunghi> 
		{
			public List<Vector2> varfuri;
			public List<int> triunghiuri;

			public int Compare(VarfTriunghi pt0, VarfTriunghi pt1) 
			{
				if (pt0.indexPunct < pt1.indexPunct) 
				{
					return -1;
				} 
				else if (pt0.indexPunct > pt1.indexPunct) 
				{
					return 1;
				} 
				else if (pt0.indexTriunghi == pt1.indexTriunghi) 
				{
					Debug.Assert(pt0.indexPunct == pt1.indexPunct);
					return 0;
				} 
				else 
				{
					return CompareAngles(pt0, pt1);
				}
			}

			int CompareAngles(VarfTriunghi pt0, VarfTriunghi pt1) 
			{
				Debug.Assert(pt0.indexPunct == pt1.indexPunct);

                Vector2 punctDeReferinta = varfuri[pt0.indexPunct];

                // Centrele de greutate in raport cu pozitia punctului de referinta
                Vector2 p0 = CentrulDeGreutate(pt0) - punctDeReferinta;
                Vector2 p1 = CentrulDeGreutate(pt1) - punctDeReferinta;

				// Cadranul din care face parte punctul p0 si respectiv p1, false pentru cadranele 1,2 si true pentru 3,4.
				bool q0, q1;
				if((p0.y < 0) || ((p0.y == 0) && (p0.x < 0)))
					q0 = true;
				else
					q0 = false;
				
				if ((p1.y < 0) || ((p1.y == 0) && (p1.y < 0)))
					q1 = true;
				else
					q1 = false;


				if (q0 == q1) 
				{
                    // p0 si p1 sunt in cadranele 1,2 sau 3,4 ceea ce inseamna ca sunt la cel mult 180 de grade distanta.
                    // Folosim produs vectorial ca sa aflam daca pt1 este la stanga lui pt0.

                    float produsVectorial = p0.x*p1.y - p0.y*p1.x;

					if (produsVectorial > 0) 
					{
						return -1;
					} 
					else if (produsVectorial < 0) 
					{
						return 1;
					} 
					else 
					{
						return 0;
					}
				} 
				else 
				{
                    // Daca q0 != q1, q1 este true rezulta p0 este in cadranul 1 sau 2,
                    // si p1 este in cadranul 3 sau 4. If q1
                    // Daca q1 nu este true, cadranele lui p0 si p1 sunt invers
                    if (q1) 
					{
						return -1;
					}
					else
                    {
						return 1;
                    }
				}
			}

			Vector2 CentrulDeGreutate(VarfTriunghi pt) 
			{
                int ti = pt.indexTriunghi;
				return Geometrie.CentrulDeGreutateTriunghi(varfuri[triunghiuri[ti]], varfuri[triunghiuri[ti+1]], varfuri[triunghiuri[ti+2]]);
			}
		}
	}
}
