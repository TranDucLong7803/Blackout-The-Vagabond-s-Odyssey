using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDetails_", menuName = "Scriptable Objects/Enemy/EnemyDetails")]
public class EnemyDetailSO : ScriptableObject
{
    #region Header BASE ENEMY DETAILS
    [Space(10)]
    [Header("BASE ENEMY DETAILS")]
    #endregion

    #region Tooltip
    [Tooltip("The name of the enemy")]
    #endregion
    public string enemyName;

    #region Tooltip
    [Tooltip("The prefab for the enemy")]
    #endregion
    public GameObject enemyPrefab;

    #region Tooltip
    [Tooltip("Distance to the player before enemy starts chasing")]
    #endregion
    public float chaseDistance = 50f;


    #region Validation

#if UNITY_EDITOR
    // Validate the scriptable object details entered
    private void OnValidate()
    {
        Utility.ValidateCheckEmptyString(this, nameof(enemyName), enemyName);
        Utility.ValidateCheckNullValue(this, nameof(enemyPrefab), enemyPrefab);
        Utility.ValidateCheckPositiveValue(this, nameof(chaseDistance), chaseDistance, false);
    }

#endif
    #endregion
}
