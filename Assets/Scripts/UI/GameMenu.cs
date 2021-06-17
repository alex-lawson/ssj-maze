using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameMenu : MonoBehaviour
{
	private GameMenuActions menuActions;
	private CanvasGroup canvasGroup;
	private Animator animator;
	private PlayerInput playerInput;

	private bool isVisible;

	public void ToggleMenu()
	{
		if (!isVisible)
		{
			ShowMenu();
		}
		else
		{
			HideMenu();
		}
	}

	public void ShowMenu()
	{
		isVisible = true;

		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;

		animator.Play("gamemenu_fade_in");

		playerInput.DeactivateInput();
	}

	public void HideMenu()
	{
		isVisible = false;

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

		animator.Play("gamemenu_fade_out");

		playerInput.ActivateInput();
	}

	public void ContinueGame()
	{
		HideMenu();
	}

	public void QuitGame()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
         Application.OpenURL(Application.absoluteURL);
#else
         Application.Quit();
#endif
	}

	private void Awake()
	{
		menuActions = new GameMenuActions();
		animator = GetComponent<Animator>();
		canvasGroup = GetComponent<CanvasGroup>();
		playerInput = FindObjectOfType<PlayerInput>();
	}

	private void OnEnable()
	{
		menuActions.Enable();
	}

	private void OnDisable()
	{
		menuActions.Disable();
	}

	private void Start()
	{
		menuActions.Toggle.ToggleGameMenu.performed += _ => { ToggleMenu(); };
	}
}
