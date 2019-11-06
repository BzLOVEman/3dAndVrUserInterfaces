using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveCube : MonoBehaviour {

    //頂点座標
    Vector3[] StartVertex = new Vector3[4];
    Vector3[] EndVertex = new Vector3[4];
    Vector3[] SideVertex = new Vector3[16];
    //全頂点
    Vector3[] AllVertex;
    //合成後のメッシュから座標の重なる頂点をまとめたもの
    int?[,] AllVertexKey;
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
    public Material material;

    //現在の分割部分（計算用）
    private int divisionNum = 0;
    CombineInstance[] combineInstanceAry;

    //メッシュの再計算が必要か
    private bool needReCulc = true;


    //外部初期化を受け付ける内容
    //分割数
    [SerializeField, Header("分割数")]
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
    }
    [SerializeField, Header("波の種類")]
    private WaveType waveType = WaveType.sin;


    void Start() {
        //必要なものをアタッチ
        mesh_filter = this.gameObject.AddComponent<MeshFilter>();
        meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
    }

    void Update() {
        if (needReCulc) {
            //今までのところに子オブジェクトがいればすべて消す
            ChildrenDestroy();
            //直方体を表示する
            DisplayObject();
            //計算後の頂点をまとめる
            PutTogetherAllVertexKey();
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
        foreach (Transform n in gameObject.transform) {
            Destroy(n.gameObject);
        }
    }

    //直方体を表示する
    private void DisplayObject() {
        // CombineMeshes()する時に使う配列   始端と終端も含めるので+3
        combineInstanceAry = new CombineInstance[division + 3];

        AllVertex = new Vector3[4 + division * 4 + 4];
        AllVertexKey = new int?[4 + division * 4 + 4, 4];

        //メッシュ作成
        for (divisionNum = 0; divisionNum <= division; divisionNum++) {
            //頂点計算
            CalcVertices();

            if (divisionNum == 0) {
                //最初の一枚だけ別計算
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
            }
            Mesh mesh = new Mesh();
            //メッシュリセット
            mesh.Clear();
            //メッシュへの頂点情報の追加
            mesh.vertices = SideVertex;
            //メッシュへの面情報の追加
            mesh.triangles = SideFace;

            //合成するMesh（同じMeshを円形に並べたMesh）
            combineInstanceAry[divisionNum + 1].mesh = mesh;
            combineInstanceAry[divisionNum + 1].transform = Matrix4x4.Translate(Vector3.zero);

            if (divisionNum == division) {
                //最後の一枚だけ別計算
                //メッシュ作成
                Mesh meshLast = new Mesh();
                //メッシュリセット
                meshLast.Clear();
                //メッシュへの頂点情報の追加
                meshLast.vertices = EndVertex;
                //メッシュへの面情報の追加
                meshLast.triangles = EndFace;

                // 合成するMesh（同じMeshを円形に並べたMesh）
                combineInstanceAry[divisionNum + 2].mesh = meshLast;
                combineInstanceAry[divisionNum + 2].transform = Matrix4x4.Translate(Vector3.zero);
            }
        }
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
    }

    //直方体の素の頂点計算
    private void CalcVertices() {

        //上側手前左の頂点座標
        Vector3 vertex1 = new Vector3(width / ( division + 1 ) * ( divisionNum + 0 ), vertical, 0);
        //上側手前右の頂点座標
        Vector3 vertex2 = new Vector3(width / ( division + 1 ) * ( divisionNum + 1 ), vertical, 0);
        //下側手前左の頂点座標
        Vector3 vertex3 = new Vector3(width / ( division + 1 ) * ( divisionNum + 0 ), 0, 0);
        //下側手前右の頂点座標
        Vector3 vertex4 = new Vector3(width / ( division + 1 ) * ( divisionNum + 1 ), 0, 0);
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

        //端面用
        if (divisionNum == 0) {
            //最初の端面
            StartVertex[0] = vertex1;
            StartVertex[1] = new Vector3(vertex1.x, vertex1.y, vertex1.z + depth);
            StartVertex[2] = vertex3;
            StartVertex[3] = new Vector3(vertex3.x, vertex3.y, vertex3.z + depth);
        }
        if (divisionNum == division) {
            //最後の端面
            EndVertex[0] = new Vector3(vertex2.x, vertex2.y, vertex2.z + depth);
            EndVertex[1] = vertex2;
            EndVertex[2] = new Vector3(vertex4.x, vertex4.y, vertex4.z + depth);
            EndVertex[3] = vertex4;
        }

        //デバッグ用
        //頂点に球を付ける
        if (true && divisionNum == 0) {
            for (int i = 0; i < StartVertex.Length; i++) {
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.transform.name = "FirstMarker";
                marker.transform.parent = this.gameObject.transform;
                marker.transform.localPosition = StartVertex[i];
                marker.transform.localScale = Vector3.one * 0.1f;
                marker.GetComponent<MeshRenderer>().material.color = Color.red;
            }
        } else if (true) {
            for (int i = 0; i < SideVertex.Length / 4; i++) {
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.transform.name = "Marker " + i.ToString();
                marker.transform.parent = this.gameObject.transform;
                marker.transform.localPosition = SideVertex[i];
                marker.transform.localScale = Vector3.one * 0.1f;
                marker.GetComponent<MeshRenderer>().material.color = Color.cyan;
            }
        }
        if (true && divisionNum == division) {
            for (int i = 0; i < EndVertex.Length; i++) {
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.transform.name = "EndMarker";
                marker.transform.parent = this.gameObject.transform;
                marker.transform.localPosition = EndVertex[i];
                marker.transform.localScale = Vector3.one * 0.1f;
                marker.GetComponent<MeshRenderer>().material.color = Color.yellow;
            }
        }

        //全頂点を保持
        for (int i = 0; i < SideVertex.Length / 2; i++) {
            //4つ飛ばしで8つ保存することで、端面の頂点までカバー
            AllVertex[divisionNum * SideVertex.Length / 4 + i] = SideVertex[i];
        }
    }

    //すべての頂点を必要にまとめる
    private void PutTogetherAllVertexKey() {
        //動作の重さ対策
        int vertexCount = mesh_filter.mesh.vertexCount;
        Vector3[] meshVertices = mesh_filter.mesh.vertices;
        //meshのどの頂点がどの頂点になっているか
        //事前に取得した頂点のそれぞれに当たる座標を検索する
        for (int i = 0; i < AllVertex.Length; i++) {
            //メッシュの頂点をひとつづつ見ていく
            for (int j = 0; j < vertexCount; j++) {
                //事前の頂点とメッシュの頂点が重なるか
                if (AllVertex[i] == meshVertices[j]) {
                    //見つけた頂点番号を保存するにあたり、既に同じ座標で見つけていないか確認
                    for (int k = 0; k < AllVertexKey.GetLength(1); k++) {
                        //まだ保存してない配列部分に保存
                        if (AllVertexKey[i, k] == null)
                            AllVertexKey[i, k] = j;
                    }
                }
            }
        }
    }
    //波を直方体にアタッチさせる
    private void GivePattern() {
        switch (waveType) {
            case WaveType.sin:
                List<Vector3> newMeshList=new List<Vector3>();
                int vertexCount = mesh_filter.mesh.vertexCount;
                Vector3[] meshVertices = mesh_filter.mesh.vertices;
                for (int i = 0; i < vertexCount; i++) {
                    newMeshList.Add(meshVertices[i]);
                }
                for (int i = 0; i < AllVertexKey.GetLength(0); i++) {
                    //直方体の左端
                    float firstX = AllVertex[0].x;
                    //現在地
                    float currentX = AllVertex[i].x;
                    //直方体の右端
                    float endX = AllVertex[AllVertex.Length-1].x;
                    //
                    float x = ( currentX - firstX ) / ( endX - firstX );
                    float y = SinWave(x);
                    for (int j = 0; j < AllVertexKey.GetLength(1); j++) {
                        Vector3 vector = newMeshList[(int)AllVertexKey[i, j]];
                        newMeshList[(int)AllVertexKey[i, j]] = new Vector3(vector.x,y,vector.z);
                    }
                }
                mesh_filter.mesh.SetVertices(newMeshList);
                break;
            case WaveType.cos:
                mesh_filter.mesh.vertices[0] = new Vector3(0, wavelength, 0);
                AllVertex[0] = new Vector3(0, wavelength, 0);
//                mesh_filter.mesh.SetVertices()
                mesh_filter.mesh.RecalculateNormals();
                break;
            case WaveType.tan:
                break;
            default:
                break;
        }
    }

    //sin波を計算する
    private float SinWave(float x) {
        float y;
        y = amplitude * Mathf.Sin(2 * Mathf.PI * frequency * x);
        Debug.Log(y);
        return y;
    }
}
