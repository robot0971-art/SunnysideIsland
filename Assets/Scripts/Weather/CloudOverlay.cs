using UnityEngine;
using UnityEngine.UI;

namespace SunnysideIsland.Weather
{
    /// <summary>
    /// 구름 날씨 시 화면을 어둡게 하는 오버레이
    /// </summary>
    public class CloudOverlay : MonoBehaviour
    {
        [SerializeField] private Image _overlayImage;
        [SerializeField] private float _fadeSpeed = 1f;
        [SerializeField] private Color _cloudyColor = new Color(0f, 0f, 0f, 0.3f);
        [SerializeField] private Color _rainyColor = new Color(0f, 0f, 0f, 0.4f);
        
        private bool _isCloudy;
        private float _targetAlpha;
        private CanvasGroup _canvasGroup;
        
        private void Awake()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }
            
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            _canvasGroup.alpha = 0f;
        }
        
        private void Update()
        {
            float currentAlpha = _canvasGroup.alpha;
            float targetAlpha = _isCloudy ? _targetAlpha : 0f;
            
            if (Mathf.Abs(currentAlpha - targetAlpha) > 0.01f)
            {
                _canvasGroup.alpha = Mathf.Lerp(currentAlpha, targetAlpha, _fadeSpeed * Time.deltaTime);
            }
            else
            {
                _canvasGroup.alpha = targetAlpha;
            }
        }
        
        public void SetCloudy(bool isCloudy)
        {
            _isCloudy = isCloudy;
            _targetAlpha = isCloudy ? _cloudyColor.a : 0f;
            
            if (_overlayImage != null)
            {
                _overlayImage.color = isCloudy ? _cloudyColor : Color.clear;
            }
        }
        
        public void SetRainy()
        {
            _isCloudy = true;
            _targetAlpha = _rainyColor.a;
            
            if (_overlayImage != null)
            {
                _overlayImage.color = _rainyColor;
            }
        }
    }
}
