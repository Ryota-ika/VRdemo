using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//���̃R�[�h��animator�̃A�j���[�V������FinalScene�ɂȂ����^�C�~���O�ő������u���b�N����������
public class DeleteEffect : MonoBehaviour
{
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator= GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        var a=animator.GetCurrentAnimatorStateInfo(0);
        if( a.IsName("FinalScene") ) {
            Destroy(this.gameObject);
        }
    }
}
