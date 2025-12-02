/*using UnityEngine;

public class IsEnabled : MonoBehaviour
{
    [Header("Настройки кубика")]
    public int cubeID; // 1 для Cube1, 2 для Cube2 и т.д.

    [Header("Пороги открытия")]
    public int[] unlockScores = { 0, 10, 20, 30, 40, 50, 60, 80, 90, 100 };

    [Header("Материалы кубиков")]
    public Material[] cubeMaterials;

    private void Start()
    {
        int playerScore = PlayerPrefs.GetInt("score");

        // Проверяем, открыт ли этот конкретный кубик
        if (cubeID >= 0 && cubeID < unlockScores.Length && playerScore >= unlockScores[cubeID])
        {
            // Кубик открыт - используем его нормальный материал
            if (cubeID < cubeMaterials.Length)
                GetComponent<MeshRenderer>().material = cubeMaterials[cubeID];
        }
        else
        {
            // Кубик закрыт - можно сделать серым или полупрозрачным
            GetComponent<MeshRenderer>().material = cubeMaterials[0]; // или специальный материал для закрытых
        }
    }
}
*/
using UnityEngine;
public class IsEnabled : MonoBehaviour
{
    public int needToUnlock;
    public Material CloseMaterial;
    private void Start()
    {
        if (PlayerPrefs.GetInt("score") < needToUnlock)
            GetComponent<MeshRenderer>().material = CloseMaterial;
    }
}
