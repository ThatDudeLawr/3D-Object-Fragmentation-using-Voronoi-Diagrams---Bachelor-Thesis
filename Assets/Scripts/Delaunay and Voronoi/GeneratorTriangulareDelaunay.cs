using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GK 
{
	public class GeneratorTriangulareDelaunay 
	{

		int highest = -1;
		IList<Vector2> varfuri;

		List<TriangleNode> triunghiuri;

		/// <summary>
		/// Constructor GeneratorTriangulareDelaunay
		/// </summary>
		public GeneratorTriangulareDelaunay() {
			triunghiuri = new List<TriangleNode>();
		}

		/// <summary>
		/// Calculeaza Triangularea inputului de varfuri
		/// </summary>
		/// <param name="varfuri">Lista de varfuri ce urmeaza sa fie triangulata.</param>
		/// <returns>Returneaza triangularea<returns>
		public TriangulareaDelaunay CalculeazaTriangulare(IList<Vector2> varfuri) 
		{
			TriangulareaDelaunay result = null;
			CalculeazaTriangulare(varfuri, ref result);

			return result;
		}

		/// <summary>
		/// Calculeaza triangularea inputului de varfuri
		///
		/// Acest overload previne cumularea de garbage permitand reutilizarea rezultatului acesta fiind returnat ca referinta.
		/// </summary>
		/// <param name="varfuri">Lista de varfuri ce va fi utilizata la triangulare</param>
		/// <param name="result">Referinta ce va returna triangularea</param>
		public void CalculeazaTriangulare(IList<Vector2> varfuri, ref TriangulareaDelaunay result) 
		{
			if (varfuri == null) {
				throw new ArgumentNullException("Niciun varf detectat");
			}
			if (varfuri.Count < 3) {
				throw new ArgumentException("Sunt necesare minim 3 puncte pentru o triangulare.");
			}

			triunghiuri.Clear();
			this.varfuri = varfuri;

			highest = 0;

			for (int i = 0; i < varfuri.Count; i++) {
				if (Higher(highest, i)) {
					highest = i;
				}
			}

			// "Super-triunghiul" ce contine toate punctele ce urmeaza a fi triangulate
			triunghiuri.Add(new TriangleNode(-2, -1, highest));

			BowyerWatson();
			GenereazaRezultat(ref result);

			this.varfuri = null;
		}

		bool Higher(int pi0, int pi1) 
		{
			if (pi0 == -2) 
			{
				return false;
			} 
			else if (pi0 == -1)
			{
				return true;
			} 
			else if (pi1 == -2)
			{
				return true;
			}
			else if (pi1 == -1) 
			{
				return false;
			} 
			else 
			{
                Vector2 p0 = varfuri[pi0];
                Vector2 p1 = varfuri[pi1];

				if (p0.y < p1.y) 
				{
					return true;
				} 
				else if (p0.y > p1.y)
				{
					return false;
				} 
				else 
				{
					return p0.x < p1.x;
				}
			}
		}

		/// <summary>
		/// Algoritmul Bowyer-Watson
		/// </summary>
		void BowyerWatson() 
		{
			// Pentru fiecare punct cauta triunghiul ce il contine, il imparte in 3 triunghiuri noi
			// si apeleaza LegalizeazaMuchie pe toate muchiile opuse celor noi adaugate punctelor noi
			for (int i = 0; i < varfuri.Count; i++) 
			{
                int pi = i;

				if (pi == highest) 
					continue;

                // Indexul triunghiului ce contine punctul
                int ti = IdentificaTriunghiulCeContinePunctul(pi);

                TriangleNode t = triunghiuri[ti];

                // Varfurile triunghiului ce contine punctul in ordine CCW
                int p0 = t.P0;
                int p1 = t.P1;
                int p2 = t.P2;

                // Indexii triunghiurilor nou create
                int nti0 = triunghiuri.Count;
                int nti1 = nti0 + 1;
                int nti2 = nti0 + 2;

                // Noile triunghiuri! In ordine CCW (counter clockwise)
                TriangleNode nt0 = new TriangleNode(pi, p0, p1);
                TriangleNode nt1 = new TriangleNode(pi, p1, p2);
                TriangleNode nt2 = new TriangleNode(pi, p2, p0);


				// Setting the adjacency triangle references.  Only way to make
				// sure you do this right is by drawing the triunghiuri up on a
				// piece of paper.
				nt0.A0 = t.A2;
				nt1.A0 = t.A0;
				nt2.A0 = t.A1;

				nt0.A1 = nti1;
				nt1.A1 = nti2;
				nt2.A1 = nti0;

				nt0.A2 = nti2;
				nt1.A2 = nti0;
				nt2.A2 = nti1;

				// Noile triunghiuri sunt "copii" celui initial
				t.C0 = nti0;
				t.C1 = nti1;
				t.C2 = nti2;

				triunghiuri[ti] = t;

				triunghiuri.Add(nt0);
				triunghiuri.Add(nt1);
				triunghiuri.Add(nt2);

				if (nt0.A0 != -1) 
					LegalizeazaMuchie(nti0, nt0.A0, pi, p0, p1);
				if (nt1.A0 != -1) 
					LegalizeazaMuchie(nti1, nt1.A0, pi, p1, p2);
				if (nt2.A0 != -1) 
					LegalizeazaMuchie(nti2, nt2.A0, pi, p2, p0);
			}
		}

		void GenereazaRezultat(ref TriangulareaDelaunay result) 
		{
			if (result == null) 
			{
				result = new TriangulareaDelaunay();
			}

			result.Clear();

			for (int i = 0; i < varfuri.Count; i++) 
			{
				result.Varfuri.Add(varfuri[i]);
			}

			for (int i = 1; i < triunghiuri.Count; i++) 
			{
                TriangleNode t = triunghiuri[i];

				if (t.IsLeaf && t.IsInner) 
				{
					result.Triunghiuri.Add(t.P0);
					result.Triunghiuri.Add(t.P1);
					result.Triunghiuri.Add(t.P2);
				}
			}

		}

		/// <summary>
		/// Cauta frunza arborelui triunghiuri[ti] ce contine o anumita muchie.
		/// </summary>
		int LeafWithEdge(int ti, int e0, int e1) 
		{
			Debug.Assert(triunghiuri[ti].AreMuchia(e0, e1));

			while (!triunghiuri[ti].IsLeaf) 
			{
                TriangleNode t = triunghiuri[ti];

				if (t.C0 != -1 && triunghiuri[t.C0].AreMuchia(e0, e1)) 
				{
					ti = t.C0;
				} 
				else if (t.C1 != -1 && triunghiuri[t.C1].AreMuchia(e0, e1)) 
				{
					ti = t.C1;
				} 
				else if (t.C2 != -1 && triunghiuri[t.C2].AreMuchia(e0, e1)) 
				{
					ti = t.C2;
				} 
				else 
				{
					Debug.Assert(false);
					throw new System.Exception("This should never happen");
				}
			}

			return ti;
		}

		/// <summary>
		/// Este muchia legala sau trebuie facut edge flip?
		/// </summary>
		bool MuchieLegala(int k, int l, int i, int j) 
		{
			Debug.Assert(k != highest && k >= 0);

			// Verifica daca muchia este a "super-triunghiului"
            bool lMagic = l < 0;
            bool iMagic = i < 0;
            bool jMagic = j < 0;

			Debug.Assert(!(iMagic && jMagic));

			if (lMagic) 
			{
				return true;
			} 
			else if (iMagic) 
			{
				Debug.Assert(!jMagic);

                Vector2 p = varfuri[l];
                Vector2 l0 = varfuri[k];
                Vector2 l1 = varfuri[j];

				return Geometrie.EstePunctulLaStangaDreptei(p, l0, l1);
			} 
			else if (jMagic) 
			{
				Debug.Assert(!iMagic);

                Vector2 p = varfuri[l];
                Vector2 l0 = varfuri[k];
                Vector2 l1 = varfuri[i];

				return !Geometrie.EstePunctulLaStangaDreptei(p, l0, l1);
			} 
			else
			{
				Debug.Assert(k >= 0 && l >= 0 && i >= 0 && j >= 0);

                Vector2 p = varfuri[l];
                Vector2 c0 = varfuri[k];
                Vector2 c1 = varfuri[i];
                Vector2 c2 = varfuri[j];

				Debug.Assert(Geometrie.EstePunctulLaStangaDreptei(c2, c0, c1));
				Debug.Assert(Geometrie.EstePunctulLaStangaDreptei(c2, c1, p));

				return !Geometrie.InCentrulCircumscris(p, c0, c1, c2);
			}
		}

		void LegalizeazaMuchie(int ti0, int ti1, int pi, int li0, int li1) 
		{
			// ti1 s-ar putea sa nu fie frunza asa ca vom verifica acest lucru si vom returna frunza in cazul in care acesta nu este (ti0 este frunza pentru ca abia a fost adaugat )
			ti1 = LeafWithEdge(ti1, li0, li1);

            TriangleNode t0 = triunghiuri[ti0];
            TriangleNode t1 = triunghiuri[ti1];
            int qi = t1.AlTreileaVarf(li0, li1);

			Debug.Assert(t0.AreMuchia(li0, li1));
			Debug.Assert(t1.AreMuchia(li0, li1));
			Debug.Assert(t0.IsLeaf);
			Debug.Assert(t1.IsLeaf);
			Debug.Assert(t0.P0 == pi || t0.P1 == pi || t0.P2 == pi);
			Debug.Assert(t1.P0 == qi || t1.P1 == qi || t1.P2 == qi);

			if (!MuchieLegala(pi, qi, li0, li1)) 
			{
                int ti2 = triunghiuri.Count;
                int ti3 = ti2 + 1;

                TriangleNode t2 = new TriangleNode(pi, li0, qi);
                TriangleNode t3 = new TriangleNode(pi, qi, li1);

				t2.A0 = t1.TriunghiOpus(li1);
				t2.A1 = ti3;
				t2.A2 = t0.TriunghiOpus(li1);

				t3.A0 = t1.TriunghiOpus(li0);
				t3.A1 = t0.TriunghiOpus(li0);
				t3.A2 = ti2;

				triunghiuri.Add(t2);
				triunghiuri.Add(t3);

                TriangleNode nt0 = triunghiuri[ti0];
                TriangleNode nt1 = triunghiuri[ti1];

				nt0.C0 = ti2;
				nt0.C1 = ti3;

				nt1.C0 = ti2;
				nt1.C1 = ti3;

				triunghiuri[ti0] = nt0;
				triunghiuri[ti1] = nt1;

				if (t2.A0 != -1) 
					LegalizeazaMuchie(ti2, t2.A0, pi, li0, qi);
				if (t3.A0 != -1) 
					LegalizeazaMuchie(ti3, t3.A0, pi, qi, li1);
			}
		}

		/// <summary>
		/// Cauta frunza(triunghiul) din arborele de triunghiuri ce contine punctul "pi"
		/// </summary>
		int IdentificaTriunghiulCeContinePunctul(int pi) 
		{
            int curr = 0;

			while (!triunghiuri[curr].IsLeaf) 
			{
                TriangleNode t = triunghiuri[curr];

				if (t.C0 >= 0 && VerificaPunctInTriunghi(pi, t.C0)) 
				{
					curr = t.C0;
				} 
				else if (t.C1 >= 0 && VerificaPunctInTriunghi(pi, t.C1)) 
				{
					curr = t.C1;
				} 
				else 
				{
					curr = t.C2;
				}
			}

			return curr;
		}

		/// <summary>
		/// Verifica daca punctul pi se afla in interiorul triunghiului ti.
		/// </summary>
		/// <param name="pi">Punctul ce trebuie verificat</param>
		/// <param name="ti">Triunghiul in care se face verificarea</param>
		bool VerificaPunctInTriunghi(int pi, int ti) 
		{
            TriangleNode t = triunghiuri[ti];
			if (EsteLaStanga(pi, t.P0, t.P1) && EsteLaStanga(pi, t.P1, t.P2) && EsteLaStanga(pi, t.P2, t.P0))
				return true;
			else
				return false;
		}

		/// <summary>
		/// Verifica daca punctul pi este la stanga muchiei li0 li1.
		/// </summary>
		bool EsteLaStanga(int pi, int li0, int li1) 
		{
			if (li0 == -2) 
			{
				return Higher(li1, pi);
			} 
			else if (li0 == -1) 
			{
				return Higher(pi, li1);
			} 
			else if (li1 == -2) 
			{
				return Higher(pi, li0);
			} 
			else if (li1 == -1) 
			{
				return Higher(li0, pi);
			} 
			else 
			{
				Debug.Assert(li0 >= 0);
				Debug.Assert(li1 >= 0);

				return Geometrie.EstePunctulLaStangaDreptei(varfuri[pi], varfuri[li0], varfuri[li1]);
			}
		}

		/// <summary>
		/// Un nod in arborele de triunghiuri
		///
		/// Toti parametrii sunt indecsi
		/// </summary>
		struct TriangleNode 
		{
			// Punctele triunghiului
			public int P0;
			public int P1;
			public int P2;

			// Triunghiurile copii ale acestui triunghi
			//
			// A value of -1 means "no child"
			public int C0;
			public int C1;
			public int C2;

			// Triunghiurile adiacente triunghiului
			//
			// A0 este triunghiul adiacent opus punctului P0 (triunghiul A0 are (P1, P2) ca muchie
			//
			// -1 inseamna ca "nu exista triunghi adiacent" (triunghiul are muchia pe "super triunghi")
			public int A0;
			public int A1;
			public int A2;

			// Este acest triunghi o frunza in arbore? (Nu are copii)
			public bool IsLeaf 
			{
				get 
				{
					return C0 < 0 && C1 < 0 && C2 < 0;
				}
			}

			/// <summary>
			/// Face parte din triangularea finala (nu are muchii asociate cu "super-triunghiul")
			/// </summary>
			public bool IsInner 
			{
				get 
				{
					return P0 >= 0 && P1 >= 0 && P2 >= 0;
				}
			}

			public TriangleNode(int P0, int P1, int P2) 
			{
				this.P0 = P0;
				this.P1 = P1;
				this.P2 = P2;

				this.C0 = -1;
				this.C1 = -1;
				this.C2 = -1;

				this.A0 = -1;
				this.A1 = -1;
				this.A2 = -1;
			}


			/// <summary>
			/// Contine triunghiul muchia e0e1?
			/// </summary>
			public bool AreMuchia(int e0, int e1) 
			{
				if (e0 == P0)
					return e1 == P1 || e1 == P2;
				else if (e0 == P1) 
					return e1 == P0 || e1 == P2;
				else if (e0 == P2) 
					return e1 == P0 || e1 == P1;
				else
					return false;
			}


			/// <summary>
			/// Presupunand ca p0 si p1 sunt 2 varfuri ale triunghiului, returneaza al 3lea varf.
			/// </summary>
			public int AlTreileaVarf(int p0, int p1)
			{
				if (p0 == P0) 
				{
					if (p1 == P1) 
						return P2;
					if (p1 == P2) 
						return P1;
					throw new ArgumentException("p0 si p1 nu fac parte din triunghi");
				}
				if (p0 == P1)
				{
					if (p1 == P0) 
						return P2;
					if (p1 == P2) 
						return P0;
					throw new ArgumentException("p0 si p1 nu fac parte din triunghi");
				}
				if (p0 == P2) 
				{
					if (p1 == P0) 
						return P1;
					if (p1 == P1) 
						return P0;
					throw new ArgumentException("p0 si p1 nu fac parte din triunghi");
				}

				throw new ArgumentException("p0 si p1 nu fac parte din triunghi");
			}


			/// <summary>
			/// Returneaza triunghiul opus punctului p
			/// </summary>
			public int TriunghiOpus(int p)
			{
				if (p == P0) 
					return A0;
				if (p == P1)
					return A1;
				if (p == P2) 
					return A2;
				throw new ArgumentException("p not in triangle");
			}

		}
	}
}
