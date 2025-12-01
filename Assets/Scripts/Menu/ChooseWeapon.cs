using UnityEngine;
using UnityEngine.SceneManagement;

public class ChooseWeapon : MonoBehaviour
{
    private void SetWeaponAndLoad(GameManager.WeaponType weapon)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetWeapon(weapon);
        }

        // Load scene index 1 after choosing the weapon
        SceneManager.LoadScene(1);
    }

    public void ChooseSword()
    {
        SetWeaponAndLoad(GameManager.WeaponType.Sword);
    }

    public void ChooseSpear()
    {
        SetWeaponAndLoad(GameManager.WeaponType.Spear);
    }

    public void ChooseBow()
    {
        SetWeaponAndLoad(GameManager.WeaponType.Bow);
    }

    public void ChooseAmulet()
    {
        SetWeaponAndLoad(GameManager.WeaponType.Amulet);
    }
}
