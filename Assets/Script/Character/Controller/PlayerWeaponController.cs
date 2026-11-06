
using Assets.Script.Weapon.Throwable_Weapon;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    public GameObject Weapon1;
    public GameObject Weapon2;
    public List<GameObject> ListObj;
    public bool haveOneWepon;
    public BaseWeapon baseWeapon { get; set; }
    public void Attack()
    {
        baseWeapon.Attack();
    }

    public void RotateWeapon()
    {
        baseWeapon.RotateWeapon();
    }

    private static GameObject sceneHolder;
    void Start()
    {
        baseWeapon = GetComponentInChildren<BaseWeapon>();
        ListObj = GetAllChild();
        foreach (GameObject obj in ListObj)
        {
            obj.SetActive(false);
        }
        if (ListObj.Count == 1)
        {
            Weapon1 = ListObj[0];
            haveOneWepon = true;
            Weapon1.SetActive(true);
            WeaponUI.Instance.SetImageAndActiveWeapon1(baseWeapon.GetWeaponDetailSO().weaponImage);
        }
        else
        {
            Weapon1 = ListObj[0];
            Weapon2 = ListObj[1];
            haveOneWepon = false;
            Weapon1.SetActive(true);
            Weapon2.SetActive(false);
        }

    }

    public void ChangeWeapon(int numPress)
    {
        if (haveOneWepon)
        {
            return;
        }
        switch (numPress)
        {
            case 1:

                Weapon1.SetActive(true);
                Weapon2.SetActive(false);
                baseWeapon = GetComponentInChildren<BaseWeapon>();
                GameManager.Instance.SetWeaponUsing(baseWeapon.GetWeaponDetailSO().weaponName);
                WeaponUI.Instance.SetImageAndActiveWeapon1(baseWeapon.GetWeaponDetailSO().weaponImage);

                break;

            case 2:
                Weapon1.SetActive(false);
                Weapon2.SetActive(true);
                baseWeapon = GetComponentInChildren<BaseWeapon>();
                GameManager.Instance.SetWeaponUsing(baseWeapon.GetWeaponDetailSO().weaponName);
                WeaponUI.Instance.SetImageAndActiveWeapon2(baseWeapon.GetWeaponDetailSO().weaponImage);
                break;
        }
    }

    List<GameObject> GetAllChild()
    {
        List<GameObject> list = new List<GameObject>();
        Transform parentTransform = this.transform;
        foreach (Transform child in parentTransform)
        {
            list.Add(child.gameObject);
        }
        return list;
    }

    public void ChangeAndAddWeaponIfHaveOne(GameObject obj)
    {
        if (haveOneWepon)
        {
            AddWeapon(obj);
        }
        else
        {
            ChangeWeapon(obj);
        }

    }

    private void AddWeapon(GameObject newWeapon)
    {
        ListObj.Add(newWeapon);
        Weapon1.SetActive(false);
        Weapon2 = newWeapon;
        Weapon2.SetActive(true);
        haveOneWepon = false;
        baseWeapon = GetComponentInChildren<BaseWeapon>();
        GameManager.Instance.SetWeaponUsing(baseWeapon.GetWeaponDetailSO().weaponName);
        WeaponUI.Instance.SetImageAndActiveWeapon2(baseWeapon.GetWeaponDetailSO().weaponImage);
    }

    private void ChangeWeapon(GameObject newWeapon)
    {
        if (sceneHolder == null)
        {
            sceneHolder = new GameObject("SceneObjectHolder");
        }
        if (Weapon1.activeInHierarchy)
        {
            Weapon1.transform.SetParent(sceneHolder.transform, true);
            if (Weapon1.TryGetComponent<IPick>(out var weapon))
            {
                weapon.Drop();
            }
            Weapon1 = newWeapon;
            ListObj[0] = newWeapon;
            Weapon1.SetActive(true);
            Weapon2.SetActive(false);
            baseWeapon = GetComponentInChildren<BaseWeapon>();
            GameManager.Instance.SetWeaponUsing(baseWeapon.GetWeaponDetailSO().weaponName);
            WeaponUI.Instance.SetImageAndActiveWeapon1(baseWeapon.GetWeaponDetailSO().weaponImage);
        }
        else if (Weapon2.activeInHierarchy)
        {
            Weapon2.transform.SetParent(sceneHolder.transform, true);
            if (Weapon2.TryGetComponent<IPick>(out var weapon))
            {
                weapon.Drop();
            }
            Weapon2 = newWeapon;
            ListObj[1] = newWeapon;
            Weapon1.SetActive(false);
            Weapon2.SetActive(true);
            baseWeapon = GetComponentInChildren<BaseWeapon>();
            GameManager.Instance.SetWeaponUsing(baseWeapon.GetWeaponDetailSO().weaponName);
            WeaponUI.Instance.SetImageAndActiveWeapon2(baseWeapon.GetWeaponDetailSO().weaponImage);
        }
    }
}
