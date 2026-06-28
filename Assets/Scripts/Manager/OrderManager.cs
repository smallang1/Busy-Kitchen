using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance { get; private set; }

    public event EventHandler OnRecipeWpawed;
    public event EventHandler OnRecipeSucceed;
    public event EventHandler OnRecipeFailed;


    [SerializeField] private RecipeListSO recipeSOList;
    [SerializeField] private int orderMaxCount = 5; //��󶩵���
    [SerializeField] private float orderRate = 2;

    private List<RecipeSO> orderRecipeSOList = new List<RecipeSO>(); //�洢���ж���
    
    private float orderTimer = 0;
    private bool isStartOrder = false;
    private int orderCount = 0;
    private int successDeliveryCount = 0;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
    }

    private void GameManager_OnStateChanged(object sender, EventArgs e)
    {
        if (GameManager.Instance.IsGamePlayingState())
        {
            StartSpanOrder(); //��ʼ���ɶ���
        }
    }

    private void Update()
    {
        if (isStartOrder)
        {
            OrderUpdate();
        }
    }
    private void OrderUpdate()
    {
        orderTimer += Time.deltaTime;
        if(orderTimer >= orderRate)
        {
            orderTimer = 0;
            OrderANewRecipe();
        }
    }

    private void OrderANewRecipe()
    {
        if (orderRecipeSOList.Count >= orderMaxCount)
            return;
        orderCount++;
        //������Сֵ�����������ֵ
        int index = UnityEngine.Random.Range(0, recipeSOList.recipeSOList.Count);
        orderRecipeSOList.Add(recipeSOList.recipeSOList[index]);
        OnRecipeWpawed?.Invoke(this, EventArgs.Empty);
    }

    public bool DelivertRecipe(PlateKitchenObject plateKitchenObject)
    {
        RecipeSO correctRecipe = null;
        foreach (RecipeSO recipe in orderRecipeSOList)
        {
            if(IsCorrect(recipe, plateKitchenObject))
            {
                correctRecipe = recipe;
                break;
            }
        }
        if(correctRecipe == null)
        {
            OnRecipeFailed?.Invoke(this, EventArgs.Empty);
            return false;
        }
        else
        {
            orderRecipeSOList.Remove(correctRecipe);
            OnRecipeSucceed?.Invoke(this, EventArgs.Empty); 
            successDeliveryCount++;
            return true;
        }
    }

    private bool IsCorrect(RecipeSO recipe, PlateKitchenObject plateKitchenObject)
    {
        List<KitchenObjectSO> list1 = recipe.kitchenObjectSOList;
        List<KitchenObjectSO> list2 = plateKitchenObject.GetKitchenObjectSOList();
        if (list1.Count != list2.Count)
            return false;
        foreach(KitchenObjectSO kitchenObjectSO in list1)
        {
            if(list2.Contains(kitchenObjectSO) == false)
            {
                return false;
            }
        }
        return true;
    }
    public List<RecipeSO> GetOrderList()
    {
        return orderRecipeSOList; //�������ж�������
    }
    public void StartSpanOrder()
    {
        isStartOrder = true;
        OrderANewRecipe();
    }
    public void TriggerRecipeSucceed() { OnRecipeSucceed?.Invoke(this, EventArgs.Empty); }
    public void TriggerRecipeFailed() { OnRecipeFailed?.Invoke(this, EventArgs.Empty); }
    public int GetSuccessDeliveryCount()
    {
        return successDeliveryCount; //���سɹ��ϲ˴���
    }
}