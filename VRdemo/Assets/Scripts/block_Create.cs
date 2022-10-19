using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct BlockType {
    public int[ , ] shape;//�`����`����z��
}

public class block_Create : MonoBehaviour {
    //�u���b�N�̃Q�[���I�u�W�F�N�g
    [SerializeField]
    GameObject blockObj;

    //�Ə�
    [SerializeField]
    GameObject pointObj;

    //���˂���u���b�N���j�b�g���`
    GameObject shotObj;

    [SerializeField]
    BlockType[ ] block_Types;

    //���E�ǂ���̃R���g���[�������擾���邽�߂Ɏg�p����Controller�N���X
    OVRInput.Controller contoroller;

    void Start( ) {
        //���E�ǂ��炩�̃R���g���[�������擾
        contoroller = GetComponent<OVRControllerHelper>( ).m_controller;

        //�u���b�N���j�b�g�̎�ނ��쐬
        CreateBlockType( );

        //�u���b�N���j�b�g�����O�ɐ���
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
                //�|�C���^�̉���
                ViewPointer( );
                //A/X�{�^������������u���b�N�𔭎�
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

    //�u���b�N�𔭎˂���֐�
    void Fire( Vector3 startPos, Vector3 direction, GameObject target ) {
        ////�u���b�N�̃R�s�[���������Ȃ��悤�ɃR�����g�A�E�g
        //GameObject go = Instantiate(target);

        //�d�͂�ݒ�
        var rb = target.GetComponent<Rigidbody>( );
        ////�d�͂��g�p���Ȃ�
        //rb.useGravity=true;
        target.transform.parent = null;

        //�u���b�N�̏Փ˔����������
        for( int i = 0; i < target.transform.childCount; i++ ) {
            target.transform.GetChild( i ).GetComponent<BoxCollider>( ).enabled = true;
        }


        //�u���b�N�̔��ˈʒu��ݒ�
        target.transform.position = startPos;
        //�u���b�N���R���g���[�����ʕ����ɕ���
        target.GetComponent<Rigidbody>( ).AddForce( direction * 10f, ForceMode.Impulse );
        //��Ԃ��uWait�v�ɕύX
        GameStatus.status = "Wait";//�A�˂��Ȃ��Ȃ�
        Destroy(pointObj);
    }
    //�u���b�N�𐶐�
    GameObject create_Block( int type_Num,string type ) {
        int size = block_Types[ type_Num ].shape.GetLength( 0 );

        GameObject blockUnits = new GameObject( "BlockUnits" );
        blockUnits.tag = "blockUnits";
        //�������Z��K�p�����邽��Rigidbody��ǉ�
        var rb = blockUnits.AddComponent<Rigidbody>( );
        //�d�͂�������Ȃ��悤��
        rb.useGravity = false;
        //���������u���b�N���j�b�g���������Z�ɂ���]���Ȃ��悤��
        rb.freezeRotation = true;
        //�񎟌��z������[�v�ŏ���
        for( int i = 0; i < block_Types[ type_Num ].shape.GetLength( 0 ); i++ ) {
            for( int j = 0; j < block_Types[ type_Num ].shape.GetLength( 1 ); j++ ) {
                //�u���b�N��z�u����ʒu�ł���΃u���b�N����
                if( block_Types[ type_Num ].shape[ i, j ] == 1 ) {
                    GameObject go = Instantiate( blockObj );
                    go.transform.parent = blockUnits.transform;
                    //�u���b�N�𐶐�����ʒu�͐e�I�u�W�F�N�g�̑��Έʒu�Ō���
                    go.transform.localPosition = new Vector3( i - size / 2, size / 2 - j, 0 ) * 0.1f;
                    //�ꎞ�I�Ƀu���b�N�̏Փ˔�����Ȃ���
                    go.GetComponent<BoxCollider>( ).enabled = false;
                    if(/*�����^�C�v�������œ��͂����ĐF������*/type=="point"){
                        /*go�̐F�ς�����*/go.GetComponent<Renderer>().material.color=new Color(255,255,255,0.3f);
                    }
                }
            }
        }
        return blockUnits;
    }

    //7��ނ̃u���b�N�^�C�v�𐶐�
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
                //�E��]
                Vector3 tmp = shotObj.transform.GetChild( i ).transform.localPosition;
                int x = shotObj.transform.GetChild( i ).GetComponent<Block>( ).x;
                int y = shotObj.transform.GetChild( i ).GetComponent<Block>( ).y;
                shotObj.transform.GetChild( i ).transform.localPosition = new Vector3( -tmp.y, tmp.x, 0 );
                shotObj.transform.GetChild( i ).GetComponent<Block>( ).x = -y;
                shotObj.transform.GetChild( i ).GetComponent<Block>( ).y = x;
            }
            for (int i = 0; i < pointObj.transform.childCount; i++)
            {
                //�E��]
                Vector3 tmp = pointObj.transform.GetChild(i).transform.localPosition;
                int x = pointObj.transform.GetChild(i).GetComponent<Block>().x;
                int y = pointObj.transform.GetChild(i).GetComponent<Block>().y;
                pointObj.transform.GetChild(i).transform.localPosition = new Vector3(-tmp.y, tmp.x, 0);
                pointObj.transform.GetChild(i).GetComponent<Block>().x = -y;
                pointObj.transform.GetChild(i).GetComponent<Block>().y = x;
            }
        } else {
            for( int i = 0; i < shotObj.transform.childCount; i++ ) {
                //����]
                Vector3 tmp = shotObj.transform.GetChild( i ).transform.localPosition;
                int x = shotObj.transform.GetChild( i ).GetComponent<Block>( ).x;
                int y = shotObj.transform.GetChild( i ).GetComponent<Block>( ).y;
                shotObj.transform.GetChild( i ).transform.localPosition = new Vector3( tmp.x, -tmp.y, 0 );
                shotObj.transform.GetChild( i ).GetComponent<Block>( ).x = y;
                shotObj.transform.GetChild( i ).GetComponent<Block>( ).y = -x;
            }
            for (int i = 0; i < pointObj.transform.childCount; i++)
            {
                //����]
                Vector3 tmp = pointObj.transform.GetChild(i).transform.localPosition;
                int x = pointObj.transform.GetChild(i).GetComponent<Block>().x;
                int y = pointObj.transform.GetChild(i).GetComponent<Block>().y;
                pointObj.transform.GetChild(i).transform.localPosition = new Vector3(tmp.x, -tmp.y, 0);
                pointObj.transform.GetChild(i).GetComponent<Block>().x = y;
                pointObj.transform.GetChild(i).GetComponent<Block>().y = -x;
            }
        }
    }

    //�R���g���[���̎w����̕����ɂԂ������Q�[���I�u�W�F�N�g�����ׂĎ擾
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
