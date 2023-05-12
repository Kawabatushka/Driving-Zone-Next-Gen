using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechnicalChocolates : MonoBehaviour
{
    public GameObject panelWithMessage;
	

    public void ShowMessage()
	{
		StartCoroutine(DisactivateMessage());
	}

	public IEnumerator DisactivateMessage()
	{
		panelWithMessage.SetActive(true);
		yield return new WaitForSeconds(1.5f);
		panelWithMessage.SetActive(false);
	}
}
