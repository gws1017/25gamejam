using UnityEngine;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    public GameObject[] prefabs; //스폰할 프리팹 모음

    private List<GameObject>[] pools; //실제 객체 보관 풀


    private void Awake()
    {

        //풀 초기화
        pools = new List<GameObject>[prefabs.Length];

        for(int i =0; i<pools.Length; ++i)
        {
            pools[i] = new List<GameObject>();
        }
    }

    //하나 꺼내오기
    public GameObject GetObject(int index)
    {
        GameObject select = null;

        foreach (GameObject item in pools[index])
        {
            if(!item.activeSelf)
            {
                select = item;
                select.SetActive(true);
                break;
            }
        }

        //없으면 생성
        if(!select)
        {
            select = Instantiate(prefabs[index], transform);
            pools[index].Add(select);
        }
        return select;
    }

}
