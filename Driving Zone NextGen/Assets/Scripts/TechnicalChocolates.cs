using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechnicalChocolates : MonoBehaviour
{
    public GameObject panelWithMessage;
	

    public void ShowMessage()
	{
		panelWithMessage.SetActive(true);
		DisactivateMessage();
	}

	public IEnumerator DisactivateMessage()
	{
		yield return new WaitForSeconds(1.5f);
		panelWithMessage.SetActive(false);
	}
}
