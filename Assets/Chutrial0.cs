using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//三角1面作製
public class Chutrial0 : MonoBehaviour {
	//頂点座標
	Vector3[] TriangleVertex = new Vector3[3];

	int[] TriangleFace = new int[3] { 0, 1, 2 };

	//マテリアル
	public Material material;

	void Start() {
		DisplayObject();
	}

	//直方体を表示する
	private void DisplayObject() {
		//必要なものをアタッチ
		MeshFilter mesh_filter = this.gameObject.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = this.gameObject.AddComponent<MeshRenderer>();

		//頂点計算
		CalcVertices();

		//合成用インスタンスの配列
		CombineInstance[] combineInstanceAry = new CombineInstance[1];
		//合成用メッシュインスタンスのメッシュ領域を確保する
		combineInstanceAry[0].mesh = new Mesh();
		//頂点情報を追加
		combineInstanceAry[0].mesh.vertices = TriangleVertex;
		//面情報を追加
		combineInstanceAry[0].mesh.triangles = TriangleFace;
		//おまじない
		combineInstanceAry[0].transform = Matrix4x4.Translate(Vector3.zero);

		//メッシュの入れ先を確保する
		mesh_filter.mesh = new Mesh();
		//メッシュのアタッチと合成
		mesh_filter.mesh.CombineMeshes(combineInstanceAry);

		//マテリアルアタッチ
		meshRenderer.material = material;
	}

	//頂点計算
	private void CalcVertices() {
		TriangleVertex[0] = new Vector3(0, 0, 0);
		TriangleVertex[1] = new Vector3(0, 1, 0);
		TriangleVertex[2] = new Vector3(1, 0, 0);
	}
}
