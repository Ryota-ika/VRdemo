using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class score_Representation : MonoBehaviour
{
    //60�t���[������Text��\������Q�[���I�u�W�F�N�g�𐶐�
    [SerializeField]
    public TextMesh textMesh;

    [SerializeField]
    int cnt = 0;
    const int MAXCNT = 60;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        cnt++;
        cnt %= 60;
        if (cnt == 0)
        {
            Destroy(this.gameObject);
        }
        transform.position += Vector3.up * 0.05f;
    }
}
