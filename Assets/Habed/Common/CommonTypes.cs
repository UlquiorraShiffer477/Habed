using System.Threading.Tasks;
using UnityEngine;

namespace Habed
{
    #region Callbacks Delegate Types

    public delegate void VoidCallback();

    public delegate void IntegerCallback(int value);

    public delegate void StringCallback(string str);

    public delegate void BooleanCallback(bool value);

    public delegate void LoadTextureCallback(Texture2D texture, string path);
    // public delegate void RemoteConfigInfoCallback(global::Firebase.RemoteConfig.ConfigInfo info);

    #endregion
}