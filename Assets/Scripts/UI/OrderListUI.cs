using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderListUI : MonoBehaviour
{
    [SerializeField] private Transform recipeParent;
    [SerializeField] private RecipeUI recipeUITemplate;

    private void Start()
    {
        recipeUITemplate.gameObject.SetActive(false);
        OrderManager.Instance.OnRecipeWpawed += OrderManager_OnRecipeWpawed;
        OrderManager.Instance.OnRecipeSucceed += OrderManager_OnRecipeSucceed;
    }

    private void OrderManager_OnRecipeSucceed(object sender, EventArgs e)
    {
        UpdateUI();
    }

    private void OrderManager_OnRecipeWpawed(object sender, EventArgs e)
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        foreach(Transform child in recipeParent)
        {
            if(child != recipeUITemplate.transform)
            {
                Destroy(child.gameObject);
            }
        }
         List<RecipeSO> recipeSOList=OrderManager.Instance.GetOrderList();
        foreach(RecipeSO recipeSO in recipeSOList)
        {
            RecipeUI recipeUI= GameObject.Instantiate(recipeUITemplate);
            recipeUI.transform.SetParent(recipeParent); //设置父对象为recipeParent
            recipeUI.gameObject.SetActive(true);
            recipeUI.UpdateUI(recipeSO); //更新UI
        }
    }
}
