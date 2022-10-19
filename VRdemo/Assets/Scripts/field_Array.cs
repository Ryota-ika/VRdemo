
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using TMPro;
using UnityEngine;

public class field_Array : MonoBehaviour
{
    //ブロックが配置されていればtrue,そうでなければfalse
    bool[,] blocks = new bool[10, 20];

    [SerializeField]
    GameObject score_View_Obj;
    [SerializeField]
    List<GameObject> blockList;
    //スコア用のTextMesh
    [SerializeField]
    TextMeshPro scoreTextMesh;
    [SerializeField]
    GameObject deleteEffect;

    //スコアの値
    int score;

    //ブロックを一列消去した際に追加するポイント
    const int DELETE_POINT = 120;

    int waitCnt = 0;
    const int MAXWAIT = 60;
    // Start is called before the first frame update
    void Start()
    {
        InitBlocks();
    }

    // Update is called once per frame
    void Update()
    {

        switch (GameStatus.status)
        {
            case "Shot":
                waitCnt = MAXWAIT;
                break;
            case "Wait":
                waitCnt--;
                //１秒ブロックが当たらなかったらShot状態に戻す
                if (waitCnt <= 0)
                {
                    GameStatus.status = "Shot";
                }
                break;
            case "Fall":
                Debug.Log("Fall Status");
                Debug.Log(FallBlocks());
                if (!FallBlocks())
                {
                    //FallBlocksの処理がうまくいっておらずfalseを満たさないため発射待機の状態に戻らない
                    GameStatus.status = "Delete";
                }
                break;
            case "Delete":
                CheckLines();
                GameStatus.status = "Wait";
                //Debug.Log(GameStatus.status);
                break;
        }
    }

    //block初期化
    void InitBlocks()
    {
        //GetLength=長さ取得
        for (int i = 0; i < blocks.GetLength(0); i++)
        {
            for (int j = 0; j < blocks.GetLength(1); j++)
            {
                blocks[i, j] = false;
            }
        }
    }

    int GetPosition(float value, float distance, int minRange, int maxRange)
    {
        for (int i = minRange; i < maxRange; i++)
        {
            if (Mathf.Abs(value - (float)(i)) < distance)
            {
                return i;
            }
        }
        return minRange - 1;
    }

    //当たったら実行
    void OnCollisionEnter(Collision other)
    {
        //var p = other.transform.position;
        //var b = other.gameObject.transform.rotation;
        //var u = 0.1f;
        //var g = new Vector3(
        //    ( float )( ( int )( b.x / u ) ) * u , //x座標
        //    ( float )( ( int )( b.y / u ) ) * u , //y座標
        //    transform.position.z         //z座標
        //    );

        ////座標を反映
        //other.gameObject.transform.position = g;
        ////向きを一定にする
        //other.gameObject.transform.rotation = Quaternion.Euler( 0 , 0 , 0 );

        //var rb = other.gameObject.GetComponent<Rigidbody>( );
        //Destroy( rb );
        ////Instantiate = prefab化したオブジェクトを生成
        //GameObject sv = Instantiate( score_View_Obj );
        //sv.transform.position = other.gameObject.transform.position;
        //sv.GetComponent<score_Representation>( ).textMesh.text = $"({p.x},{p.y},{p.z})";
        //Debug.Log(other.gameObject.tag);
        if (other.gameObject.tag != "blockUnits")
            return;

        GameStatus.status = "Fall";
        var b = other.gameObject.transform.position;
        var u = 0.1f;
        var g = new Vector3(
            ((int)(b.x / u)) * u,           //x座標
            ((int)(b.y / u)) * u,           //y座標
            transform.position.z        //z座標
            );

        //向きを一定にする
        other.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);

        //左下座標を定義
        float px = -0.4f;
        float py = 0.1f;

        //blocksの要素番号を取得
        int bx = GetPosition((g.x - px) / u, 0.01f, 0, 10);
        int by = GetPosition((g.y - py) / u, 0.01f, 0, 20);

