using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//四角（三角2面）作製（折り返しありフェイス分けなし）
public class Chutrial3 : MonoBehaviour {
	//頂点座標
	Vector3[] StartVertex = new Vector3[4];
	Vector3[] EndVertex = new Vector3[4];
	Vector3[] CubeVertex = new Vector3[24];
	//色の変更など
	MeshRenderer meshRenderer;
	//メッシュフィルター
	MeshFilter mesh_filter;

	//面情報
	int[] Face = new int[36]{0,  1,  3,
							0,  3,  2,

							2 + 8,  3 + 8,  5,
							2 + 8,  5,  4,

							4 + 8,  5 + 8,  7,
							4 + 8,  7,  6,

							1 + 8,  6 + 8,  7 + 8,
							1 + 8,  0 + 8,  6 + 8,

							0 + 16,  2 + 16,  4 + 16,
							0 + 16,  4 + 16,  6 + 16,

							5 + 16,  3 + 16,  7 + 16,
							3 + 16,  1 + 16,  7 + 16};

	int[] EndFace = new int[6]  {1, 0,  3,
								0,  2,  3 };
	int[] SideFace = new int[24] { 2,      5,      4,
									2,      3,      5,

									1,      6,      7,
									1,      0,      6,

									2 + 8,  4 + 8,  0 + 8,
									4 + 8,  6 + 8,  0 + 8,

									7 + 8,  3 + 8,  1 + 8,
									5 + 8,  3 + 8,  7 + 8,
	};

	//マテリアル
	public Material material;
	CombineInstance[] combineInstanceAry;

	//生成したキューブを格納
	private GameObject generetedCube;

	void Start() {
		DisplayObject();
	}

	//直方体を表示する
	private void DisplayObject() {
		//キューブ用空オブジェクト生成
		generetedCube = new GameObject("myCube");
		generetedCube.transform.position = new Vector3(0, 0, 0);
		//必要なものをアタッチ
		mesh_filter = this.generetedCube.AddComponent<MeshFilter>();
		meshRenderer = this.generetedCube.AddComponent<MeshRenderer>();


		// CombineMeshes()する時に使う配列   始端と終端も含めるので+3
		combineInstanceAry = new CombineInstance[1];

		//頂点計算
		CalcVertices();

		Mesh mesh = new Mesh();
		//メッシュリセット
		mesh.Clear();
		//メッシュへの頂点情報の追加
		mesh.vertices = CubeVertex;
		//メッシュへの面情報の追加
		mesh.triangles = Face;
		/*//メッシュへの頂点情報の追加
		mesh.vertices = EndVertex;
		//メッシュへの面情報の追加
		mesh.triangles = EndFace;*/

		// 合成するMesh（同じMeshを円形に並べたMesh）
		// combineInstanceAry[0].mesh = meshLast;
		combineInstanceAry[0].mesh = mesh;
		combineInstanceAry[0].transform = Matrix4x4.Translate(Vector3.zero);
		//合成した（する）メッシュ
		Mesh combinedMesh = new Mesh();
		combinedMesh.name = transform.name;
		combinedMesh.CombineMeshes(combineInstanceAry);

		//メッシュアタッチ
		mesh_filter.mesh = combinedMesh;
		//レンダラーにマテリアルアタッチ
		meshRenderer.material = material;

		//NormalMapの再計算
		mesh_filter.mesh.RecalculateNormals();
		//できたら子供に。
		generetedCube.transform.parent = this.gameObject.transform;
	}

	//直方体の素の頂点計算
	private void CalcVertices() {
		//上側手前左の頂点座標
		Vector3 vertex1 = new Vector3(0, 1, 0);
		//上側手前右の頂点座標
		Vector3 vertex2 = new Vector3(1, 1, 0);
		//下側手前左の頂点座標
		Vector3 vertex3 = new Vector3(0, 0, 0);
		//下側手前右の頂点座標
		Vector3 vertex4 = new Vector3(1, 0, 0);
		//全頂点数8にそれぞれ座標が2つずつある
		for (int i = 0; i < 8 * 3; i++) {
			if (i % 8 == 0) {
				CubeVertex[i] = vertex3;
			} else if (i % 8 == 1) {
				CubeVertex[i] = new Vector3(vertex3.x, vertex3.y, vertex3.z + 1);
			} else if (i % 8 == 2) {
				CubeVertex[i] = vertex1;
			} else if (i % 8 == 3) {
				CubeVertex[i] = new Vector3(vertex1.x, vertex1.y, vertex1.z + 1);
			} else if (i % 8 == 4) {
				CubeVertex[i] = vertex2;
			} else if (i % 8 == 5) {
				CubeVertex[i] = new Vector3(vertex2.x, vertex2.y, vertex2.z + 1);
			} else if (i % 8 == 6) {
				CubeVertex[i] = vertex4;
			} else if (i % 8 == 7) {
				CubeVertex[i] = new Vector3(vertex4.x, vertex4.y, vertex4.z + 1);
			}
		}
		/*
				//最初の端面
				StartVertex[0] = vertex1;
				StartVertex[1] = new Vector3(vertex1.x, vertex1.y, vertex1.z);
				StartVertex[2] = vertex3;
				StartVertex[3] = new Vector3(vertex3.x, vertex3.y, vertex3.z);
				//最後の端面
				EndVertex[0] = new Vector3(vertex2.x, vertex2.y, vertex2.z);
				EndVertex[1] = vertex2;
				EndVertex[2] = new Vector3(vertex4.x, vertex4.y, vertex4.z);
				EndVertex[3] = vertex4;*/
	}
}
