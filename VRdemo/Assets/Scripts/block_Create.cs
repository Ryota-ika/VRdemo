using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct BlockType {
    public int[ , ] shape;//形状を定義する配列
}

public class block_Create : MonoBehaviour {
    //ブロックのゲームオブジェクト
    [SerializeField]
    GameObject blockObj;

    //照準
    [SerializeField]
    GameObject pointObj;

    //発射するブロックユニットを定義
    GameObject shotObj;

    [SerializeField]
    BlockType[ ] block_Types;

    //左右どちらのコントローラかを取得するために使用するControllerクラス
    OVRInput.Controller contoroller;

    void Start( ) {
        //左右どちらかのコントローラかを取得
        contoroller = GetComponent<OVRControllerHelper>( ).m_controller;

        //ブロックユニットの種類を作成
        CreateBlockType( );

        //ブロックユニットを事前に生成
        int get_Random1 = 0;/*Random.RandomRange(0, 7);*/
        shotObj = create_Block(get_Random1, "shot");
        pointObj = create_Block(get_Random1, "point");
        //material=pointObj.GetComponent<Material>();
        //material.color = Color.white;
        // pointObj.GetComponent<Renderer>().sharedMaterial= material;
        shotObj.transform.parent = transform;
        shotObj.transform.localPosition = Vector3.zero;
    }

    // Update is called once per frame
    void Update( ) {
        switch( GameStatus.status ) {
            case "Shot":
                //ポインタの可視化
                ViewPointer( );
                //A/Xボタンを押したらブロックを発射
                if( OVRInput.GetDown( OVRInput.Button.PrimaryIndexTrigger, contoroller ) ) {
                    // Fire(transform.position,transform.forward, blockObj));
                    Fire( transform.position, transform.forward, shotObj );
                    int get_Random2 = 0;/*Random.RandomRange(0, 7);*/
                    shotObj = create_Block(get_Random2, "shot");
                    pointObj = create_Block(get_Random2, "point");
                    //material = pointObj.GetComponent<Material>( );
                    //material.color = Color.white;
                    shotObj.transform.parent = transform;
                    shotObj.transform.localPosition = -Vector3.zero;
                }
                if( OVRInput.GetDown( OVRInput.Button.One, contoroller ) ) {
                    RotateBlockUnit( true );
                }
                if( OVRInput.GetDown( OVRInput.Button.Two, contoroller ) ) {
                    RotateBlockUnit( false );
                }
                break;
            case "Wait":
                break;
            case "Fall":
                break;
            case "Delete":
                break;
        }

    }

    //ブロックを発射する関数
    void Fire( Vector3 startPos, Vector3 direction, GameObject target ) {
        ////ブロックのコピー生成させないようにコメントアウト
        //GameObject go = Instantiate(target);

        //重力を設定
        var rb = target.GetComponent<Rigidbody>( );
        ////重力を使用しない
        //rb.useGravity=true;
        target.transform.parent = null;

        //ブロックの衝突判定を許可する
        for( int i = 0; i < target.transform.childCount; i++ ) {
            target.transform.GetChild( i ).GetComponent<BoxCollider>( ).enabled = true;
        }


        //ブロックの発射位置を設定
        target.transform.position = startPos;
        //ブロックをコントローラ正面方向に放つ
        target.GetComponent<Rigidbody>( ).AddForce( direction * 10f, ForceMode.Impulse );
        //状態を「Wait」に変更
        GameStatus.status = "Wait";//連射しなくなる
        Destroy(pointObj);
    }
    //ブロックを生成
    GameObject create_Block( int type_Num,string type ) {
        int size = block_Types[ type_Num ].shape.GetLength( 0 );

        GameObject blockUnits = new GameObject( "BlockUnits" );
        blockUnits.tag = "blockUnits";
        //物理演算を適用させるためRigidbodyを追加
        var rb = blockUnits.AddComponent<Rigidbody>( );
        //重力がかからないように
        rb.useGravity = false;
        //生成したブロックユニットが物理演算により回転しないように
        rb.freezeRotation = true;
        //二次元配列をループで処理
        for( int i = 0; i < block_Types[ type_Num ].shape.GetLength( 0 ); i++ ) {
            for( int j = 0; j < block_Types[ type_Num ].shape.GetLength( 1 ); j++ ) {
                //ブロックを配置する位置であればブロック生成
                if( block_Types[ type_Num ].shape[ i, j ] == 1 ) {
                    GameObject go = Instantiate( blockObj );
                    go.transform.parent = blockUnits.transform;
                    //ブロックを生成する位置は親オブジェクトの相対位置で決定
                    go.transform.localPosition = new Vector3( i - size / 2, size / 2 - j, 0 ) * 0.1f;
                    //一時的にブロックの衝突判定をなくす
                    go.GetComponent<BoxCollider>( ).enabled = false;
                    if(/*生成タイプを引数で入力させて色を識別*/type=="point"){
                        /*goの色変え処理*/go.GetComponent<Renderer>().material.color=new Color(255,255,255,0.3f);
                    }
                }
            }
        }
        return blockUnits;
    }

