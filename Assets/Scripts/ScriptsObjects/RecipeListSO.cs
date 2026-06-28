using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class RecipeListSO : ScriptableObject
{
    public List<RecipeSO> recipeSOList; //存储所有 食谱集合
}
