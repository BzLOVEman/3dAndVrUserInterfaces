using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveCube : MonoBehaviour {

    //頂点座標
    Vector3[] EndVertex = new Vector3[4];
    Vector3[] SideVertex = new Vector3[16];
    //色の変更など
    MeshRenderer meshRenderer;

    //面情報
    int[] EndFace = new int[6]  { 1,      0,      3,
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
    //分割数
    private int division = 10;
    //縦y
    private float vertical = 1f;
    //横x
    private float width = 6f;
    //奥行z
    private float depth = 1f;
    //周波数
    private float frequency = 1f;
    //振幅
    private float amplitude = 1f;
    //波長
    private float wavelength = 1f;
    //波の間隔
    private float interval = 0f;
    //波を片方だけにする（+側だけなど）
    private int oneSide = 0;
    /* 0 上下
     * 1 上だけ
     * 2 下だけ
     */
    //上下反転
    private bool isInversion = false;

    //親
    //private GameObject parentObj;
    //マテリアル
    public Material material;

    //メッシュ
    Mesh meshFirst;
    Mesh mesh;
    Mesh meshLast;

    //現在の分割部分（計算用）
    private int divisionNum = 0;
    CombineInstance[] combineInstanceAry;

    void Start() {
        DisplayObject();
    }

    void Update() {
    }

    private void DisplayObject() {
        // CombineMeshes()する時に使う配列   始端と終端も含めるので+3
        combineInstanceAry = new CombineInstance[division + 3];

        //メッシュ作成
        for (divisionNum = 0; divisionNum <= division; divisionNum++) {
            //頂点計算
            CalcVertices();

            if (divisionNum == 0) {
                //最初の一枚だけ別計算
                //メッシュ作成
                meshFirst = new Mesh();
                //メッシュリセット
                meshFirst.Clear();
                //メッシュへの頂点情報の追加
                meshFirst.vertices = EndVertex;
                //メッシュへの面情報の追加
                meshFirst.triangles = EndFace;

                // 合成するMesh（同じMeshを円形に並べたMesh）
                combineInstanceAry[0].mesh = meshFirst;
                combineInstanceAry[0].transform = Matrix4x4.Translate(Vector3.zero);
            }
            mesh = new Mesh();
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
                meshLast = new Mesh();
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
            Debug.Log("run " + divisionNum);
        }
        //合成した（する）メッシュ
        Mesh combinedMesh = new Mesh();
        combinedMesh.name = transform.name;
        combinedMesh.CombineMeshes(combineInstanceAry);

        //メッシュフィルター追加
        MeshFilter mesh_filter = new MeshFilter();
        mesh_filter = this.gameObject.AddComponent<MeshFilter>();
        //メッシュアタッチ
        mesh_filter.mesh = combinedMesh;
        //レンダラー追加 + マテリアルアタッチ
        meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
        //コライダーアタッチ
        MeshCollider meshCollider = this.gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh_filter.mesh;
        meshCollider.convex = true;
        meshCollider.isTrigger = true;

        //NormalMapの再計算
        mesh_filter.mesh.RecalculateNormals();
    }

    private void CalcVertices() {

        //上側手前左の頂点座標
        Vector3 vertex1 = new Vector3(width / (float)division * (float)divisionNum,
                                      vertical,
                                      0);
        //上側手前右の頂点座標
        Vector3 vertex2 = new Vector3(width / (float)division * (float)( divisionNum + 1 ),
                                      vertical,
                                      0);
        //下側手前左の頂点座標
        Vector3 vertex3 = new Vector3(width / (float)division * (float)divisionNum,
                                      0,
                                      0);
        //下側手前右の頂点座標
        Vector3 vertex4 = new Vector3(width / (float)division * (float)( divisionNum + 1 ),
                                      0,
                                      0);
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
            Debug.Log("run" + i.ToString());
        }

        //端面用
        if (divisionNum == 0) {
            //最初の端面
            EndVertex[0] = vertex1;
            EndVertex[1] = new Vector3(vertex1.x, vertex1.y, vertex1.z + depth);
            EndVertex[2] = vertex3;
            EndVertex[3] = new Vector3(vertex3.x, vertex3.y, vertex3.z + depth);
        } else if (divisionNum == division) {
            //最後の端面
            EndVertex[0] = new Vector3(vertex2.x, vertex2.y, vertex2.z + depth);
            EndVertex[1] = vertex2;
            EndVertex[2] = new Vector3(vertex4.x, vertex4.y, vertex4.z + depth);
            EndVertex[3] = vertex4;
        }

        //デバッグ用
        if (true && divisionNum == 0) {
            for (int i = 0; i < EndVertex.Length; i++) {
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.transform.name = "FirstMarker";
                marker.transform.parent = this.gameObject.transform;
                marker.transform.localPosition = EndVertex[i];
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


    }

}
