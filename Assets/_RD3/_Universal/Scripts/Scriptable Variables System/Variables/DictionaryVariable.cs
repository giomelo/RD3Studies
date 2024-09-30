using System.Collections.Generic;
using UnityEngine;

namespace Refactor.Data.Variables.Variables
{
    [CreateAssetMenu(fileName = "Dictionary", menuName = "RD3/Variables/Dic", order = 1)]
    public class DictionaryVariable<TKey, TValue> : Variable<Dictionary<TKey, TValue>>
    {
        // Adicione métodos específicos, se necessário, para trabalhar com dicionários
    }
}