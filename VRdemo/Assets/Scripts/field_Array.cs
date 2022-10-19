
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using TMPro;
using UnityEngine;

public class field_Array : MonoBehaviour
{
    //�u���b�N���z�u����Ă����true,�����łȂ����false
    bool[,] blocks = new bool[10, 20];

    [SerializeField]
    GameObject score_View_Obj;
    [SerializeField]
    List<GameObject> blockList;
    //�X�R�A�p��TextMesh
    [SerializeField]
    TextMeshPro scoreTextMesh;
    [SerializeField]
    GameObject deleteEffect;

    //�X�R�A�̒l
    int score;

    //�u���b�N�������������ۂɒǉ�����|�C���g
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
                //�P�b�u���b�N��������Ȃ�������Shot��Ԃɖ߂�
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
                    //FallBlocks�̏��������܂������Ă��炸false�𖞂����Ȃ����ߔ��ˑҋ@�̏�Ԃɖ߂�Ȃ�
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

    //block������
    void InitBlocks()
    {
        //GetLength=�����擾
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

    //������������s
    void OnCollisionEnter(Collision other)
    {
        //var p = other.transform.position;
        //var b = other.gameObject.transform.rotation;
        //var u = 0.1f;
        //var g = new Vector3(
        //    ( float )( ( int )( b.x / u ) ) * u , //x���W
        //    ( float )( ( int )( b.y / u ) ) * u , //y���W
        //    transform.position.z         //z���W
        //    );

        ////���W�𔽉f
        //other.gameObject.transform.position = g;
        ////���������ɂ���
        //other.gameObject.transform.rotation = Quaternion.Euler( 0 , 0 , 0 );

        //var rb = other.gameObject.GetComponent<Rigidbody>( );
        //Destroy( rb );
        ////Instantiate = prefab�������I�u�W�F�N�g�𐶐�
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
            ((int)(b.x / u)) * u,           //x���W
            ((int)(b.y / u)) * u,           //y���W
            transform.position.z        //z���W
            );

        //���������ɂ���
        other.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);

        //�������W���`
        float px = -0.4f;
        float py = 0.1f;

        //blocks�̗v�f�ԍ����擾
        int bx = GetPosition((g.x - px) / u, 0.01f, 0, 10);
        int by = GetPosition((g.y - py) / u, 0.01f, 0, 20);

        //�͈͊O�Ȃ�G���[��Ԃ�
        if (bx < 0 || by < 0 || bx >= 10 || by >= 20 || CheckExistBlock(other.gameObject, bx, by))
        {
            GameStatus.status = "Shot";
            for (int i = 0; i < other.gameObject.transform.childCount; i++)
            {
                Destroy(other.gameObject.transform.GetChild(i).transform.GetComponent<BoxCollider>());
            }
            //ForceMode.Impulse=��̊֐��Ăяo���ŏu���ɏՌ��͂�K�p�B������Փ˂̗͂Ƃ��āA�u���ɋN����͂�K�p����̂ɗL�p
            //AddForce�Ńt�B�[���h���璵�˕Ԃ�悤�ȕ����Ƀu���b�N�����ł����悤�ɏ���
            other.gameObject.transform.GetComponent<Rigidbody>().AddForce(Vector3.back * 10f, ForceMode.Impulse);
        }
        else
        {
            //���W�𔽉f
            other.gameObject.transform.position = new Vector3(bx * u + px, by * u + py, transform.position.z);
            //�z�u���ꂽ�ʒu�̔z���true�ɂ���
            ApplyBlockUnits(other.gameObject, bx, by);

            // �ʒu��\��
            GameObject sv = Instantiate(score_View_Obj);
            sv.transform.position = other.gameObject.transform.position;
            sv.GetComponent<score_Representation>().textMesh.text = $"({bx},{by})";


            //�e�I�u�W�F�N�g�̍폜
            int cnt = other.transform.childCount;
            for (int i = 0; i < cnt; i++)
            {
                other.transform.GetChild(0).parent = null;
            }
            other.transform.tag = "Untagged";
            other.transform.position = Vector3.forward * 1000f;

        }

        ////�z�u���ꂽ�ʒu�̔z���true�ɂ���
        //blocks[ bx , by ] = true;
        //ApplyBlockUnits( other.gameObject, bx, by );
        ////�Q�Ƃł���悤�ɖ��O��ݒ�
        //other.gameObject.name = $"name:{bx},{by}";

