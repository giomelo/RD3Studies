using UnityEngine;

namespace _RD3.ExposedFields
{
    [ExposedObject]
    public class GameManager : MonoBehaviour
    {
        [ExposedField("LOL")]
        public int lol = 20;
        [ExposedField("LOL2")]
        public int lol2 = 30;
    }
}
