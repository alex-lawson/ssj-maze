using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenu : MonoBehaviour
{
	private Animator animator;
	private CanvasGroup canvasGroup;
	private PlayerInput playerInput;

	private bool isVisible;

	public void ShowMenu()
	{
		isVisible = true;

		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;

		canvasGroup.alpha = 1;
		canvasGroup.blocksRaycasts = true;

		playerInput.DeactivateInput();
	}

	public void HideMenu()
	{
		isVisible = false;

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

		animator.Play("title_fade_out");

		playerInput.ActivateInput();
	}

	public void StartGame()
	{
		HideMenu();
	}

	private void Awake()
	{
		animator = GetComponent<Animator>();
		canvasGroup = GetComponent<CanvasGroup>();
		playerInput = FindObjectOfType<PlayerInput>();
	}

	private void Start()
	{
		ShowMenu();
	}
}
