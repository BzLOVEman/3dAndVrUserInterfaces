using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//cubeを大量に配置する版。
public class WaveCubeGeneretor : MonoBehaviour {
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
	public Material material2;

	//マテリアルの割合
	[Range(0, 1.0f)]
	public float percent;

	//現在の分割部分（計算用）
	private int divisionNum = 0;
	CombineInstance[] combineInstanceAry;

	//メッシュの再計算が必要か
	private bool needReCulc = true;


	//生成したキューブを格納
	private GameObject[] generetedCubes;

	//外部初期化を受け付ける内容
	//分割数
	[SerializeField, Header("分割数(Cube数)")]
	private int division = 10;
	//縦y
	[SerializeField, Header("縦y")]
	private float vertical = 1f;
	//横x
	[SerializeField, Header("横x")]
	private float width = 6f;
	//奥行z
	[SerializeField, Header("奥行z")]
	private float depth = 1f;
	//周波数
	[SerializeField, Header("周波数")]
	private float frequency = 1f;
	//振幅
	[SerializeField, Header("振幅")]
	private float amplitude = 1f;
	//波長 t=1/f
	[SerializeField, Header("波長")]
	private float wavelength = 1f;
	//波の間隔
	[SerializeField, Header("波の間隔")]
	private float interval = 0f;
	//波の速度
	[SerializeField, Header("波の速度")]
	private float speed = 1f;
	//波を片方だけにする（+側だけなど）
	public enum OneSide {
		both,
		upSide,
		downSide,
	}
	[SerializeField, Header("片面化")]
	private OneSide oneSide = OneSide.both;
	//上下反転
	[SerializeField, Header("上下反転")]
	private bool isInversion = false;
	//波の種類
	public enum WaveType {
		sin,
		cos,
		tan,
		arbitrary,
	}
	[SerializeField, Header("波の種類")]
	private WaveType waveType = WaveType.sin;


	void Start() {
		generetedCubes = new GameObject[division];
		for (divisionNum = 0; divisionNum < division; divisionNum++) {
			DisplayObject();
		}
	}

	void Update() {
		if (needReCulc) {
			//今までのところに子オブジェクトがいればすべて消す
			ChildrenDestroy();
			//直方体を表示する
			//キューブ格納先を初期化
			generetedCubes = new GameObject[division];
			for (divisionNum = 0; divisionNum < division; divisionNum++) {
				DisplayObject();
			}
			//次のフレームでは処理しない
			needReCulc = false;
		}
		GivePattern();
	}

	//インスペクターからの変更時に再計算
	private void OnValidate() {
		needReCulc = true;
	}

	//子オブジェクトを全削除
	private void ChildrenDestroy() {
		if (generetedCubes != null) {
			foreach (GameObject n in generetedCubes) {
				Destroy(n);
			}
		}
	}

	//直方体を表示する
	private void DisplayObject() {
		//キューブ用空オブジェクト生成
		generetedCubes[divisionNum] = new GameObject(divisionNum.ToString());
		generetedCubes[divisionNum].transform.position = new Vector3(width / ( division ) * ( divisionNum + 0 ),
																			generetedCubes[divisionNum].transform.position.y,
																			generetedCubes[divisionNum].transform.position.z);
		//必要なものをアタッチ
		mesh_filter = this.generetedCubes[divisionNum].AddComponent<MeshFilter>();
		meshRenderer = this.generetedCubes[divisionNum].AddComponent<MeshRenderer>();


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
		if (divisionNum < percent * 100) {
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

	//波を直方体にアタッチさせる
	private void GivePattern() {
		for (divisionNum = 0; divisionNum < division; divisionNum++) {
			generetedCubes[divisionNum].transform.localPosition = new Vector3(generetedCubes[divisionNum].transform.localPosition.x,
																				CalcY(int.Parse(generetedCubes[divisionNum].name) / (float)( division - 1 )),
																				generetedCubes[divisionNum].transform.localPosition.z);
		}
	}

	//ｘ軸の入力からｙ軸の出力を得る
	private float CalcY(float x) {
		if (waveType == WaveType.sin) {
			return SinWave(x);
		} else if (waveType == WaveType.cos) {
			return CosWave(x);
		} else if (waveType == WaveType.tan) {
			return TanWave(x);
		} else if (waveType == WaveType.arbitrary) {
			return ArbitraryWave(x);
		} else {
			//error
			Debug.LogWarning("波の計算種類に異常があります。");
			return 0f;
		}
	}

	//x値を味付け
	private float SeasonX(float x) {
		float l_x = x;
		l_x += Time.realtimeSinceStartup * wavelength * speed;
		return l_x;
	}

	//y値を味付け
	private float SeasonY(float y) {
		float l_y = y;
		//上下反転
		if (isInversion) {
			l_y = -l_y;
		}
		//片面化
		if (oneSide == OneSide.both) {
			//そのまま
		} else if (oneSide == OneSide.upSide) {
			//マイナス値は0
			if (l_y < 0)
				l_y = 0;
		} else if (oneSide == OneSide.downSide) {
			//プラス値は0
			if (l_y > 0)
				l_y = 0;
		} else {
			//エラー
		}
		return l_y;
	}

	//インターバル中か？
	private bool IsIntervalDuring(float x) {
		//今が何波長分かを求めて、停める波長数+動かす波長数（interval + 1）で割ると今何回目かでる。
		if (( SeasonX(x) / wavelength ) % ( interval + 1 ) >= 1) {
			return true;
		} else {
			return false;
		}
	}


	//sin波を計算する
	private float SinWave(float x) {
		float l_y = 0;
		//インターベル中じゃないなら、y=a*sin(2*pi*f*x)
		if (!IsIntervalDuring(x)) {
			l_y = amplitude * Mathf.Sin(2 * Mathf.PI * frequency * SeasonX(x));
			l_y = SeasonY(l_y);
		}
		return l_y;
	}

	//cos波を計算する
	private float CosWave(float x) {
		float l_y = 0;
		//インターベル中じゃないなら、y=a*cos(2*pi*f*x)
		if (!IsIntervalDuring(x)) {
			l_y = amplitude * Mathf.Cos(2 * Mathf.PI * frequency * SeasonX(x));
			l_y = SeasonY(l_y);
		}
		return l_y;
	}

	//tan波を計算する
	private float TanWave(float x) {
		float l_y = 0;
		//インターベル中じゃないなら、y=a*tan(2*pi*f*x)
		if (!IsIntervalDuring(x)) {
			l_y = amplitude * Mathf.Tan(2 * Mathf.PI * frequency * SeasonX(x));
			l_y = SeasonY(l_y);
		}
		return l_y;
	}

	//任意の波
	//自由にお使いください。
	private float ArbitraryWave(float x) {
		float y;
		y = 0;
		return y;
	}
}
