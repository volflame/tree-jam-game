using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SpawnGhost : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject ghostPrefab;
    void Start()
    {
        StartCoroutine(Spawn());
    }

    // Update is called once per frame
    IEnumerator Spawn()
    {
        while (true)
        {
            Instantiate(ghostPrefab, new Vector2(Random.Range(-10, 10), Random.Range(0, 10)), Quaternion.identity);
            yield return new WaitForSeconds(5f);    
        }
    }
}
