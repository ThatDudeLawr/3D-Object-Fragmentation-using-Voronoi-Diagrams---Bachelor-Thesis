
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GK {

	public class Geometrie : MonoBehaviour 
	{

		/// <summary>
		/// Verifica daca 2 varfuri sunt identice
		/// </summary>
		public static bool SuntVarfuriIdentice(Vector2 a, Vector2 b) 
		{
			if ((a - b).magnitude < 0.000001f)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Este punctul p la stanga dreptei l0, l1
		/// </summary>
		public static bool EstePunctulLaStangaDreptei(Vector2 p, Vector2 l0, Vector2 l1) 
		{
			if (((l1.x - l0.x) * (p.y - l0.y) - (l1.y - l0.y) * (p.x - l0.x)) >= 0)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Este punctul p in interiorul centrului cercului circumscris al triunghiului c0, c1, c2
		/// </summary>
		public static bool InCentrulCircumscris(Vector2 p, Vector2 c0, Vector2 c1, Vector2 c2) 
		{
            float ax = c0.x-p.x;
            float ay = c0.y-p.y;
            float bx = c1.x-p.x;
            float by = c1.y-p.y;
            float cx = c2.x-p.x;
            float cy = c2.y-p.y;

			if (((ax * ax + ay * ay) * (bx * cy - cx * by) -
				(bx * bx + by * by) * (ax * cy - cx * ay) +
				(cx * cx + cy * cy) * (ax * by - bx * ay)) > 0.000001f)
			{
				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// Roteste punctul v cu 90 de grade (il muta in cadranul urmator in sensul acelor de ceas)
		/// CW = in sensul acelor de ceas
		/// </summary>
		public static Vector2 Roteste90DeGradeCW(Vector2 v) 
		{
            float x = v.x;
			v.x = -v.y;
			v.y = x;

			return v;
		}

		/// <summary>
		/// Fiecare dreapta este definita de cate un punct (p0 si p1) si de o directie (v0 si v1)
		/// Valoarea returnata indica daca liniile se intersecteaza sau nu. 
		/// m0 si m1 reprezinta coeficiientii de multiplicare pentru coordonatele punctelor de directie v0 si v1 astfel incat sa obtinem o intersectie.
		///
		/// Ex: Daca intersectia este localizata in punctul X, atunci: 
		/// X = p0 + m0 * v0
		/// X = p1 + m1 * v1
		///
		/// Verificand valorile m0 si m1, putem verifica intersectia dreptelor, semidreptelor sau segmentelor.
		/// 
		/// p0 si p1 = punct pe dreapta
		/// v0 si v1 = directia "dreptei" in care verificam
		/// </summary>
		public static bool TestIntersectieDouaDrepte(Vector2 p0, Vector2 v0, Vector2 p1, Vector2 v1, out float m0, out float m1) 
		{
            float determinant = (v0.x * v1.y - v0.y * v1.x);

			if (Mathf.Abs(determinant) > 0.00000001f)
			{
				m0 = ((p0.y - p1.y) * v1.x - (p0.x - p1.x) * v1.y) / determinant;

				if (Mathf.Abs(v1.x) >= 0.0001f)
				{
					m1 = (p0.x + m0 * v0.x - p1.x) / v1.x;
				}
				else
				{
					m1 = (p0.y + m0 * v0.y - p1.y) / v1.y;
				}

				return true;
			} 
			else 
			{
				m0 = float.NaN;
				m1 = float.NaN;

				return false;
			}
		}

		/// <summary>
		/// Returneaza intersectia a doua drepte. p0/p1 sunt puncte pe drepte, v0/v1 reprezinta directia in care verificam
		/// Daca intersectia nu exista returneaza NaN
		/// <summary>
		public static Vector2 PunctulDeIntersectieDouaDrepte(Vector2 p0, Vector2 v0, Vector2 p1, Vector2 v1) 
		{
			float m0, m1;
			Vector2 rezultat = new Vector2();

			if (TestIntersectieDouaDrepte(p0, v0, p1, v1, out m0, out m1)) 
			{
				rezultat = p1 + m1 * v1;
				return rezultat;
			} 
			else 
			{
				rezultat = new Vector2(float.NaN, float.NaN);
				return rezultat;
			}
		}

		/// <summary>
		/// Returneaza Centrul Cercului Circumscris al triunghiului c0, c1, c2
		/// </summary>
		public static Vector2 CentruCircumscris(Vector2 c0, Vector2 c1, Vector2 c2) 
		{
			float m0, m1;
			Vector2 rezultat = new Vector2();

			Vector2 v0 = Roteste90DeGradeCW(c0 - c1);
			Vector2 v1 = Roteste90DeGradeCW(c1 - c2);

			Vector2 mp0 = 0.5f * (c0 + c1);
            Vector2 mp1 = 0.5f * (c1 + c2);

			Geometrie.TestIntersectieDouaDrepte(mp0, v0, mp1, v1, out m0, out m1);

			rezultat = mp0 + m0 * v0;

			return rezultat;
		}

		/// <summary>
		/// Returneaza centrul de greutate al triunghiului alcatuit de varfurile c0, c1 and c2. 
		/// </summary>
		public static Vector2 CentrulDeGreutateTriunghi(Vector2 c0, Vector2 c1, Vector2 c2) 
		{
            Vector2 val = (1.0f/3.0f) * (c0 + c1 + c2) ;
			return val;
		}

		/// <summary>
		/// Returneaza aria poligonului. Poligonul CCW returneaza o valoare pozitiva iar cel CW o valoare negativa.
		/// </summary>
		public static float CalculeazaArie(IList<Vector2> poligon) 
		{
			int count = poligon.Count;
			float arie = 0f;
			int j;

			for (int i = 0; i < count; i++) 
			{
				if(i==count-1)
                {
					j = 0;
                }
				else
                {
					j = i + 1;
                }

                Vector2 p0 = poligon[i];
                Vector2 p1 = poligon[j];

				arie += p0.x*p1.y - p1.y*p1.x;
			}

			return 0.5f * arie;
		}
	}
}
