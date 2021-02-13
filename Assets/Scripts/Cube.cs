using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//cube
public class Cube : MonoBehaviour {
	//頂点座標
	Vector3[] CubeVertex = new Vector3[24];
	//色の変更など
	MeshRenderer meshRenderer;
	//メッシュフィルター
	MeshFilter meshFilter;

	//面情報
	int[] Face = new int[36]{0,  1,  3,
							0,  3,  2,

							0+8, 2+8, 4,
							2+8, 6,   4,

							4+8, 6+8, 5,
							5,   6+8, 7,

							1+8, 5+8, 7+8,
							1+8, 7+8, 3+16,

							1+16,0+16,4+16,
							1+16,4+16,5+16,

							2+16,3+16,7+16,
							2+16,7+16,6+16,
							};

	//マテリアル
	public Material material;
	CombineInstance[] combineInstanceAry;

	//profile
	//ワールド座標
	private Vector3 position;
	//ワールド回転
	private Quaternion rotation;
	//ワールドスケール
	private Vector3 scale;
	//一意に与えられる番号
	private int myNum;

	//コンストラクタ（4値）
	public Cube(Vector3 constructorPosition, Quaternion constructorRotation, Vector2 constructorScale, int constructorName) {
		position = constructorPosition;
		rotation = constructorRotation;
		scale = constructorScale;
		myNum = constructorName;
	}

	//デフォルトコンストラクタ
	public Cube() {
		position = Vector3.zero;
		rotation.eulerAngles = Vector3.zero;
		scale = Vector3.one;
		myNum = 0;
	}

	void Start() {
		DisplayObject();
	}

	//直方体を表示する
	private void DisplayObject() {
		//自身にキューブポリゴンを生成していくための下準備
		this.gameObject.transform.name = myNum.ToString();
		this.gameObject.transform.position = Vector3.zero;
		//必要なものをアタッチ
		if (this.gameObject.GetComponent<MeshFilter>()) {
			meshFilter = this.gameObject.GetComponent<MeshFilter>();
		} else {
			meshFilter = this.gameObject.AddComponent<MeshFilter>();
		}
		if (this.gameObject.GetComponent<MeshRenderer>()) {
			meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
		} else {
			meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
		}

		// CombineMeshes()する時に使う配列
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

		// 合成するMesh（同じMeshを円形に並べたMesh）
		combineInstanceAry[0].mesh = mesh;
		combineInstanceAry[0].transform = Matrix4x4.Translate(Vector3.zero);
		//合成した（する）メッシュ
		Mesh combinedMesh = new Mesh();
		combinedMesh.name = transform.name;
		combinedMesh.CombineMeshes(combineInstanceAry);

		//メッシュアタッチ
		meshFilter.mesh = combinedMesh;
		//レンダラーにマテリアルアタッチ
		meshRenderer.material = material;

		//NormalMapの再計算
		meshFilter.mesh.RecalculateNormals();

		this.gameObject.transform.position = position;
		this.gameObject.transform.rotation = rotation;

	}

	//直方体の素の頂点計算
	private void CalcVertices() {
		//同じ頂点に3個頂点を配置する
		for (int wNum = 0; wNum < 3; wNum++) {
			for (int xNum = 0; xNum < 2; xNum++) {
				for (int yNum = 0; yNum < 2; yNum++) {
					for (int zNum = 0; zNum < 2; zNum++) {
						CubeVertex[wNum * 8 + xNum * 4 + yNum * 2 + zNum * 1] = new Vector3(
							scale.x * xNum - scale.x / 2,
							scale.y * yNum - scale.y / 2,
							scale.z * zNum - scale.z / 2
							);
					}
				}
			}
		}
	}
}
