using UnityEngine;
using System.Collections;

/// <summary>
/// 활 아이템 - 좌클릭 시 가장 가까운 적에게 화살 발사
/// - 시속 160km (44.44 m/s)로 화살이 날아감
/// - 히트스캔 방식으로 무조건 맞음
/// - BaseItem 상속으로 월드 드롭/줍기 지원
/// </summary>
public class BowItem : BaseItem, IWeapon {
    [Header("Bow Settings")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float range = 20f; // 최대 사거리
    [SerializeField] private float arrowSpeed = 44.44f; // 시속 160km = 44.44 m/s
    [SerializeField] private LayerMask enemyLayer;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform firePoint; // 화살 발사 위치
    
    [Header("Cooldown")]
    [SerializeField] private float cooldown = 1f;
    private float lastAttackTime = -999f;
    
    public void Attack(GameObject user) {
        // 쿨타임 체크
        if (Time.time - lastAttackTime < cooldown) {
            Debug.Log($"[BowItem] 쿨타임 중... ({cooldown - (Time.time - lastAttackTime):F1}초 남음)");
            return;
        }
        
        // 가장 가까운 적 찾기
        Transform closestEnemy = FindClosestEnemy(user.transform.position);
        if (closestEnemy == null) {
            Debug.Log("[BowItem] 범위 내에 적이 없습니다.");
            return;
        }
        
        // 적을 향해 회전
        Vector3 direction = (closestEnemy.position - user.transform.position).normalized;
        direction.y = 0; // 수평 방향만
        user.transform.rotation = Quaternion.LookRotation(direction);
        
        // 화살 발사
        lastAttackTime = Time.time;
        StartCoroutine(ShootArrow(user, closestEnemy));
        
        Debug.Log($"[BowItem] {closestEnemy.name}에게 화살 발사!");
    }
    
    /// <summary>
    /// 가장 가까운 적 찾기
    /// </summary>
    private Transform FindClosestEnemy(Vector3 position) {
        Collider[] enemies = Physics.OverlapSphere(position, range, enemyLayer);
        
        Transform closest = null;
        float closestDistance = float.MaxValue;
        
        foreach (Collider enemy in enemies) {
            float distance = Vector3.Distance(position, enemy.transform.position);
            if (distance < closestDistance) {
                closestDistance = distance;
                closest = enemy.transform;
            }
        }
        
        return closest;
    }
    
    /// <summary>
    /// 화살 발사 코루틴
    /// </summary>
    private IEnumerator ShootArrow(GameObject user, Transform target) {
        // 발사 위치 결정
        Vector3 startPos = firePoint != null ? firePoint.position : user.transform.position + Vector3.up * 1.5f;
        Vector3 targetPos = target.position + Vector3.up * 1f; // 적의 중심 높이
        
        float distance = Vector3.Distance(startPos, targetPos);
        float travelTime = distance / arrowSpeed;
        
        // 화살 시각 효과 (프리팹이 있는 경우)
        GameObject arrowVisual = null;
        if (arrowPrefab != null) {
            arrowVisual = Instantiate(arrowPrefab, startPos, Quaternion.LookRotation(targetPos - startPos));
        }
        
        float elapsed = 0f;
        
        // 화살이 날아가는 동안
        while (elapsed < travelTime) {
            elapsed += Time.deltaTime;
            float t = elapsed / travelTime;
            
            if (arrowVisual != null) {
                // 타겟이 이동해도 따라감 (유도 효과)
                Vector3 currentTargetPos = target != null ? target.position + Vector3.up * 1f : targetPos;
                arrowVisual.transform.position = Vector3.Lerp(startPos, currentTargetPos, t);
                arrowVisual.transform.LookAt(currentTargetPos);
            }
            
            yield return null;
        }
        
        // 화살 시각 효과 제거
        if (arrowVisual != null) {
            Destroy(arrowVisual);
        }
        
        // 데미지 적용 (히트스캔 - 무조건 맞음)
        if (target != null) {
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null) {
                damageable.TakeDamage(damage);
                Debug.Log($"[BowItem] {target.name}에게 {damage} 데미지!");
            }
        }
    }
    
    public float GetDamage() => damage;
    public float GetRange() => range;
    public float GetCooldown() => cooldown;
}

