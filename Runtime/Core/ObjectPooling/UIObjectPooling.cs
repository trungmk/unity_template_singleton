namespace Core
{
    public class UIObjectPooling : ObjectPooling
    {
        private static UIObjectPooling _thisinstance;
        
        public static UIObjectPooling ThisInstance 
        {
            get 
            {
                if (_thisinstance == null) 
                {
                    _thisinstance = FindFirstObjectByType<UIObjectPooling>();
                }
                
                return _thisinstance;
            }
        }

        private void Awake()
        {
            _thisinstance = this;
        }
    }
}

