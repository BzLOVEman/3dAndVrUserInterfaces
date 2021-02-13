using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//cube
public class Chutrial5 : MonoBehaviour {
	//頂点座標
	Vector3[] Vertex = new Vector3[24];

	//面情報
	int[] Face = new int[36]{   0,  1,  3,
								0,  3,  2,

								2 + 8,  3 + 8,  5,
								2 + 8,  5,  4,

								4 + 8,  5 + 8,  7,
								4 + 8,  7,  6,

								1 + 8,  6 + 8,  7 + 8,
								1 + 8,  0 + 8,  6 + 8,

								0 + 16,  2 + 16,  6 + 16,
								2 + 16,  4 + 16,  6 + 16,

								1 + 16,  5 + 16,  3 + 16,
								1 + 16,  7 + 16,  5 + 16, };

	//マテリアル
	public Material material;

	void Start() {
		DisplayObject();
		DisplayCaption("立方体");
	}

	//立方体表示
	private void DisplayObject() {
		//必要なものをアタッチ
		MeshFilter meshFilter
			= this.gameObject.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer
			= this.gameObject.AddComponent<MeshRenderer>();

		//頂点計算
		CalcVertices();

		//合成用インスタンスの配列
		CombineInstance[] combineInstanceAry
			= new CombineInstance[1];
		//合成用メッシュインスタンスのメッシュ領域を確保する
		combineInstanceAry[0].mesh = new Mesh();
		//頂点情報を追加
		combineInstanceAry[0].mesh.vertices = Vertex;
		//面情報を追加
		combineInstanceAry[0].mesh.triangles = Face;
		//おまじない
		combineInstanceAry[0].transform
			= Matrix4x4.Translate(Vector3.zero);

		//メッシュの入れ先を確保する
		meshFilter.mesh = new Mesh();
		//メッシュのアタッチと合成
		meshFilter.mesh.CombineMeshes(combineInstanceAry);

		//NormalMapの再計算
		meshFilter.mesh.RecalculateNormals();
		//マテリアルアタッチ
		meshRenderer.material = material;
	}

	//頂点計算
	private void CalcVertices() {
		//基点座標
		Vector3 vertex0 = new Vector3(0, 0, 0);
		Vector3 vertex1 = new Vector3(0, 1, 0);
		Vector3 vertex2 = new Vector3(1, 0, 0);
		Vector3 vertex3 = new Vector3(1, 1, 0);
		//全頂点数8にそれぞれ座標が3つずつある
		for (int i = 0; i < 8 * 3; i++) {
			if (i % 8 == 0) {
				Vertex[i] = vertex0;
			} else if (i % 8 == 1) {
				Vertex[i] = vertex1;
			} else if (i % 8 == 2) {
				Vertex[i] = vertex2;
			} else if (i % 8 == 3) {
				Vertex[i] = vertex3;
			} else if (i % 8 == 4) {
				Vertex[i] = vertex2 + Vector3.forward;
			} else if (i % 8 == 5) {
				Vertex[i] = vertex3 + Vector3.forward;
			} else if (i % 8 == 6) {
				Vertex[i] = vertex0 + Vector3.forward;
			} else if (i % 8 == 7) {
				Vertex[i] = vertex1 + Vector3.forward;
			}
		}
	}

	//キャプション表示
	private void DisplayCaption(string caption) {
		GameObject captionObj = new GameObject();
		captionObj.transform.localScale = Vector3.one * 0.05f;
		TextMesh textMesh = captionObj.AddComponent<TextMesh>();

		textMesh.fontSize = 200;
		textMesh.text = caption;
	}
}
