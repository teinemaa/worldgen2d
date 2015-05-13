using System;
using BlockEngine;
using UnityEngine;
using System.Collections;

public abstract class BlockWorldGenerator : MonoBehaviour
{

    public abstract BlockProperties GetBlockProperties(int x, int y);

    public abstract BackgroundProperties GetBackgroundProperties(int x, int y);

    public abstract BlockProperties[] GetAllBlockProperies();
    public abstract BackgroundProperties[] GetAllBackgroundProperies();

}
