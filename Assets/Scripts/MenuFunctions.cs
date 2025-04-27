using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuFunctions : MonoBehaviour
{
    public DataMapper dataMapper;

    public void OnColorMenuItemSelected(int itemId)
    {
        // -1 = clear data binding
        //  0 = bind to variable #0
        //  1 = bind to variable #1
        //  etc.
        dataMapper.SetColorDataBinding(itemId - 1);
    }

    public void OnSizeMenuItemSelected(int itemId)
    {
        // -1 = clear data binding
        //  0 = bind to variable #0
        //  1 = bind to variable #1
        //  etc.
        dataMapper.SetSizeDataBinding(itemId - 1);
    }

}
