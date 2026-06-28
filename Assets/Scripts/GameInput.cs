using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }
    private GameControl gameControl;

    private const string GAMEINPUT_BINDINGS = "GameInputBindings";

    public event EventHandler OnInteractAction;
    public event EventHandler OnOperateAction;
    public event EventHandler OnPauseAction;

    public enum BindingType
    {
        Up,
        Down,
        Left,
        Right,
        Interact,
        Operate,
        Pause

    }

    private void Awake()
    {
        Instance = this; //单例模式
        gameControl = new GameControl();
        if (PlayerPrefs.HasKey(GAMEINPUT_BINDINGS))
        {
            gameControl.LoadBindingOverridesFromJson(PlayerPrefs.GetString(GAMEINPUT_BINDINGS));
        }
        //启用Player这个input
        gameControl.Player.Enable();

        //事件performed 订阅发布模式
        gameControl.Player.Interact.performed += Interact_Performed;
        gameControl.Player.Operate.performed += Operate_Performed;
        gameControl.Player.Pause.performed += Pause_Performed;
    }
    //private void Update()
    //{
    //    if (Input.GetMouseButton(0))
    //    {
    //        print("开始绑定");
    //        gameControl.Player.Disable();
    //        gameControl.Player.Move.PerformInteractiveRebinding(1).OnComplete(callback =>
    //        {
    //            print(callback.action.bindings[1].path);
    //            print(callback.action.bindings[1].overridePath);
    //            callback.Dispose();
    //            print("绑定完成");
    //            gameControl.Player.Enable(); //重新启用Player这个input
    //        }).Start();
    //    }
    //}

    /// <summary>
    /// 绑定按键
    /// </summary>
    /// <param name="bindingTyp"></param>
    /// <param name="onComplete"></param>
    public void ReBinding(BindingType bindingTyp,Action onComplete)
    {
        gameControl.Player.Disable();
        InputAction inputAction = null;
        int index = -1;
        switch (bindingTyp)
        {
            case BindingType.Up:
                index = 1;
                inputAction = gameControl.Player.Move;
                break;
            case BindingType.Down:
                index = 2;
                inputAction = gameControl.Player.Move;
                break;
            case BindingType.Left:
                index = 3;
                inputAction = gameControl.Player.Move;
                break;
            case BindingType.Right:
                index = 4;
                inputAction = gameControl.Player.Move;
                break;
            case BindingType.Interact:
                index = 0;
                inputAction = gameControl.Player.Interact;
                break;
            case BindingType.Operate:
                index = 0;
                inputAction = gameControl.Player.Operate;
                break;
            case BindingType.Pause:
                index = 0;
                inputAction = gameControl.Player.Pause;
                break;
            default:
                break;
        }
        inputAction.PerformInteractiveRebinding(index).OnComplete(callback =>
        {
            callback.Dispose();
            gameControl.Player.Enable(); //重新启用Player这个input
            onComplete?.Invoke(); //回调函数

            PlayerPrefs.SetString(GAMEINPUT_BINDINGS, gameControl.SaveBindingOverridesAsJson());
            PlayerPrefs.Save(); //保存设置

        }).Start();
    }
    /// <summary>
    /// 获取按键的显示字符串
    /// </summary>
    /// <param name="bindingType"></param>
    /// <returns></returns>
    public string GetBindingDisplayString(BindingType bindingType)
    {
        switch (bindingType)
        {
            case BindingType.Up:
                return gameControl.Player.Move.bindings[1].ToDisplayString();
            case BindingType.Down:
                return gameControl.Player.Move.bindings[2].ToDisplayString();
            case BindingType.Left:
                return gameControl.Player.Move.bindings[3].ToDisplayString();
            case BindingType.Right:
                return gameControl.Player.Move.bindings[4].ToDisplayString();
            case BindingType.Interact:
                return gameControl.Player.Interact.bindings[0].ToDisplayString();
            case BindingType.Operate:
                return gameControl.Player.Operate.bindings[0].ToDisplayString();
            case BindingType.Pause:
                return gameControl.Player.Pause.bindings[0].ToDisplayString();
            default:
                break;
        }
        return "";
    }
    //private void Start()
    //{
    //    print(gameControl.Player.Move.bindings[1].ToDisplayString());
    //    print(gameControl.Player.Move.bindings[2].ToDisplayString());
    //    print(gameControl.Player.Move.bindings[3].ToDisplayString());
    //    print(gameControl.Player.Move.bindings[4].ToDisplayString());

    //    print(gameControl.Player.Interact.bindings[0].ToDisplayString());
    //}
    private void OnDestroy()
    {
        gameControl.Player.Interact.performed -= Interact_Performed;
        gameControl.Player.Operate.performed -= Operate_Performed;
        gameControl.Player.Pause.performed -= Pause_Performed;

        gameControl.Dispose(); //释放资源
    }

    private void Pause_Performed(InputAction.CallbackContext context)
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    private void Operate_Performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnOperateAction?.Invoke(this, EventArgs.Empty);
    }

    private void Interact_Performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnInteractAction?.Invoke(this,EventArgs.Empty);
    }


    public Vector3 GetMovementDirectionNormalized()
    {
        Vector2 inputVector2 = gameControl.Player.Move.ReadValue<Vector2>();

        //float horizontal = Input.GetAxisRaw("Horizontal");
        //float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(inputVector2.x, 0, inputVector2.y);
        direction = direction.normalized; //单位化向量 （1，0，1）变为（0.7，0，0.7）

        return direction;
    }
}