        //範囲外ならエラーを返す
        if (bx < 0 || by < 0 || bx >= 10 || by >= 20 || CheckExistBlock(other.gameObject, bx, by))
        {
            GameStatus.status = "Shot";
            for (int i = 0; i < other.gameObject.transform.childCount; i++)
            {
                Destroy(other.gameObject.transform.GetChild(i).transform.GetComponent<BoxCollider>());
            }
            //ForceMode.Impulse=一つの関数呼び出しで瞬時に衝撃力を適用。爆発や衝突の力として、瞬時に起こる力を適用するのに有用
            //AddForceでフィールドから跳ね返るような方向にブロックが飛んでいくように処理
            other.gameObject.transform.GetComponent<Rigidbody>().AddForce(Vector3.back * 10f, ForceMode.Impulse);
        }
        else
        {
            //座標を反映
            other.gameObject.transform.position = new Vector3(bx * u + px, by * u + py, transform.position.z);
            //配置された位置の配列をtrueにする
            ApplyBlockUnits(other.gameObject, bx, by);

            // 位置を表示
            GameObject sv = Instantiate(score_View_Obj);
            sv.transform.position = other.gameObject.transform.position;
            sv.GetComponent<score_Representation>().textMesh.text = $"({bx},{by})";


            //親オブジェクトの削除
            int cnt = other.transform.childCount;
            for (int i = 0; i < cnt; i++)
            {
                other.transform.GetChild(0).parent = null;
            }
            other.transform.tag = "Untagged";
            other.transform.position = Vector3.forward * 1000f;

        }

        ////配置された位置の配列をtrueにする
        //blocks[ bx , by ] = true;
        //ApplyBlockUnits( other.gameObject, bx, by );
        ////参照できるように名前を設定
        //other.gameObject.name = $"name:{bx},{by}";

