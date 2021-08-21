
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GK {
	public class TaiereVoronoi 
	{

		List<Vector2> pointsIn = new List<Vector2>();
		List<Vector2> pointsOut = new List<Vector2>();

		/// <summary>
		/// Constructor Taiere Voronoi
		/// </summary>
		public TaiereVoronoi() { }

		
		/// <summary>
		/// Taie celula din diagrama Voronoi folosindu-se de poligon (neaparat convex).
		/// Returneaza varfurile generate de taiere intr-o lista. Nu modifica poligonul sau diagrama.
		/// </summary>
		public void TaieCelula(DiagramaVoronoi diagrama, IList<Vector2> poligon, int celulaVoronoi, ref List<Vector2> clipped) 
		{
			pointsIn.Clear();

			pointsIn.AddRange(poligon);

			int primaMuchie, ultimaMuchie;
			
			if (celulaVoronoi == diagrama.Celule.Count - 1) 
			{
				primaMuchie = diagrama.PrimaMuchieACelulei[celulaVoronoi];
				ultimaMuchie = diagrama.Muchii.Count - 1;
			}
			else 
			{
				primaMuchie = diagrama.PrimaMuchieACelulei[celulaVoronoi];
				ultimaMuchie = diagrama.PrimaMuchieACelulei[celulaVoronoi + 1] - 1;
			}

			for (int indexMuchie = primaMuchie; indexMuchie <= ultimaMuchie; indexMuchie++) 
			{
				pointsOut.Clear();

                DiagramaVoronoi.Muchie muchie = diagrama.Muchii[indexMuchie];

				Vector2 punctulDreptei, directiaDreptei;

				if (muchie.tipDeMuchie == DiagramaVoronoi.TipDeMuchie.SemidreaptaCCW || muchie.tipDeMuchie == DiagramaVoronoi.TipDeMuchie.SemidreaptaCW)
				{
					punctulDreptei = diagrama.Varfuri[muchie.varf1];
					directiaDreptei = muchie.directie;

					if (muchie.tipDeMuchie == DiagramaVoronoi.TipDeMuchie.SemidreaptaCW) 
					{
						directiaDreptei *= -1;
					}
				} 
				else if (muchie.tipDeMuchie == DiagramaVoronoi.TipDeMuchie.Segment) 
				{
                    Vector2 punctul1 = diagrama.Varfuri[muchie.varf1];
                    Vector2 punctul2 = diagrama.Varfuri[muchie.varf2];

					punctulDreptei = punctul1;
					directiaDreptei = punctul2 - punctul1;
				} 
				else if (muchie.tipDeMuchie == DiagramaVoronoi.TipDeMuchie.Dreapta)
				{
					throw new NotSupportedException("Nu am implementat semiplane Voronoi");
				} 
				else 
				{
					Debug.Assert(false);
					return;
				}

				for (int pi0 = 0; pi0 < pointsIn.Count; pi0++) 
				{
					int pi1;

					if (pi0 == pointsIn.Count - 1)
					{
						pi1 = 0;
					}
					else
					{
						pi1 = pi0 + 1;
					}

                    Vector2 p0 = pointsIn[pi0];
                    Vector2 p1 = pointsIn[pi1];

                    bool p0Inside = Geometrie.EstePunctulLaStangaDreptei(p0, punctulDreptei, punctulDreptei + directiaDreptei);
                    bool p1Inside = Geometrie.EstePunctulLaStangaDreptei(p1, punctulDreptei, punctulDreptei + directiaDreptei);

					if (p0Inside && p1Inside) 
					{
						pointsOut.Add(p1);
					} 
					else if (!p0Inside && !p1Inside)
					{
						// Nu este nimic de facut, ambele puncte sunt in exterior
					} 
					else 
					{
                        Vector2 intersectie = Geometrie.PunctulDeIntersectieDouaDrepte(punctulDreptei, directiaDreptei.normalized, p0, (p1 - p0).normalized);

						if (p0Inside) 
						{
							pointsOut.Add(intersectie);
						} 
						else if (p1Inside)
						{
							pointsOut.Add(intersectie);
							pointsOut.Add(p1);
						} 
						else 
						{
							Debug.Assert(false);
						}
					}
				}

                List<Vector2> tmp = pointsIn;
				pointsIn = pointsOut;
				pointsOut = tmp;
			}

			if (clipped == null) 
			{
				clipped = new List<Vector2>();
			} 
			else
			{
				clipped.Clear();
			}

			clipped.AddRange(pointsIn);
		}
	}
}
