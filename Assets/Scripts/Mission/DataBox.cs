using UnityEngine;

/// <summary>
/// Tag component placed on any cube that represents a variable.
/// Read by MemoryCell and ConsoleScreen to know what value is stored.
/// </summary>
public class DataBox : MonoBehaviour
{
    public string variableName = "nombre";
    public string value        = "0";
    public string typeName     = "int";
}
