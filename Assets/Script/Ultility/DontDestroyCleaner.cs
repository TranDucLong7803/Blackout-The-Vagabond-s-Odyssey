using UnityEngine;
using UnityEngine.SceneManagement;

public class DontDestroyCleaner : MonoBehaviour
{
    public static void ClearDDOL()
    {
        GameObject temp = new GameObject("Temp_DDOL");
        DontDestroyOnLoad(temp);
        Scene ddolScene = temp.scene;
        DestroyImmediate(temp);

        GameObject[] rootObjects = ddolScene.GetRootGameObjects();
        foreach (GameObject obj in rootObjects)
        {
            if (!obj.TryGetComponent<InputManager>(out _))
                Destroy(obj);
        }
    }
}
