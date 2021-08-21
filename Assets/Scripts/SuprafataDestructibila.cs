using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace GK 
{
	public class SuprafataDestructibila : MonoBehaviour 
	{
		public TextMeshProUGUI text;
		public bool VoronoiCentreDeGreutate = false;
		public bool sticlaSecurizata = false;
		public MeshFilter Filter     { get; private set; }
		public MeshRenderer Renderer { get; private set; }
		public MeshCollider Collider { get; private set; }
		public Rigidbody Rigidbody   { get; private set; }

		//Contine punctele ce formeaza una dintre fete sdsdsds
		public List<Vector2> Poligon;
		public float Grosime = 1.0f;
		public float suprafataMinimaFracturare = 1f;
		public float fortaMinimaDeImpact = 50.0f;
		public float distantaMaxima = 0.5f;

		public float _Area = -1.0f;

		int age;

		[SerializeField]
        private int numarDeFragmente = 10;

        public float Arie 
		{
			get 
			{
				if (_Area < 0.0f) 
				{
					_Area = Geometrie.CalculeazaArie(Poligon);
				}

				return _Area;
			}
		}

		void Start() 
		{
			age = 0;

			if (Filter == null) Filter = GetComponent<MeshFilter>();
			if (Renderer == null) Renderer = GetComponent<MeshRenderer>();
			if (Collider == null) Collider = GetComponent<MeshCollider>();
			if (Rigidbody == null) Rigidbody = GetComponent<Rigidbody>();

			Reload();
		}

		public void Reload() 
		{
			if(sticlaSecurizata)
            {
				Rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            }


			if (Poligon.Count == 0) 
			{
				Vector3 scale = transform.localScale;
				Grosime = scale.z;
                // Assume it's a cube with localScale dimensions
                scale = 0.5f * scale;

				Poligon.Add(new Vector2(-scale.x, -scale.y));
				Poligon.Add(new Vector2(scale.x, -scale.y));
				Poligon.Add(new Vector2(scale.x, scale.y));
				Poligon.Add(new Vector2(-scale.x, scale.y));

				transform.localScale = Vector3.one;
			}

            Mesh mesh = PoligonTo3DMesh(Poligon, Grosime);

			Filter.sharedMesh = mesh;
			Collider.sharedMesh = mesh;

			//if(Arie < suprafataMinimaFracturare)
			//	StartCoroutine(stergeRigidBody());


		}

		private IEnumerator stergeRigidBody()
        {
			yield return new WaitForSeconds(10f);
			Destroy(Rigidbody);
        }

		void FixedUpdate() 
		{
            Vector3 pos = transform.position;

			age++;
			if (pos.magnitude > 1000.0f) 
			{
				DestroyImmediate(gameObject);
			}
		}

		private void OnMouseEnter()
		{
			if(!sticlaSecurizata)
				text.text = "Suprafata destructibila cu un numar de " + numarDeFragmente + " fragmente.";
			else
				text.text = "Suprafata destructibila cu un numar de " + numarDeFragmente + " fragmente si sticla securizata.";
		}

		private void OnMouseExit()
        {
			text.text = null;
        }
		
		void OnCollisionEnter(Collision coll) 
		{
			float fortaColiziune = coll.relativeVelocity.magnitude;
			if (age > 5 && fortaColiziune > fortaMinimaDeImpact) 
			{
                Vector3 punctDeImpact = coll.contacts[0].point;
				Fracturare((Vector2)transform.InverseTransformPoint(punctDeImpact));
			}
		}

		/// <summary>
		/// Variabila aleatoare cu distributie normala standard
		/// </summary>
		/// <param name="media"></param>
		/// <param name="deviatieStandard"></param>
		/// <returns></returns>
		static float NormalizedRandom(float media, float deviatieStandard) 
		{
            float u1 = UnityEngine.Random.value;
            float u2 = UnityEngine.Random.value;

            float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
				Mathf.Sin(2.0f * Mathf.PI * u2);

			return media + deviatieStandard * randStdNormal;
		}

		public void Fracturare(Vector2 pozitiaDeImpact) 
		{
            float arie = Arie;
			if (arie > suprafataMinimaFracturare) 
			{
                GeneratorDiagramaVoronoi generatorVoronoi = new GeneratorDiagramaVoronoi();
                TaiereVoronoi taiereVoronoi = new TaiereVoronoi();

                Vector2[] celuleVoronoi = new Vector2[numarDeFragmente];

				//Generam pozitiile punctelor ce vor fi folosite la generarea diagramei Voronoi utilizand pozitii aleatoare determinate de o distributie normala standard 
				for (int i = 0; i < celuleVoronoi.Length; i++) 
				{
					float dist = Mathf.Abs(NormalizedRandom(0.5f, distantaMaxima));
                    float angle = 2.0f * Mathf.PI * Random.value;

					celuleVoronoi[i] = pozitiaDeImpact + new Vector2(
							dist * Mathf.Cos(angle),
							dist * Mathf.Sin(angle));
					
				}

                DiagramaVoronoi diagrama = generatorVoronoi.CalculeazaDiagrama(celuleVoronoi, VoronoiCentreDeGreutate);

                List<Vector2> varfuriTaiere = new List<Vector2>();
				int debugNumarCeluleGenerate=0;

				//Parcurgem Array-ul de celule Voronoi si determinam pozitiile varfurilor ce vor forma noile obiecte pe baza carora vom instantia noile obiecte in scena
				for (int i = 0; i < celuleVoronoi.Length; i++) 
				{
					taiereVoronoi.TaieCelula(diagrama, Poligon, i, ref varfuriTaiere);
					
					if (varfuriTaiere.Count > 0) 
					{
						debugNumarCeluleGenerate++;
                        GameObject newGo = Instantiate(gameObject, transform.parent);

						newGo.transform.localPosition = transform.localPosition;
						newGo.transform.localRotation = transform.localRotation;


                        SuprafataDestructibila bs = newGo.GetComponent<SuprafataDestructibila>();

						bs.Grosime = Grosime;
						bs.Poligon.Clear();
						bs.Poligon.AddRange(varfuriTaiere);

                        float arieFragmentRezultat = bs.Arie;

						//Check if child is NaN and destroy
						if (!arieFragmentRezultat.EsteReal())
						{
							Destroy(newGo);
							continue;
						}
						

						Rigidbody rb = bs.GetComponent<Rigidbody>();

						rb.mass = Rigidbody.mass * (arieFragmentRezultat / arie);
					}
				}				

				gameObject.SetActive(false);
				Destroy(gameObject);
				Debug.Log(debugNumarCeluleGenerate);
			}
		}

		static Mesh PoligonTo3DMesh(List<Vector2> poligon, float Grosime) 
		{
            int count = poligon.Count;
			float extrudare = 0.5f * Grosime;

			int[] triunghiuri = new int[3 * (4 * count - 4)];
			Vector3[] varfuri = new Vector3[6 * count];
            Vector3[] normale = new Vector3[6 * count];

			//Indexi pentru array-urile de varfuri, normale si triunghiuri
			int ti = 0;
			int vi = 0;
            int ni = 0;

			//Poligon spate
			for (int i = 0; i < count; i++) 
			{
				varfuri[vi++] = new Vector3(poligon[i].x, poligon[i].y, extrudare);
				normale[ni++] = Vector3.forward;
			}

			//Poligon fata
			for (int i = 0; i < count; i++) 
			{
				varfuri[vi++] = new Vector3(poligon[i].x, poligon[i].y, -extrudare);
				normale[ni++] = Vector3.back;
			}

			//Restul poligoanelor
			for (int i = 0; i < count; i++) 
			{

				int iNext;

				if (i == count - 1)
				{
					iNext = 0;
				}
				else
				{
					iNext = i + 1;
				}

				varfuri[vi++] = new Vector3(poligon[i].x, poligon[i].y, extrudare);
				varfuri[vi++] = new Vector3(poligon[i].x, poligon[i].y, -extrudare);
				varfuri[vi++] = new Vector3(poligon[iNext].x, poligon[iNext].y, -extrudare);
				varfuri[vi++] = new Vector3(poligon[iNext].x, poligon[iNext].y, extrudare);

                Vector3 normala = Vector3.Cross(poligon[iNext] - poligon[i], Vector3.forward).normalized;

				normale[ni++] = normala;
				normale[ni++] = normala;
				normale[ni++] = normala;
				normale[ni++] = normala;
			}

			// Triunghiuri spate
			for (int varf = 2; varf < count; varf++) 
			{
				triunghiuri[ti++] = 0;
				triunghiuri[ti++] = varf - 1;
				triunghiuri[ti++] = varf;
			}

			//Triunghiuri fata (count = offset intre punctele de pe spate si fata)
			for (int varf = 2; varf < count; varf++) 
			{
				triunghiuri[ti++] = count;
				triunghiuri[ti++] = count + varf;
				triunghiuri[ti++] = count + varf - 1;
			}

			//Restul triunghiurilor
			for (int vert = 0; vert < count; vert++) 
			{
                int si = 2*count + 4*vert;

				triunghiuri[ti++] = si;
				triunghiuri[ti++] = si + 1;
				triunghiuri[ti++] = si + 2;

				triunghiuri[ti++] = si;
				triunghiuri[ti++] = si + 2;
				triunghiuri[ti++] = si + 3;
			}


			Debug.Assert(ti == triunghiuri.Length);
			Debug.Assert(vi == varfuri.Length);

            Mesh mesh = new Mesh();

			mesh.vertices = varfuri;
			mesh.triangles = triunghiuri;
			mesh.normals = normale;

			return mesh;

		}
	}

}