        //var rb = other.gameObject.GetComponent<Rigidbody>( );
        //Destroy( rb );
        ////}
    }

    //その位置にブロックが存在するかチェックする関数
    bool CheckExistBlock(GameObject target, int x, int y)
    {
        for (int i = 0; i < target.transform.childCount; i++)
        {
            Vector3 g = target.transform.GetChild(i).localPosition;
            int bx = (int)(g.x * 10);
            int by = (int)(g.y * 10);
            //枠外にはブロックが存在することにしておく
            if (x + bx < 0 || y + by < 0 || x + bx >= 10 || y + by >= 20)
            {
                return true;
            }
            //（x+bx,y+by）の位置にブロックが存在しているかどうかチェック
            if (!(x + bx < 0 || x + bx >= 10 || y + by < 0 || y + by >= 20) && blocks[x + bx, y + by])
            {
                return true;
            }
        }
        return false;

    }
    void ApplyBlockUnits(GameObject target, int x, int y)
    {
        //下記のコードを入れるとlistの中身がはいらなくなり、入れなければ落ちるところまでは実行されるが次ブロックを放つ時当たり判定はとれるが下に落ちなくなる。
        blockList = new List<GameObject>();
        for (int i = 0; i < target.transform.childCount; i++)
        {
            Vector3 g = target.transform.GetChild(i).localPosition;
            int bx = (int)(g.x * 10);
            int by = (int)(g.y * 10);
            blocks[x + bx, y + by] = true;
            //判定用の座標を設定
            target.transform.GetChild(i).GetComponent<Block>().x = x + bx;
            target.transform.GetChild(i).GetComponent<Block>().y = y + by;
            //参照できるように名前を設定
            target.transform.GetChild(i).name = $"name:{x + bx},{y + by}";
            //落下用ゲームオブジェクトに設定
            Debug.Log("ここまで到達");
            //blocklistへの代入が起こってない？
            blockList.Add(target.transform.GetChild(i).gameObject);

        }
    }
    void SortBlockList()
    {
        for (int i = 0; i < blockList.Count; i++)
        {
            for (int j = i; j < blockList.Count; j++)
            {
                if (blockList[i].GetComponent<Block>().y > blockList[j].GetComponent<Block>().y)
                {
                    var tmp = blockList[i];
                    blockList[i] = blockList[j];
                    blockList[j] = tmp;
                }
            }
        }
    }

    //ブロックを一段だけ落下させる処理
    //1.ブロックリストを高さ方向にソート
    //2.全てのブロックの一個下の位置にブロックが存在するかチェック
    //  1.存在しなければブロックを再配置
    //  2.存在すれば落下処理を終了させる（返り値をFalseで返す）
    bool FallBlocks()
    {
        var retblocks = blocks.Clone() as bool[,];
        //ソート
        SortBlockList();

        //ブロックが落とせる状態にあるかチェック
        bool isFall = true;

        //一個下にブロックがあるかチェック
        foreach (GameObject go in blockList)
        {
            int x = go.GetComponent<Block>().x;
            int y = go.GetComponent<Block>().y;
            if (y - 1 < 0 || retblocks[x, y - 1])
            {
                Debug.Log("到達");
                isFall = false;
                break;
            }
            retblocks[x, y] = false;
        }
        Debug.Log("isfall=" + isFall);
        if (isFall)
        {
            //ブロックを配置
            foreach (GameObject go in blockList)
            {
                //blockListに要素が入っておらず一回もfor文が回っていない模様
                int x = go.GetComponent<Block>().x;
                int y = go.GetComponent<Block>().y;
                Debug.Log("ここまで到達");
                go.transform.position += Vector3.down * 0.1f;

                //調査8が存在しない？
                blocks[x, y] = false;
                blocks[x, y - 1] = true;
                Debug.Log(go.name);
                go.GetComponent<Block>().y -= 1;
                go.name = $"name:{x},{y - 1}";
            }
        }
        return isFall;
    }
    //揃っている列があるかどうかをチェックしてその列のブロックを削除する関数
    void CheckLines()
    {
        for (int i = 0; i < 20; i++)
        {
            bool isDelete = true;
            for (int j = 0; j < 10; j++)
            {
                //上から順に調べたいため、「i」ではなく「20-i-1」を使用する
                if (!blocks[j, 20 - i - 1])
                {
                    isDelete = false;
                    break;
                }
            }
            //削除出来る列であれば削除
            if (isDelete)
            {
                //一列ブロックを削除
                DeleteBlocks(20 - i - 1);
                //消えた分だけブロックを下げる
                DropBlocks(20 - i - 1);

                //スコアを追加
                score += DELETE_POINT;
                scoreTextMesh.text = "" + score;
            }
        }
    }
    //1列ブロックを削除する処理
    void DeleteBlocks(int h)
    {
        GameObject[] glist = GameObject.FindGameObjectsWithTag("block");
        foreach (GameObject go in glist)
        {
            if (go.GetComponent<Block>().y == h)
            {
                //DeleteBlocks()により消去されるブロックがわかる。そのためそのブロックの位置でDeleteEffectが生成される。
                GameObject deleteEffectClone = Instantiate(deleteEffect);
                deleteEffectClone.transform.position = go.transform.position + Vector3.back * 0.1f;
                deleteEffectClone.transform.localScale = Vector3.one * 0.1f;
                blocks[go.GetComponent<Block>().x, h] = false;
                Destroy(go);
            }
        }
    }
    //消した列より高い位置のブロックを落下させる処理
    void DropBlocks(int h)
    {
        //落下させるブロックを登録
        List<GameObject> glist = new List<GameObject>(GameObject.FindGameObjectsWithTag("block"));
        glist.OrderBy((x) => x.GetComponent<Block>().y);

        foreach (GameObject go in glist)
        {
            if (go.GetComponent<Block>().y > h)
            {
                //blocksを更新
                int x = go.GetComponent<Block>().x;
                int y = go.GetComponent<Block>().y;
                blocks[x, y] = false;
                blocks[x, y - 1] = true;

                //y座標をずらす
                go.GetComponent<Block>().y -= 1;

                //GameObjectの座標をずらす
                go.transform.position += Vector3.down * 0.1f;
            }
        }
    }
}