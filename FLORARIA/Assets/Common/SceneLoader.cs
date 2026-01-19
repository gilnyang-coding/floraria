using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 전환을 담당하는 유틸리티 클래스
/// </summary>
public class SceneLoader : MonoBehaviour {
    
    /// <summary>
    /// 씬 이름으로 씬 로드
    /// </summary>
    /// <param name="sceneName">로드할 씬 이름</param>
    public void LoadScene(string sceneName) {
        if (string.IsNullOrEmpty(sceneName)) {
            Debug.LogError("[SceneLoader] 씬 이름이 비어있습니다.");
            return;
        }
        
        Debug.Log($"[SceneLoader] {sceneName} 씬으로 전환합니다.");
        SceneManager.LoadScene(sceneName);
    }
    
    /// <summary>
    /// 씬 인덱스로 씬 로드
    /// </summary>
    /// <param name="sceneIndex">로드할 씬 인덱스 (Build Settings에서 확인)</param>
    public void LoadScene(int sceneIndex) {
        if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings) {
            Debug.LogError($"[SceneLoader] 유효하지 않은 씬 인덱스: {sceneIndex}");
            return;
        }
        
        Debug.Log($"[SceneLoader] 씬 인덱스 {sceneIndex}로 전환합니다.");
        SceneManager.LoadScene(sceneIndex);
    }
}
