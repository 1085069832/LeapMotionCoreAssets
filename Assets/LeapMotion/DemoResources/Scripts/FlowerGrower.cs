/******************************************************************************\
* Copyright (C) Leap Motion, Inc. 2011-2014.                                   *
* Leap Motion proprietary. Licensed under Apache 2.0                           *
* Available at http://www.apache.org/licenses/LICENSE-2.0.html                 *
\******************************************************************************/

using UnityEngine;
using System.Collections;

public class FlowerGrower : MonoBehaviour
{
    public float growthRate = 1.0f;
    public float deathRate = 1.0f;
    public float growthProgress = 0.0f;//花生长的进程度

    public StemMesh stem;
    public float stemStartGrowth = 0.0f;//开始生长枝干的时间
    public float stemEndGrowth = 0.6f;//枝干生长完成的时间

    public Transform flowerHead;
    public float flowerHeadStartGrowth = 0.5f;
    public float flowerHeadEndGrowth = 0.8f;

    public Light lightSource;
    public float lightSourceStartGrowth = 0.0f;
    public float lightSourceEndGrowth = 0.8f;

    public PetalMesh[] leaves;
    public float leavesStartGrowth = 0.4f;
    public float leavesEndGrowth = 0.8f;

    public PetalMesh[] pedals;
    public float pedalsStartGrowth = 0.8f;
    public float pedalsEndGrowth = 1.0f;

    public FlowerBloom flowerToBloom;//花瓣

    private Vector3 flower_head_scale_ = Vector3.zero;
    private float light_source_intensity_ = 0.0f;
    //记录花的状态
    private bool growing_ = true;
    private bool dieing_ = false;
    bool isIns;
    [SerializeField] Transform pos;

    [SerializeField] GameObject flower;

    void Start()
    {
        //Instantiate(flower, transform.position + Vector3.right, transform.rotation);
        flower_head_scale_ = flowerHead.localScale;//记录花蕾自身缩放
        light_source_intensity_ = lightSource.intensity;//灯光
        flowerHead.localScale = Vector3.zero;//设置花中心自身缩放为0

        foreach (PetalMesh pedal in pedals)
            pedal.growthProgress = 0;  //设置花瓣生长度

        foreach (PetalMesh leaf in leaves)
            leaf.growthProgress = 0; //设置叶生长度
    }

    float ComputeGrowthAmount(float start, float end)
    {
        return Mathf.Clamp((growthProgress - start) / (end - start), 0.0f, 1.0f);
    }
    /// <summary>
    /// 设置花生长度
    /// </summary>
    void SetSizes()
    {
        //枝干生长度
        stem.growthProgress = ComputeGrowthAmount(stemStartGrowth, stemEndGrowth);
        //花蕾缩放度
        float flower_head_growth = ComputeGrowthAmount(flowerHeadStartGrowth, flowerHeadEndGrowth);
        flowerHead.localScale = flower_head_growth * flower_head_scale_;
        //花灯光强度
        float light_source_growth = ComputeGrowthAmount(lightSourceStartGrowth, lightSourceEndGrowth);
        lightSource.intensity = light_source_growth * light_source_intensity_;
        //叶生长度
        float leaf_growth = ComputeGrowthAmount(leavesStartGrowth, leavesEndGrowth);
        foreach (PetalMesh leaf in leaves)
            leaf.growthProgress = leaf_growth;
        //花瓣生长度
        float pedal_growth = ComputeGrowthAmount(pedalsStartGrowth, pedalsEndGrowth);
        foreach (PetalMesh pedal in pedals)
            pedal.growthProgress = pedal_growth;
    }

    public void RemoveStump()
    {
        stem.RemoveStump();
    }

    public bool IsStumpClear()
    {
        return stem.IsStumpClear();
    }

    public void Die()
    {
        dieing_ = true;
    }

    public bool IsDead()
    {
        return growthProgress == 0.0f;
    }

    public bool IsBroken()
    {
        return stem.IsBroken();
    }

    public bool IsGrabbed()
    {
        GrabbableObject[] grabbables = GetComponentsInChildren<GrabbableObject>();
        foreach (GrabbableObject grabbable in grabbables)
        {
            if (grabbable.IsGrabbed())
                return true;
        }
        return false;
    }

    void Update()
    {
        if (dieing_)//凋谢
            growthProgress = Mathf.Clamp(growthProgress - Time.deltaTime * deathRate, 0.0f, 1.0f);
        else if (growing_)//生长
            growthProgress = Mathf.Clamp(growthProgress + Time.deltaTime * growthRate, 0.0f, 1.0f);
        //花生长
        SetSizes();
        //生长度达到1开启花
        if (growthProgress == 1.0f && flowerToBloom != null)
            flowerToBloom.open = true;

        print("IsBroken" + IsBroken());

        if (IsGrabbed() && !isIns)
        {
            dieing_ = true;
            isIns = true;
            Instantiate(flower, pos.position, Quaternion.identity);
            print("生成花" + isIns);
        }
    }

}
