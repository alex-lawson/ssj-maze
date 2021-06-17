// GENERATED AUTOMATICALLY FROM 'Assets/Scripts/Input/GameMenuActions.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @GameMenuActions : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @GameMenuActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""GameMenuActions"",
    ""maps"": [
        {
            ""name"": ""Toggle"",
            ""id"": ""ee3b0e2d-a462-4ba6-af68-84ae3f22fcd8"",
            ""actions"": [
                {
                    ""name"": ""ToggleGameMenu"",
                    ""type"": ""Button"",
                    ""id"": ""d99d57b8-3225-4859-8891-94bf97dd9637"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""758e1cf2-fd45-454f-bf81-d9bc750470ef"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ToggleGameMenu"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""94733546-9ca4-40a5-a5e1-602f01748828"",
                    ""path"": ""<Keyboard>/enter"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ToggleGameMenu"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Toggle
        m_Toggle = asset.FindActionMap("Toggle", throwIfNotFound: true);
        m_Toggle_ToggleGameMenu = m_Toggle.FindAction("ToggleGameMenu", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Toggle
    private readonly InputActionMap m_Toggle;
    private IToggleActions m_ToggleActionsCallbackInterface;
    private readonly InputAction m_Toggle_ToggleGameMenu;
    public struct ToggleActions
    {
        private @GameMenuActions m_Wrapper;
        public ToggleActions(@GameMenuActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @ToggleGameMenu => m_Wrapper.m_Toggle_ToggleGameMenu;
        public InputActionMap Get() { return m_Wrapper.m_Toggle; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(ToggleActions set) { return set.Get(); }
        public void SetCallbacks(IToggleActions instance)
        {
            if (m_Wrapper.m_ToggleActionsCallbackInterface != null)
            {
                @ToggleGameMenu.started -= m_Wrapper.m_ToggleActionsCallbackInterface.OnToggleGameMenu;
                @ToggleGameMenu.performed -= m_Wrapper.m_ToggleActionsCallbackInterface.OnToggleGameMenu;
                @ToggleGameMenu.canceled -= m_Wrapper.m_ToggleActionsCallbackInterface.OnToggleGameMenu;
            }
            m_Wrapper.m_ToggleActionsCallbackInterface = instance;
            if (instance != null)
            {
                @ToggleGameMenu.started += instance.OnToggleGameMenu;
                @ToggleGameMenu.performed += instance.OnToggleGameMenu;
                @ToggleGameMenu.canceled += instance.OnToggleGameMenu;
            }
        }
    }
    public ToggleActions @Toggle => new ToggleActions(this);
    public interface IToggleActions
    {
        void OnToggleGameMenu(InputAction.CallbackContext context);
    }
}
