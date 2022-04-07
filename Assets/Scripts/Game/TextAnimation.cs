using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class TextAnimation : MonoBehaviour
    {
        private int _index;
        private Text _text;
        [SerializeField] private string add;
        [SerializeField] private string original;
        [SerializeField] private int startIndex;

        private void Awake()
        {
            _text = gameObject.GetComponent<Text>();
            _index = startIndex;
        }

        public void Start()
        {
            _index = startIndex;
            if(_text.text.StartsWith(original) && gameObject.activeInHierarchy) StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            var text = _text.text;
            while (text.StartsWith(original))
            {
                text = _text.text.Substring(0, _index);
                if(!text.StartsWith(original)) StopCoroutine(Animate());
                if (text.Length - startIndex < add.Length)
                {
                    text += add[text.Length - startIndex];
                    _index += 1;
                }
                else
                {
                    text = _text.text.Substring(0, startIndex);
                    _index = startIndex;
                }
                
                _text.text = text;
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}