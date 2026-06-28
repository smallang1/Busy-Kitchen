using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveCounter : BaseCounter
{
    [SerializeField]private FryingRecipListSO fryingRecipeList;
    [SerializeField]private FryingRecipListSO bruningRecipeList;

    [SerializeField]private StoveCounterVisual stoveCounterVisual;
    [SerializeField]private ProgressBarUI progressBarUI;
    [SerializeField]private AudioSource sound;

    private void Start()
    {
        warningControl = GetComponent<WarningControl>();
    }
    public enum StoveState
    {
        Idle,
        Frying,
        Burning,
    }

    private FryingRecipe fryingRecipe;
    private float fryingTimer = 0;
    private StoveState state = StoveState.Idle;
    private WarningControl warningControl;
    public override void Interact(Player player)
    {
        if (player.IsHaveKitchenObject())
        {
            //手上有食材
            //GetKitchenObject() 返回值为kitchenObject食物
            //GetKitchenObjectSO() 返回值为  kitchenObjectSO 数据化食物对象
            if (IsHaveKitchenObject() == false )
            {
                if(fryingRecipeList.TryGetFryingRecipe(
                player.GetKitchenObject().GetKitchenObjectSO(), out FryingRecipe fryingRecipe))
                {
                    TransferKitchenObject(player, this);
                    StartFaying(fryingRecipe);
                }
                else if (bruningRecipeList.TryGetFryingRecipe(
                 player.GetKitchenObject().GetKitchenObjectSO(), out FryingRecipe burningRecipe))
                {
                    TransferKitchenObject(player, this);
                    StartBurning(burningRecipe);
                }
                else
                {

                }
            }
            else
            {
                //没有食材

            }
        }
        else
        {
            //手上没食材
            if (IsHaveKitchenObject() == false)
            {
                //当前柜台为空
            }
            else
            {
                //当前柜台不为空
                TurnToIdle();
                TransferKitchenObject(this, player); //将食材转移到玩家手中
            }
        }
    }
    private void Update()
    {
        switch (state)
        {
            case StoveState.Idle:
                break;
            case StoveState.Frying:
                fryingTimer += Time.deltaTime;
                progressBarUI.UpdateProgress(fryingTimer / fryingRecipe.fryingTime); //更新进度条
                if(fryingTimer >= fryingRecipe.fryingTime)
                {
                    DestoryKitchenOject();
                    CreatKitchenObject(fryingRecipe.output.prefab);


                    bruningRecipeList.TryGetFryingRecipe(GetKitchenObject().GetKitchenObjectSO(),
                    out FryingRecipe newFryingRecipe);
                    StartBurning(newFryingRecipe);
                }
                break;
            case StoveState.Burning:
                float warningTimeNormalized = .5f;
                if (fryingTimer / fryingRecipe.fryingTime > warningTimeNormalized)
                {
                    warningControl.ShowWarning();
                }
                fryingTimer += Time.deltaTime;
                progressBarUI.UpdateProgress(fryingTimer / fryingRecipe.fryingTime); //更新进度条
                if (fryingTimer >= fryingRecipe.fryingTime)
                {
                    DestoryKitchenOject();
                    CreatKitchenObject(fryingRecipe.output.prefab);
                    TurnToIdle();
                }           
                break;
            default:
                break;
        }
    }
    private void StartFaying(FryingRecipe fryingRecipe)
    {
        fryingTimer = 0;
        this.fryingRecipe = fryingRecipe;
        state = StoveState.Frying;
        stoveCounterVisual.ShowStoveEffect(); //显示炉子效果
        sound.Play(); //播放声音
    }
    private void StartBurning(FryingRecipe fryingRecipe)
    {
        if(fryingRecipe == null)
        {
            Debug.LogWarning("无法获得Burning的食谱，无法进行Burning");
            TurnToIdle();
            return;
        }
        stoveCounterVisual.ShowStoveEffect(); //显示炉子效果
        fryingTimer = 0;
        this.fryingRecipe = fryingRecipe;
        state = StoveState.Burning;
        sound.Play(); //播放声音
    }
    private void TurnToIdle()
    {
        progressBarUI.Hide(); //隐藏进度条
        state = StoveState.Idle;
        stoveCounterVisual.HideStoveEffect(); //隐藏炉子效果
        sound.Pause(); //暂停声音
        warningControl.StopWarning(); //停止警告
    }
}