    //7種類のブロックタイプを生成
    void CreateBlockType( ) {
        block_Types = new BlockType[ 1 ];
        //block_Types[ 0 ].shape = new int[ , ]{
        //        { 0,0,0,0},
        //        { 0,0,0,0},
        //        { 1,1,1,1},
        //        { 0,0,0,0},
        //    };
        //block_Types[ 1 ].shape = new int[ , ]{
        //    { 1,1},
        //    { 1,1},
        //};
        //block_Types[ 2 ].shape = new int[ , ]{
        //    { 0,1,0},
        //    { 1,1,1},
        //    { 0,0,0},
        //    };
        //block_Types[ 3 ].shape = new int[ , ]{
        //        { 0,0,1},
        //        { 1,1,1},
        //        { 0,0,0},
        //    };
        //block_Types[ 4 ].shape = new int[ , ]{
        //        { 1,0,0},
        //        { 1,1,1},
        //        { 0,0,0},
        //    };
        //block_Types[ 5 ].shape = new int[ , ]{
        //        { 1,1,0},
        //        { 0,1,1},
        //        { 0,0,0},
        //    };
        //block_Types[ 6 ].shape = new int[ , ]{
        //        { 0,1,1 },
        //        { 1,1,0},
        //        { 0,0,0},
        //    };
        block_Types[0].shape = new int[,]{
                { 1,1,1,1,1,1,1,1,1,1 }
            };
    }

    void RotateBlockUnit( bool isRight ) {
        if( isRight ) {
            for( int i = 0; i < shotObj.transform.childCount; i++ ) {
                //右回転
                Vector3 tmp = shotObj.transform.GetChild( i ).transform.localPosition;
                int x = shotObj.transform.GetChild( i ).GetComponent<Block>( ).x;
                int y = shotObj.transform.GetChild( i ).GetComponent<Block>( ).y;
                shotObj.transform.GetChild( i ).transform.localPosition = new Vector3( -tmp.y, tmp.x, 0 );
                shotObj.transform.GetChild( i ).GetComponent<Block>( ).x = -y;
                shotObj.transform.GetChild( i ).GetComponent<Block>( ).y = x;
            }
            for (int i = 0; i < pointObj.transform.childCount; i++)
            {
                //右回転
                Vector3 tmp = pointObj.transform.GetChild(i).transform.localPosition;
                int x = pointObj.transform.GetChild(i).GetComponent<Block>().x;
                int y = pointObj.transform.GetChild(i).GetComponent<Block>().y;
                pointObj.transform.GetChild(i).transform.localPosition = new Vector3(-tmp.y, tmp.x, 0);
                pointObj.transform.GetChild(i).GetComponent<Block>().x = -y;
                pointObj.transform.GetChild(i).GetComponent<Block>().y = x;
            }
        } else {
            for( int i = 0; i < shotObj.transform.childCount; i++ ) {
                //左回転
                Vector3 tmp = shotObj.transform.GetChild( i ).transform.localPosition;
                int x = shotObj.transform.GetChild( i ).GetComponent<Block>( ).x;
                int y = shotObj.transform.GetChild( i ).GetComponent<Block>( ).y;
                shotObj.transform.GetChild( i ).transform.localPosition = new Vector3( tmp.x, -tmp.y, 0 );
                shotObj.transform.GetChild( i ).GetComponent<Block>( ).x = y;
                shotObj.transform.GetChild( i ).GetComponent<Block>( ).y = -x;
            }
            for (int i = 0; i < pointObj.transform.childCount; i++)
            {
                //左回転
                Vector3 tmp = pointObj.transform.GetChild(i).transform.localPosition;
                int x = pointObj.transform.GetChild(i).GetComponent<Block>().x;
                int y = pointObj.transform.GetChild(i).GetComponent<Block>().y;
                pointObj.transform.GetChild(i).transform.localPosition = new Vector3(tmp.x, -tmp.y, 0);
                pointObj.transform.GetChild(i).GetComponent<Block>().x = y;
                pointObj.transform.GetChild(i).GetComponent<Block>().y = -x;
            }
        }
    }

    //コントローラの指す先の方向にぶつかったゲームオブジェクトをすべて取得
    void ViewPointer( ) {
        RaycastHit[ ] hits = Physics.RaycastAll( transform.position, transform.forward, 10f );
        if( hits.Length == 0 ) {
            pointObj.SetActive( false );
        } else {
            pointObj.SetActive( true );

            foreach( RaycastHit hit in hits ) {
                if( hit.collider.gameObject.name == "field" ) {
                    pointObj.transform.position = hit.point;
                }
            }
        }
    }
}