        //var rb = other.gameObject.GetComponent<Rigidbody>( );
        //Destroy( rb );
        ////}
    }

    //���̈ʒu�Ƀu���b�N�����݂��邩�`�F�b�N����֐�
    bool CheckExistBlock(GameObject target, int x, int y)
    {
        for (int i = 0; i < target.transform.childCount; i++)
        {
            Vector3 g = target.transform.GetChild(i).localPosition;
            int bx = (int)(g.x * 10);
            int by = (int)(g.y * 10);
            //�g�O�ɂ̓u���b�N�����݂��邱�Ƃɂ��Ă���
            if (x + bx < 0 || y + by < 0 || x + bx >= 10 || y + by >= 20)
            {
                return true;
            }
            //�ix+bx,y+by�j�̈ʒu�Ƀu���b�N�����݂��Ă��邩�ǂ����`�F�b�N
            if (!(x + bx < 0 || x + bx >= 10 || y + by < 0 || y + by >= 20) && blocks[x + bx, y + by])
            {
                return true;
            }
        }
        return false;

    }
    void ApplyBlockUnits(GameObject target, int x, int y)
    {
        //���L�̃R�[�h�������list�̒��g���͂���Ȃ��Ȃ�A����Ȃ���Η�����Ƃ���܂ł͎��s����邪���u���b�N����������蔻��͂Ƃ�邪���ɗ����Ȃ��Ȃ�B
        blockList = new List<GameObject>();
        for (int i = 0; i < target.transform.childCount; i++)
        {
            Vector3 g = target.transform.GetChild(i).localPosition;
            int bx = (int)(g.x * 10);
            int by = (int)(g.y * 10);
            blocks[x + bx, y + by] = true;
            //����p�̍��W��ݒ�
            target.transform.GetChild(i).GetComponent<Block>().x = x + bx;
            target.transform.GetChild(i).GetComponent<Block>().y = y + by;
            //�Q�Ƃł���悤�ɖ��O��ݒ�
            target.transform.GetChild(i).name = $"name:{x + bx},{y + by}";
            //�����p�Q�[���I�u�W�F�N�g�ɐݒ�
            Debug.Log("�����܂œ��B");
            //blocklist�ւ̑�����N�����ĂȂ��H
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

    //�u���b�N����i�������������鏈��
    //1.�u���b�N���X�g�����������Ƀ\�[�g
    //2.�S�Ẵu���b�N�̈���̈ʒu�Ƀu���b�N�����݂��邩�`�F�b�N
    //  1.���݂��Ȃ���΃u���b�N���Ĕz�u
    //  2.���݂���Η����������I��������i�Ԃ�l��False�ŕԂ��j
    bool FallBlocks()
    {
        var retblocks = blocks.Clone() as bool[,];
        //�\�[�g
        SortBlockList();

        //�u���b�N�����Ƃ����Ԃɂ��邩�`�F�b�N
        bool isFall = true;

        //����Ƀu���b�N�����邩�`�F�b�N
        foreach (GameObject go in blockList)
        {
            int x = go.GetComponent<Block>().x;
            int y = go.GetComponent<Block>().y;
            if (y - 1 < 0 || retblocks[x, y - 1])
            {
                Debug.Log("���B");
                isFall = false;
                break;
            }
            retblocks[x, y] = false;
        }
        Debug.Log("isfall=" + isFall);
        if (isFall)
        {
            //�u���b�N��z�u
            foreach (GameObject go in blockList)
            {
                //blockList�ɗv�f�������Ă��炸����for��������Ă��Ȃ��͗l
                int x = go.GetComponent<Block>().x;
                int y = go.GetComponent<Block>().y;
                Debug.Log("�����܂œ��B");
                go.transform.position += Vector3.down * 0.1f;

                //����8�����݂��Ȃ��H
                blocks[x, y] = false;
                blocks[x, y - 1] = true;
                Debug.Log(go.name);
                go.GetComponent<Block>().y -= 1;
                go.name = $"name:{x},{y - 1}";
            }
        }
        return isFall;
    }
    //�����Ă���񂪂��邩�ǂ������`�F�b�N���Ă��̗�̃u���b�N���폜����֐�
    void CheckLines()
    {
        for (int i = 0; i < 20; i++)
        {
            bool isDelete = true;
            for (int j = 0; j < 10; j++)
            {
                //�ォ�珇�ɒ��ׂ������߁A�ui�v�ł͂Ȃ��u20-i-1�v���g�p����
                if (!blocks[j, 20 - i - 1])
                {
                    isDelete = false;
                    break;
                }
            }
            //�폜�o�����ł���΍폜
            if (isDelete)
            {
                //���u���b�N���폜
                DeleteBlocks(20 - i - 1);
                //�������������u���b�N��������
                DropBlocks(20 - i - 1);

                //�X�R�A��ǉ�
                score += DELETE_POINT;
                scoreTextMesh.text = "" + score;
            }
        }
    }
    //1��u���b�N���폜���鏈��
    void DeleteBlocks(int h)
    {
        GameObject[] glist = GameObject.FindGameObjectsWithTag("block");
        foreach (GameObject go in glist)
        {
            if (go.GetComponent<Block>().y == h)
            {
                //DeleteBlocks()�ɂ����������u���b�N���킩��B���̂��߂��̃u���b�N�̈ʒu��DeleteEffect�����������B
                GameObject deleteEffectClone = Instantiate(deleteEffect);
                deleteEffectClone.transform.position = go.transform.position + Vector3.back * 0.1f;
                deleteEffectClone.transform.localScale = Vector3.one * 0.1f;
                blocks[go.GetComponent<Block>().x, h] = false;
                Destroy(go);
            }
        }
    }
    //���������荂���ʒu�̃u���b�N�𗎉������鏈��
    void DropBlocks(int h)
    {
        //����������u���b�N��o�^
        List<GameObject> glist = new List<GameObject>(GameObject.FindGameObjectsWithTag("block"));
        glist.OrderBy((x) => x.GetComponent<Block>().y);

        foreach (GameObject go in glist)
        {
            if (go.GetComponent<Block>().y > h)
            {
                //blocks���X�V
                int x = go.GetComponent<Block>().x;
                int y = go.GetComponent<Block>().y;
                blocks[x, y] = false;
                blocks[x, y - 1] = true;

                //y���W�����炷
                go.GetComponent<Block>().y -= 1;

                //GameObject�̍��W�����炷
                go.transform.position += Vector3.down * 0.1f;
            }
        }
    }
}