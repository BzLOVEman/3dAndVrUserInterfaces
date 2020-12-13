using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//cubeを大量に配置する版。
public class cube : MonoBehaviour {
	//頂点座標
	Vector3[] StartVertex = new Vector3[4];
	Vector3[] EndVertex = new Vector3[4];
	Vector3[] SideVertex = new Vector3[16];
	//色の変更など
	MeshRenderer meshRenderer;
	//メッシュフィルター
	MeshFilter mesh_filter;

	//面情報
	int[] EndFace = new int[6]   { 1,      0,      3,
								   0,      2,      3 };
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
	public Material material1;
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
		generetedCube.transform.position = new Vector3(1+ transform.position.x,
																			generetedCube.transform.position.y + transform.position.y,
																			generetedCube.transform.position.z + transform.position.z);
		//必要なものをアタッチ
		mesh_filter = this.generetedCube.AddComponent<MeshFilter>();
		meshRenderer = this.generetedCube.AddComponent<MeshRenderer>();


		// CombineMeshes()する時に使う配列   始端と終端も含めるので+3
		combineInstanceAry = new CombineInstance[3];

		//頂点計算
		CalcVertices();

		//最初の一枚
		//メッシュ作成
		Mesh meshFirst = new Mesh();
		//メッシュリセット
		meshFirst.Clear();
		//メッシュへの頂点情報の追加
		meshFirst.vertices = StartVertex;
		//メッシュへの面情報の追加
		meshFirst.triangles = EndFace;

		// 合成するMesh（同じMeshを円形に並べたMesh）
		combineInstanceAry[0].mesh = meshFirst;
		combineInstanceAry[0].transform = Matrix4x4.Translate(Vector3.zero);
		Mesh mesh = new Mesh();
		//メッシュリセット
		mesh.Clear();
		//メッシュへの頂点情報の追加
		mesh.vertices = SideVertex;
		//メッシュへの面情報の追加
		mesh.triangles = SideFace;

		//合成するMesh（同じMeshを円形に並べたMesh）
		combineInstanceAry[1].mesh = mesh;
		combineInstanceAry[1].transform = Matrix4x4.Translate(Vector3.zero);

		//最後の一枚
		//メッシュ作成
		Mesh meshLast = new Mesh();
		//メッシュリセット
		meshLast.Clear();
		//メッシュへの頂点情報の追加
		meshLast.vertices = EndVertex;
		//メッシュへの面情報の追加
		meshLast.triangles = EndFace;

		// 合成するMesh（同じMeshを円形に並べたMesh）
		combineInstanceAry[2].mesh = meshLast;
		combineInstanceAry[2].transform = Matrix4x4.Translate(Vector3.zero);
		//合成した（する）メッシュ
		Mesh combinedMesh = new Mesh();
		combinedMesh.name = transform.name;
		combinedMesh.CombineMeshes(combineInstanceAry);

		//メッシュアタッチ
		mesh_filter.mesh = combinedMesh;
		//レンダラーにマテリアルアタッチ
		if ((float)divisionNum / division < percent) {
			meshRenderer.material = material1;
		} else {
			meshRenderer.material = material2;
		}

		//NormalMapの再計算
		mesh_filter.mesh.RecalculateNormals();
		//できたら子供に。
		generetedCubes[divisionNum].transform.parent = this.gameObject.transform;
	}

	//直方体の素の頂点計算
	private void CalcVertices() {
		//上側手前左の頂点座標
		Vector3 vertex1 = new Vector3(width / ( division + 0 ) * 0, vertical, 0);
		//上側手前右の頂点座標
		Vector3 vertex2 = new Vector3(width / ( division + 0 ), vertical, 0);
		//下側手前左の頂点座標
		Vector3 vertex3 = new Vector3(width / ( division + 0 ) * 0, 0, 0);
		//下側手前右の頂点座標
		Vector3 vertex4 = new Vector3(width / ( division + 0 ), 0, 0);
		//全頂点数8にそれぞれ座標が2つずつある
		for (int i = 0; i < 8 * 2; i++) {
			if (i % 8 == 0) {
				SideVertex[i] = vertex3;
			} else if (i % 8 == 1) {
				SideVertex[i] = new Vector3(vertex3.x, vertex3.y, vertex3.z + depth);
			} else if (i % 8 == 2) {
				SideVertex[i] = vertex1;
			} else if (i % 8 == 3) {
				SideVertex[i] = new Vector3(vertex1.x, vertex1.y, vertex1.z + depth);
			} else if (i % 8 == 4) {
				SideVertex[i] = vertex2;
			} else if (i % 8 == 5) {
				SideVertex[i] = new Vector3(vertex2.x, vertex2.y, vertex2.z + depth);
			} else if (i % 8 == 6) {
				SideVertex[i] = vertex4;
			} else if (i % 8 == 7) {
				SideVertex[i] = new Vector3(vertex4.x, vertex4.y, vertex4.z + depth);
			} else {
				Debug.LogWarning("Calcration Error");
			}
		}

		//最初の端面
		StartVertex[0] = vertex1;
		StartVertex[1] = new Vector3(vertex1.x, vertex1.y, vertex1.z + depth);
		StartVertex[2] = vertex3;
		StartVertex[3] = new Vector3(vertex3.x, vertex3.y, vertex3.z + depth);
		//最後の端面
		EndVertex[0] = new Vector3(vertex2.x, vertex2.y, vertex2.z + depth);
		EndVertex[1] = vertex2;
		EndVertex[2] = new Vector3(vertex4.x, vertex4.y, vertex4.z + depth);
		EndVertex[3] = vertex4;
	}
}
